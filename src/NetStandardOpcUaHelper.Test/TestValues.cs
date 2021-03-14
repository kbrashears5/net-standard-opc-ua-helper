using Opc.Ua;
using Opc.Ua.Server;
using System.IO;

namespace NetStandardOpcUaHelper.Test
{
    /// <summary>
    /// Static text for the <see cref="NetStandardOpcUaHelper"/> namespace
    /// </summary>
    internal static class TestValues
    {
        #region Client

        internal static OpcUaClientHelper OpcUaClientHelper { get; } = new OpcUaClientHelper();

        internal static OpcUaClientHelper OpcUaClientHelper_Null { get; }

        internal static string FilePath { get; } = @"C:\temp\file.xml";

        internal static string ClientName { get; } = nameof(ClientName);

        internal static string SessionName { get; } = nameof(SessionName);

        internal static uint SessionTimeout { get; } = 7;

        internal static string OpcUaUrl { get; } = "opc.tcp://localhost:4840";

        internal static string NodeId { get; } = nameof(NodeId);

        #endregion Client

        #region Server

        internal static StandardServer Server { get; } = new StandardServer();

        internal static StandardServer Server_Null { get; }

        internal static int Port { get; } = 4840;

        internal static bool StaticAttribute { get; } = true;

        internal static OpcUaServerHelper OpcUaServerHelper { get; } = new OpcUaServerHelper(server: Server,
            port: Port);

        internal static OpcUaServerHelper OpcUaServerHelper_Null { get; }

        internal static string ObjectName { get; } = nameof(ObjectName);

        internal static string GetPath() => $@"{Directory.GetCurrentDirectory()}\{nameof(OpcUaServerHelper)}.Config.xml";

        internal static ReferenceDescription ReferenceDescription { get; } = new ReferenceDescription();

        #endregion Server
    }
}