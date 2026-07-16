namespace Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static string GetMessageBrokerProvider(this IConfiguration configuration)
    {
        return configuration["MessageBroker:Provider"]
               ?? configuration["MessageBroker__Provider"]
               ?? "RabbitMQ";
    }

    public static bool IsKafka(this IConfiguration configuration)
    {
        var provider = configuration.GetMessageBrokerProvider();
        return provider.Equals("Kafka", StringComparison.OrdinalIgnoreCase);
    }
}