using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;

namespace NetStandardOpcUaHelper
{
    internal class NodeManager : CustomNodeManager2
    {
        /// <summary>
        /// Create new instance of <see cref="NodeManager"/>
        /// </summary>
        /// <param name="server"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NodeManager(IServerInternal server,
            ApplicationConfiguration configuration) : base(server: server, configuration: configuration)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            this.SystemContext.NodeIdFactory = this;
        }

        /// <summary>
        /// Add predefined node
        /// </summary>
        /// <param name="context"></param>
        /// <param name="node"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddPredefinedNodeToNodeManager(ISystemContext context,
            NodeState node)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (node == null) throw new ArgumentNullException(nameof(node));

            this.AddPredefinedNode(context: context,
                node: node);
        }

        /// <summary>
        /// Add root notifier
        /// </summary>
        /// <param name="notifier"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddRootNotifierToNodeManager(NodeState notifier)
        {
            if (notifier == null) throw new ArgumentNullException(nameof(notifier));

            this.AddRootNotifier(notifier: notifier);
        }

        /// <summary>
        /// Does any initialization required before the address space can be used.
        /// </summary>
        /// <remarks>
        /// The externalReferences is an out parameter that allows the node manager to link to nodes
        /// in other node managers. For example, the 'Objects' node is managed by the CoreNodeManager and
        /// should have a reference to the root folder node(s) exposed by this node manager.
        /// </remarks>
        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (this.Lock)
            {
                if (!externalReferences.TryGetValue(key: ObjectIds.ObjectsFolder, value: out var references))
                    externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();

                var folder = new FolderState(parent: null)
                {
                    SymbolicName = nameof(OpcUaServerHelper),
                    ReferenceTypeId = ReferenceTypes.Organizes,
                    TypeDefinitionId = ObjectTypeIds.FolderType,
                    NodeId = new NodeId(nameof(OpcUaServerHelper), this.NamespaceIndex),
                    BrowseName = new QualifiedName(nameof(OpcUaServerHelper), this.NamespaceIndex),
                    WriteMask = AttributeWriteMask.None,
                    UserWriteMask = AttributeWriteMask.None,
                    EventNotifier = EventNotifiers.None,
                    DisplayName = new LocalizedText(Text.Locale, nameof(OpcUaServerHelper))
                };

                folder.AddReference(referenceTypeId: ReferenceTypes.Organizes,
                    isInverse: true,
                    targetId: ObjectIds.ObjectsFolder);

                references.Add(new NodeStateReference(referenceTypeId: ReferenceTypes.Organizes,
                    isInverse: false,
                    targetId: folder.NodeId));

                folder.EventNotifier = EventNotifiers.SubscribeToEvents;
            }
        }
    }
}