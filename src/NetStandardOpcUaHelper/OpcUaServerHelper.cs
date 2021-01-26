using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NetStandardOpcUaHelper
{
    /// <summary>
    /// OPC UA Server Helper
    /// </summary>
    public class OpcUaServerHelper : IDisposable
    {
        /// <summary>
        /// Node Manager
        /// </summary>
        private NodeManager NodeManager { get; set; }

        /// <summary>
        /// OPC UA Server
        /// </summary>
        private StandardServer Server { get; }

        /// <summary>
        /// Server Port
        /// </summary>
        private int Port { get; }

        /// <summary>
        /// Create new instance of <see cref="OpcUaServerHelper"/>
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public OpcUaServerHelper(StandardServer server,
            int port)
        {
            this.Server = server ?? throw new ArgumentNullException(nameof(server));

            this.Port = port == 0 ? throw new ArgumentException(nameof(port)) : port;
        }

        #region IDisposable

        /// <summary>
        /// Disposed
        /// </summary>
        private bool Disposed { get; set; } = false;

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                    this.NodeManager?.Dispose();

                    if (this.Server != null)
                    {
                        if (this.Server.CurrentInstance.IsRunning)
                            this.Server.Stop();

                        this.Server.Dispose();
                    }
                }

                this.Disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~OpcUaServerHelper() => this.Dispose(disposing: false);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        /// <summary>
        /// Add predefined node
        /// </summary>
        /// <param name="rootNode"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddPredefinedNode(FolderState rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));

            this.VerifyNodeManagerCreated();

            this.NodeManager.AddPredefinedNodeToNodeManager(context: this.NodeManager.SystemContext,
                node: rootNode);
        }

        /// <summary>
        /// Get the <see cref="BuiltInType"/> of a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BuiltInType ConvertCSharpTypeToOpcUaType(ref object value)
        {
            switch (value)
            {
                case bool _:
                    return BuiltInType.Boolean;

                case byte _:
                    return BuiltInType.Byte;

                case DateTime _:
                    return BuiltInType.DateTime;

                case double _:
                    return BuiltInType.Double;

                case Enum _:
                    return BuiltInType.Enumeration;

                case float _:
                    return BuiltInType.Float;

                case Guid _:
                    return BuiltInType.Guid;

                case short _:
                    return BuiltInType.Int16;

                // this should also take the case int/number to return BuiltInType.Integer/BuiltInType.Number respectively
                case int _:
                    return BuiltInType.Int32;

                case long _:
                    return BuiltInType.Int64;

                case sbyte _:
                    return BuiltInType.SByte;

                case string _:
                    return BuiltInType.String;

                case string[] _:
                    var temp = (string[])value;

                    value = string.Join(separator: ",", value: temp);

                    return BuiltInType.String;

                case ushort _:
                    return BuiltInType.UInt16;

                // this should also take the case uint to return BuiltInType.UInteger
                case uint _:
                    return BuiltInType.UInt32;

                case ulong _:
                    return BuiltInType.UInt64;

                default:
                    return BuiltInType.Null;
            }
        }

        /// <summary>
        /// Create the root folder on the target OPC server
        /// </summary>
        /// <returns></returns>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        public FolderState CreateFolder(NodeState parent,
            string path,
            string name)
        {
            this.VerifyNodeManagerCreated();

            var folder = new FolderState(parent: parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId(value: path, namespaceIndex: this.NodeManager.NamespaceIndex),
                BrowseName = new QualifiedName(path, this.NodeManager.NamespaceIndex),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None,
                Description = new LocalizedText(string.Empty, string.Empty),
                RolePermissions = new RolePermissionTypeCollection(),
                UserRolePermissions = new RolePermissionTypeCollection(),
                DisplayName = new LocalizedText(Text.Locale, name),
            };

            if (parent != null)
                parent.AddChild(child: folder);

            folder.EventNotifier = EventNotifiers.SubscribeToEvents;

            this.NodeManager.AddRootNotifierToNodeManager(notifier: folder);

            return folder;
        }

        /// <summary>
        /// Create object
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public BaseObjectState CreateObject(BaseObjectState parent,
            ReferenceDescription sourceObject)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (sourceObject == null) throw new ArgumentNullException(nameof(sourceObject));

            this.VerifyNodeManagerCreated();

            var path = sourceObject.NodeId.Identifier.ToString();

            var name = sourceObject.DisplayName.Text;

            var baseObject = new BaseObjectState(parent: parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = ObjectTypeIds.BaseObjectType,
                NodeId = new NodeId(value: path, namespaceIndex: this.NodeManager.NamespaceIndex),
                BrowseName = new QualifiedName(name, this.NodeManager.NamespaceIndex),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None,
                Description = new LocalizedText(string.Empty, string.Empty),
                RolePermissions = new RolePermissionTypeCollection(),
                UserRolePermissions = new RolePermissionTypeCollection(),
            };

            baseObject.DisplayName = baseObject.BrowseName.Name;

            if (parent != null)
                parent.AddChild(child: baseObject);

            return baseObject;
        }

        /// <summary>
        /// Create variable
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sourceObject"></param>
        /// <param name="value"></param>
        /// <param name="staticAttribute"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BaseDataVariableState CreateVariable(BaseObjectState parent,
            ReferenceDescription sourceObject,
            DataValue value,
            bool staticAttribute = false)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (sourceObject == null) throw new ArgumentNullException(nameof(sourceObject));

            this.VerifyNodeManagerCreated();

            var dataValue = value?.Value;

            var dataType = this.ConvertCSharpTypeToOpcUaType(ref dataValue);

            value.Value = dataValue;

            var name = sourceObject.DisplayName.Text;

            var path = sourceObject.NodeId.Identifier.ToString();

            var nodeId = new NodeId(value: path, namespaceIndex: this.NodeManager.NamespaceIndex);

            // if doesn't already exist
            if (this.NodeManager.Find(nodeId: nodeId) == null)
            {
                var variable = new BaseDataVariableState(parent: parent)
                {
                    SymbolicName = name,
                    ReferenceTypeId = ReferenceTypes.Organizes,
                    TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                    NodeId = nodeId,
                    BrowseName = new QualifiedName(path, this.NodeManager.NamespaceIndex),
                    DisplayName = new LocalizedText(Text.Locale, name),
                    ValueRank = ValueRanks.Scalar,
                    AccessLevel = AccessLevels.CurrentReadOrWrite,
                    UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                    Historizing = false,
                    StatusCode = value.StatusCode,
                    Timestamp = DateTime.UtcNow,
                    Description = new LocalizedText(string.Empty, string.Empty),
                    UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                    WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                    DataType = (uint)dataType,
                    AccessRestrictions = AccessRestrictionType.None,
                    RolePermissions = new RolePermissionTypeCollection(),
                    UserRolePermissions = new RolePermissionTypeCollection(),
                    CopyPolicy = VariableCopyPolicy.Always,
                    Value = value,
                };

                if (staticAttribute)
                    variable.TypeDefinitionId = VariableTypeIds.PropertyType;

                if (parent != null)
                    parent.AddChild(child: variable);

                return variable;
            }
            // if does exist, just add reference back to itself instead of recreating it and changing the hidden id
            else
            {
                if (parent != null)
                    parent.AddReference(referenceTypeId: ReferenceTypeIds.HasComponent,
                        isInverse: false,
                        targetId: nodeId);

                return null;
            }
        }

        /// <summary>
        /// Delete node
        /// </summary>
        /// <param name="nodeId"></param>
        public void DeleteNode(NodeId nodeId)
        {
            if (nodeId == null) throw new ArgumentNullException(nameof(nodeId));

            this.VerifyNodeManagerCreated();

            _ = this.NodeManager.DeleteNode(context: this.NodeManager.SystemContext,
                nodeId: nodeId);
        }

        /// <summary>
        /// Delete node
        /// </summary>
        /// <param name="nodeIdIdentifier"></param>
        /// <param name="namespaceIndex"></param>
        public void DeleteNode(string nodeIdIdentifier,
            ushort namespaceIndex = 2)
        {
            if (string.IsNullOrWhiteSpace(nodeIdIdentifier)) throw new ArgumentNullException(nameof(nodeIdIdentifier));

            var nodeId = new NodeId(value: nodeIdIdentifier,
                namespaceIndex: namespaceIndex);

            this.DeleteNode(nodeId: nodeId);
        }

        /// <summary>
        /// Initialize OPC UA server
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task InitializeOpcUaServer(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath)) throw new ArgumentNullException(nameof(configPath));

            var application = new ApplicationInstance
            {
                ApplicationType = ApplicationType.Server,
                ConfigSectionName = nameof(OpcUaServerHelper)
            };

            // load the application configuration
            var applicationConfig = await application.LoadApplicationConfiguration(filePath: configPath,
                silent: false);

            // check the application certificate
            var validCertificate = await application.CheckApplicationInstanceCertificate(silent: false,
                minimumKeySize: 0);

            if (!validCertificate) throw new Exception(Text.InvalidCertificate);

            // start the server
            await application.Start(server: this.Server);

            // select NodeManager from all available mangers
            this.NodeManager = (NodeManager)this.Server.CurrentInstance.NodeManager.NodeManagers
                .Where(nm => nm.GetType() == typeof(NodeManager))
                .FirstOrDefault();
        }

        /// <summary>
        /// Verify that session is not null
        /// </summary>
        /// <exception cref="NodeManagerNotCreatedException"></exception>
        private void VerifyNodeManagerCreated()
        {
            if (this.NodeManager == null) throw new NodeManagerNotCreatedException();
        }
    }
}