namespace AzureHybridConnectionProxy.Core
{
    public sealed class HCOptions
    {
        public static string Name = "AZHybridConnectionOptions";

        public string RelayNamespace { get; set; } = default!;
        public string RelayConnectionName { get; set; } = default!;
        public string KeyName { get; set; } = default!;
        public string Key { get; set; } = default!;
        public string ForwaredHost { get; set; } = default!;
    }
}
