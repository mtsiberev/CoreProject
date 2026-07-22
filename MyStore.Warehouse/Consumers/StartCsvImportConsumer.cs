using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Contracts.Events;
using MyStore.Warehouse.Data;
using System.Buffers;
using System.Buffers.Text;
using System.IO.Pipelines;

namespace MyStore.Warehouse.Consumers;

public class StartCsvImportConsumer(WarehouseDbContext db, IConfiguration configuration, ILogger<StartCsvImportConsumer> logger)
    : IConsumer<StartCsvImport>
{
    private static readonly byte NewLine = (byte)'\n';
    private static readonly byte CarriageReturn = (byte)'\r';
    private static readonly byte Comma = (byte)',';
    private const int SafeMaxCapacity = 1024;

    public async Task Consume(ConsumeContext<StartCsvImport> context)
    {
        var ct = context.CancellationToken;
        var filePath = configuration["StocksImport:FilePath"] ?? "/app/data/stocks.csv";

        logger.LogInformation("Recieved command to start CSV import. File path: {Path}", filePath);

        if (!File.Exists(filePath))
        {
            logger.LogError("File not found: {Path}", filePath);
            return;
        }

        Guid[] rentedIds = ArrayPool<Guid>.Shared.Rent(SafeMaxCapacity);
        int[] rentedQuantities = ArrayPool<int>.Shared.Rent(SafeMaxCapacity);

        Array.Clear(rentedIds, 0, SafeMaxCapacity);
        Array.Clear(rentedQuantities, 0, SafeMaxCapacity);

        int uniqueCount = 0;

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        var reader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: false));
        bool isHeader = true;

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync(ct);
                var buffer = result.Buffer;

                while (TryReadLine(ref buffer, out var line))
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    int lineLength = (int)line.Length;
                    byte[] lineBuffer = ArrayPool<byte>.Shared.Rent(lineLength);

                    try
                    {
                        line.CopyTo(lineBuffer);
                        ReadOnlySpan<byte> lineSpan = lineBuffer.AsSpan(0, lineLength);

                        if (TryParseCsvLine(lineSpan, out Guid productId, out int quantity))
                        {
                            int index = rentedIds.AsSpan(0, uniqueCount).IndexOf(productId);

                            if (index >= 0)
                            {
                                rentedQuantities[index] += quantity;
                            }
                            else if (uniqueCount < SafeMaxCapacity)
                            {
                                rentedIds[uniqueCount] = productId;
                                rentedQuantities[uniqueCount] = quantity;
                                uniqueCount++;
                            }
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(lineBuffer);
                    }
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted) break;
            }
            await reader.CompleteAsync();

            if (uniqueCount == 0)
            {
                logger.LogWarning("CSV file is incorrect.");
                return;
            }

            Array.Sort(rentedIds, rentedQuantities, 0, uniqueCount);

            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                for (int i = 0; i < uniqueCount; i++)
                {
                    var currentProductId = rentedIds[i];
                    var additionalQuantity = rentedQuantities[i];

                    long productLockKey = BitConverter.ToInt64(currentProductId.ToByteArray(), 0);
                    await db.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock({0});",
                        new object[] { productLockKey },
                        ct
                    );

                    var rowsAffected = await db.Database.ExecuteSqlRawAsync(
                        @"UPDATE warehouse.""Stocks"" 
                        SET ""Quantity"" = ""Quantity"" + {1} 
                        WHERE ""ProductId"" = {0};",
                        new object[] { currentProductId, additionalQuantity },
                        ct
                    );

                    if (rowsAffected == 0)
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            @"INSERT INTO warehouse.""Stocks"" (""Id"", ""ProductId"", ""Quantity"") 
                            VALUES ({2}, {0}, {1});",
                            new object[] { currentProductId, additionalQuantity, Guid.NewGuid() },
                            ct
                        );
                    }
                }

                await transaction.CommitAsync(ct);
                logger.LogInformation("CSV import successfully completed. Unique products processed: {Count}.", uniqueCount);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                logger.LogError(ex, "Transaction Error.");
            }
        }
        finally
        {
            ArrayPool<Guid>.Shared.Return(rentedIds);
            ArrayPool<int>.Shared.Return(rentedQuantities);
        }
    }

    private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        var position = buffer.PositionOf(NewLine);
        if (position == null)
        {
            line = default;
            return false;
        }
        line = buffer.Slice(0, position.Value);
        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
        return true;
    }

    private static bool TryParseCsvLine(ReadOnlySpan<byte> line, out Guid productId, out int quantity)
    {
        productId = Guid.Empty;
        quantity = 0;

        if (line.Length > 0 && line[^1] == CarriageReturn) line = line[..^1];
        if (line.IsEmpty) return false;

        int commaIndex = line.IndexOf(Comma);
        if (commaIndex == -1) return false;

        if (!Utf8Parser.TryParse(line[..commaIndex], out productId, out _)) return false;
        if (!Utf8Parser.TryParse(line[(commaIndex + 1)..], out quantity, out _)) return false;

        return true;
    }
}
