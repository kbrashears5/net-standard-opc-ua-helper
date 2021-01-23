using NetStandardTestHelper.Xunit;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using Xunit;

namespace NetStandardOpcUaHelper.Test
{
    /// <summary>
    /// Test the <see cref="OpcUaClientHelper"/> class
    /// </summary>
    public class OpcUaClientHelper_Tests
    {
        #region Constructor

        /// <summary>
        /// Test that constructor is created successfully
        /// </summary>
        [Fact]
        public void Constructor()
        {
            var helper = TestValues.OpcUaClientHelper;

            Assert.NotNull(helper);
        }

        #endregion Constructor

        #region BrowseOpcUaNode

        /// <summary>
        /// Test that function throws for null nodeid
        /// </summary>
        [Fact]
        public void BrowseOpcNode_Null_NodeId()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.BrowseOpcUaNode(nodeId: null));

            TestHelper.AssertArgumentNullException(ex,
                "nodeId");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void BrowseOpcNode_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.BrowseOpcUaNode(nodeId: new Opc.Ua.NodeId("name")));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        /// <summary>
        /// Test that function throws for null nodeid
        /// </summary>
        [Fact]
        public void BrowseOpcNode_Overloaded_Null_NodeId()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.BrowseOpcUaNode(nodeId: null,
                sourceObjectChildren: out var _,
                continuationPoint: out var _));

            TestHelper.AssertArgumentNullException(ex,
                "nodeId");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void BrowseOpcNode_Overloaded_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.BrowseOpcUaNode(nodeId: new Opc.Ua.NodeId("name"),
                sourceObjectChildren: out var _,
                continuationPoint: out var _));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion BrowseOpcUaNode

        #region CleanupOpcUaSubscription

        /// <summary>
        /// Test that function throws for null subscription
        /// </summary>
        [Fact]
        public void CleanUpOpcUaSubscription_Null_Subscription()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CleanupOpcUaSubscription(subscription: null));

            TestHelper.AssertArgumentNullException(ex,
                "subscription");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void CleanUpOpcUaSubscription_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.CleanupOpcUaSubscription(subscription: new Subscription()));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion CleanupOpcUaSubscription

        #region CleanupOpcUaSubscriptions

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void CleanUpOpcUaSubscriptions_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.CleanupOpcUaSubscriptions());

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion CleanupOpcUaSubscriptions

        #region CreateOpcUaMonitoredItem

        /// <summary>
        /// Test that function throws exception for null opc node id
        /// </summary>
        [Fact]
        public void CreateMonitoredItem_Null_OpcNodeId()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaMonitoredItem(opcUaNodeId: NetStandardTestHelper.TestValues.StringEmpty,
                notification: null));

            TestHelper.AssertArgumentNullException(ex,
                "opcUaNodeId");
        }

        /// <summary>
        /// Test that function throws exception for null opc notification
        /// </summary>
        [Fact]
        public void CreateMonitoredItem_Null_Notification()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaMonitoredItem(opcUaNodeId: TestValues.NodeId,
                notification: null));

            TestHelper.AssertArgumentNullException(ex,
                "notification");
        }

        #endregion CreateOpcUaMonitoredItem

        #region CreateOpcUaSubscription

        /// <summary>
        /// Test that function throws for null monitored items
        /// </summary>
        [Fact]
        public void CreateSubscription_Null_MonitoredItems()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaSubscription(monitoredItems: null,
                publishingInterval: 100));

            TestHelper.AssertArgumentNullException(ex,
                "monitoredItems");
        }

        /// <summary>
        /// Test that function throws for null publishing interval being zero
        /// </summary>
        [Fact]
        public void CreateSubscription_Zero_PublishingInterval()
        {
            var ex = Assert.Throws<ArgumentException>(() => TestValues.OpcUaClientHelper.CreateOpcUaSubscription(monitoredItems: new List<MonitoredItem>(),
                publishingInterval: 0));

            TestHelper.AssertExceptionText(ex,
                "publishingInterval");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void CreateSubscription_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.CreateOpcUaSubscription(monitoredItems: new List<MonitoredItem>(),
                publishingInterval: 100));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion CreateOpcUaSubscription

        #region CreateOpcUaConfiguration

        /// <summary>
        /// Test that function throws for null client name
        /// </summary>
        [Fact]
        public async void CreateOpcConfiguration_Null_ClientName()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaConfiguration(clientName: NetStandardTestHelper.TestValues.StringEmpty,
                configPath: TestValues.FilePath));

            TestHelper.AssertArgumentNullException(ex,
                "clientName");
        }

        /// <summary>
        /// Test that function throws for null config path
        /// </summary>
        [Fact]
        public async void CreateOpcConfiguration_Null_ConfigPath()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaConfiguration(clientName: TestValues.ClientName,
                configPath: NetStandardTestHelper.TestValues.StringEmpty));

            TestHelper.AssertArgumentNullException(ex,
                "configPath");
        }

        #endregion CreateOpcUaConfiguration

        #region CreateOpcUaEndpoint

        /// <summary>
        /// Test that function throws for null opc url
        /// </summary>
        [Fact]
        public void CreateOpcEndpoint_Null_OpcUrl()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaEndpoint(opcUrl: NetStandardTestHelper.TestValues.StringEmpty));

            TestHelper.AssertArgumentNullException(ex,
                "opcUrl");
        }

        #endregion CreateOpcUaEndpoint

        #region CreateOpcUaSession

        /// <summary>
        /// Test that function throws for null application config
        /// </summary>
        [Fact]
        public async void CreateOpcUaSession_Null_ApplicationConfig()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaSession(applicationConfig: null,
                selectedEndpoint: new Opc.Ua.EndpointDescription(),
                sessionName: TestValues.SessionName,
                sessionTimeout: TestValues.SessionTimeout,
                keepAlive: null));

            TestHelper.AssertArgumentNullException(ex,
                "applicationConfig");
        }

        /// <summary>
        /// Test that function throws for null selected endpoint
        /// </summary>
        [Fact]
        public async void CreateOpcUaSession_Null_SelectedEndpoint()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaSession(applicationConfig: new Opc.Ua.ApplicationConfiguration(),
                selectedEndpoint: null,
                sessionName: TestValues.SessionName,
                sessionTimeout: TestValues.SessionTimeout,
                keepAlive: null));

            TestHelper.AssertArgumentNullException(ex,
                "selectedEndpoint");
        }

        /// <summary>
        /// Test that function throws for null session name
        /// </summary>
        [Fact]
        public async void CreateOpcUaSession_Null_SessionName()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.CreateOpcUaSession(applicationConfig: new Opc.Ua.ApplicationConfiguration(),
                selectedEndpoint: new Opc.Ua.EndpointDescription(),
                sessionName: NetStandardTestHelper.TestValues.StringEmpty,
                sessionTimeout: TestValues.SessionTimeout,
                keepAlive: null));

            TestHelper.AssertArgumentNullException(ex,
                "sessionName");
        }

        /// <summary>
        /// Test that function throws for null session timeout
        /// </summary>
        [Fact]
        public async void CreateOpcUaSession_Null_SessionTimeout()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => TestValues.OpcUaClientHelper.CreateOpcUaSession(applicationConfig: new Opc.Ua.ApplicationConfiguration(),
                selectedEndpoint: new Opc.Ua.EndpointDescription(),
                sessionName: TestValues.SessionName,
                sessionTimeout: 0,
                keepAlive: null));

            TestHelper.AssertExceptionText(ex,
                "sessionTimeout");
        }

        #endregion CreateOpcUaSession

        #region DeleteOpcUaSubscription

        /// <summary>
        /// Test that function throws for null subscription
        /// </summary>
        [Fact]
        public void DeleteSubscription_Null_Subscription()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.DeleteOpcUaSubscription(subscription: null));

            TestHelper.AssertArgumentNullException(ex,
                "subscription");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void DeleteSubscription_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.DeleteOpcUaSubscription(subscription: new Subscription()));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion DeleteOpcUaSubscription

        #region DeleteOpcUaSubscriptions

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void DeleteSubscriptions_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.DeleteOpcUaSubscriptions());

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion DeleteOpcUaSubscriptions

        #region GetKeepAliveStopped

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void GetKeepAliveStopped_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.GetKeepAliveStopped());

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion GetKeepAliveStopped

        #region GetOpcUaNamespaceUris

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void GetNamespaceUris_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.GetOpcUaNamespaceUris());

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion GetOpcUaNamespaceUris

        #region GetOpcUaReferences

        /// <summary>
        /// Test that function throws for null nodeId
        /// </summary>
        [Fact]
        public void GetReferences_Null_NodeId()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.GetOpcUaReferences(nodeId: null));

            TestHelper.AssertArgumentNullException(ex,
                "nodeId");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void GetReferences_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.GetOpcUaReferences(nodeId: new Opc.Ua.NodeId("name")));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion GetOpcUaReferences

        #region InitializeOpcUaClientConnection

        /// <summary>
        /// Test that function throws for null client name
        /// </summary>
        [Fact]
        public async void InitializeClientConnection_Null_ClientName()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.InitializeOpcUaClientConnection(clientName: NetStandardTestHelper.TestValues.StringEmpty,
                configPath: TestValues.FilePath,
                opcUaUrl: TestValues.OpcUaUrl,
                sessionName: TestValues.SessionName,
                sessionTimeout: TestValues.SessionTimeout));

            TestHelper.AssertArgumentNullException(ex,
                "clientName");
        }

        /// <summary>
        /// Test that function throws for null config path
        /// </summary>
        [Fact]
        public async void InitializeClientConnection_Null_ConfigPath()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.InitializeOpcUaClientConnection(clientName: TestValues.ClientName,
                configPath: NetStandardTestHelper.TestValues.StringEmpty,
                opcUaUrl: TestValues.OpcUaUrl,
                sessionName: TestValues.SessionName,
                sessionTimeout: TestValues.SessionTimeout));

            TestHelper.AssertArgumentNullException(ex,
                "configPath");
        }

        /// <summary>
        /// Test that function throws for null OPC UA url
        /// </summary>
        [Fact]
        public async void InitializeClientConnection_Null_OpcUrl()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.InitializeOpcUaClientConnection(clientName: TestValues.ClientName,
                configPath: TestValues.FilePath,
                opcUaUrl: NetStandardTestHelper.TestValues.StringEmpty,
                sessionName: TestValues.SessionName,
                sessionTimeout: TestValues.SessionTimeout));

            TestHelper.AssertArgumentNullException(ex,
                "opcUaUrl");
        }

        /// <summary>
        /// Test that function throws for null session name
        /// </summary>
        [Fact]
        public async void InitializeClientConnection_Null_SessionName()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaClientHelper.InitializeOpcUaClientConnection(clientName: TestValues.ClientName,
                configPath: TestValues.FilePath,
                opcUaUrl: TestValues.OpcUaUrl,
                sessionName: NetStandardTestHelper.TestValues.StringEmpty,
                sessionTimeout: TestValues.SessionTimeout));

            TestHelper.AssertArgumentNullException(ex,
                "sessionName");
        }

        /// <summary>
        /// Test that function throws for zero session timeout
        /// </summary>
        [Fact]
        public async void InitializeClientConnection_Zero_SessionTimeout()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => TestValues.OpcUaClientHelper.InitializeOpcUaClientConnection(clientName: TestValues.ClientName,
                configPath: TestValues.FilePath,
                opcUaUrl: TestValues.OpcUaUrl,
                sessionName: TestValues.SessionName,
                sessionTimeout: 0));

            TestHelper.AssertExceptionText(ex,
                "sessionTimeout");
        }

        #endregion InitializeOpcUaClientConnection

        #region IsByteString

        /// <summary>
        /// Test that function throws for null sourceReferenceDescription
        /// </summary>
        [Fact]
        public void IsByteString_Null_SourceReferenceDescription()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.IsByteString(referenceDescription: null));

            TestHelper.AssertArgumentNullException(ex,
                "referenceDescription");
        }

        #endregion IsByteString

        #region ReadValue

        /// <summary>
        /// Test that function throws for null expandedNodeId
        /// </summary>
        [Fact]
        public void ReadValue_Null_ExpandedNodeId()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.ReadValue(expandedNodeId: null));

            TestHelper.AssertArgumentNullException(ex,
                "expandedNodeId");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void ReadValue_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.ReadValue(expandedNodeId: new Opc.Ua.ExpandedNodeId("name")));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion ReadValue

        #region RemoveMonitoredItemsFromSubscription

        /// <summary>
        /// Test that function throws for null subscription
        /// </summary>
        [Fact]
        public void RemoveMonitoredItemsFromSubscription_Null_Subscription()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.RemoveMonitoredItemsFromSubscription(subscription: null,
                monitoredItems: new List<MonitoredItem>()));

            TestHelper.AssertArgumentNullException(ex,
                "subscription");
        }

        /// <summary>
        /// Test that function throws for null monitored items
        /// </summary>
        [Fact]
        public void RemoveMonitoredItemsFromSubscription_Null_MonitoredItems()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaClientHelper.RemoveMonitoredItemsFromSubscription(subscription: new Subscription(),
                monitoredItems: null));

            TestHelper.AssertArgumentNullException(ex,
                "monitoredItems");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void RemoveMonitoredItemsFromSubscription_Not_Initialized()
        {
            var ex = Assert.Throws<SessionNotCreatedException>(() => TestValues.OpcUaClientHelper.RemoveMonitoredItemsFromSubscription(subscription: new Subscription(),
                monitoredItems: new List<MonitoredItem>()));

            TestHelper.AssertExceptionText(ex,
                Text.SessionNotCreated);
        }

        #endregion RemoveMonitoredItemsFromSubscription
    }
}