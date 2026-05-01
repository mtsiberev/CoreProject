using MyStore.Contracts.Common;

namespace MyStore.Contracts.Events;

public record StockReservationFailed(
    Guid OrderId, 
    string Reason
);