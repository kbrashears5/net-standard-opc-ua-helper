namespace NetStandardOpcUaHelper.Test
{
    /// <summary>
    /// Static text for the <see cref="NetStandardOpcUaHelper"/> namespace
    /// </summary>
    internal static class TestValues
    {
        internal static OpcUaClientHelper OpcUaClientHelper { get; } = new OpcUaClientHelper();

        internal static OpcUaClientHelper OpcUaClientHelper_Null { get; }

        internal static string FilePath { get; } = @"C:\temp\file.xml";

        internal static string ClientName { get; } = nameof(ClientName);

        internal static string SessionName { get; } = nameof(SessionName);

        internal static uint SessionTimeout { get; } = 7;

        internal static string OpcUaUrl { get; } = "opc.tcp://localhost:4840";

        internal static string NodeId { get; } = nameof(NodeId);
    }
}