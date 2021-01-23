using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetStandardOpcUaHelper
{
    /// <summary>
    /// OPC UA Client Helper
    /// </summary>
    public class OpcUaClientHelper : IDisposable
    {
        /// <summary>
        /// Write value collection
        /// </summary>
        public WriteValueCollection WriteValueCollection { get; }

        /// <summary>
        /// New OPC UA session was created
        /// </summary>
        public event EventHandler NewSession;

        /// <summary>
        /// OPC UA Session
        /// </summary>
        private Session Session { get; set; }

        /// <summary>
        /// User connecting to the OPC UA server
        /// </summary>
        private UserIdentity User { get; }

        /// <summary>
        /// Cancellation Token
        /// </summary>
        private CancellationTokenSource CancellationToken { get; }

        /// <summary>
        /// Reconnect Handler
        /// </summary>
        private SessionReconnectHandler ReconnectHandler { get; set; }

        /// <summary>
        /// Session Reconnect Timer
        /// </summary>
        private System.Timers.Timer ReconnectTimer { get; }

        /// <summary>
        /// Client name for the OPC UA session
        /// </summary>
        private string ClientName { get; set; }

        /// <summary>
        /// Config path for the OPC UA session
        /// </summary>
        private string ConfigPath { get; set; }

        /// <summary>
        /// OPC UA url for the OPC UA session
        /// </summary>
        private string OpcUaUrl { get; set; }

        /// <summary>
        /// Session name for the OPC UA session
        /// </summary>
        private string SessionName { get; set; }

        /// <summary>
        /// Session timeout for the OPC UA session
        /// </summary>
        private uint SessionTimeout { get; set; }

        /// <summary>
        /// Create new instance of <see cref="OpcUaClientHelper"/>
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public OpcUaClientHelper()
        {
            this.CancellationToken = new CancellationTokenSource();

            this.WriteValueCollection = new WriteValueCollection();

            this.User = new UserIdentity(token: new AnonymousIdentityToken());

            this.ReconnectTimer = new System.Timers.Timer(interval: 30000)
            {
                AutoReset = false,
            };

            this.ReconnectTimer.Elapsed += this.SessionReconnectComplete_Event;

            this.WriteAllValuesInCollection();
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
                    try
                    {
                        this.CancellationToken?.Cancel();

                        this.CancellationToken?.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                    }

                    this.ReconnectTimer?.Dispose();

                    this.CloseSession();

                    this.Session?.Dispose();
                }

                this.WriteValueCollection.Clear();

                this.Disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~OpcUaClientHelper() => this.Dispose(disposing: false);

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
        /// Browse an OPC UA node
        /// </summary>
        /// <param name="nodeId"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public ReferenceDescriptionCollection BrowseOpcUaNode(NodeId nodeId)
        {
            if (nodeId == null) throw new ArgumentNullException(nameof(nodeId));

            this.BrowseOpcUaNode(nodeId: nodeId,
                    sourceObjectChildren: out var sourceObjectChildren,
                    continuationPoint: out var continuationPoint);

            while (continuationPoint != null)
            {
                _ = this.Session.BrowseNext(requestHeader: null,
                    releaseContinuationPoint: false,
                    continuationPoint: continuationPoint,
                    revisedContinuationPoint: out continuationPoint,
                    references: out _);
            }

            return sourceObjectChildren;
        }

        /// <summary>
        /// Browse an OPC UA node
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="sourceObjectChildren"></param>
        /// <param name="continuationPoint"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public void BrowseOpcUaNode(NodeId nodeId,
            out ReferenceDescriptionCollection sourceObjectChildren,
            out byte[] continuationPoint)
        {
            if (nodeId == null) throw new ArgumentNullException(nameof(nodeId));

            this.VerifySessionCreated();

            _ = this.Session.Browse(requestHeader: null,
                view: null,
                nodeToBrowse: nodeId,
                maxResultsToReturn: 0u,
                browseDirection: BrowseDirection.Forward,
                referenceTypeId: ReferenceTypeIds.HierarchicalReferences,
                includeSubtypes: true,
                nodeClassMask: (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                continuationPoint: out continuationPoint,
                references: out sourceObjectChildren);
        }

        /// <summary>
        /// Remove monitored items that are not created on the subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <exception cref="ServiceResultException"></exception>
        public void CleanupOpcUaSubscription(Subscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            this.VerifySessionCreated();

            var monitoredItemsToRemove = subscription.MonitoredItems.Where(m => !m.Created);

            if (monitoredItemsToRemove == null) return;

            this.RemoveMonitoredItemsFromSubscription(subscription: subscription,
                monitoredItems: monitoredItemsToRemove);
        }

        /// <summary>
        /// Remove monitored items that are not created on the all subscriptions
        /// </summary>
        /// <exception cref="ServiceResultException"></exception>
        public void CleanupOpcUaSubscriptions()
        {
            this.VerifySessionCreated();

            _ = Task.Run(action: () => Parallel.ForEach(this.Session.Subscriptions.ToList(), subscription => this.CleanupOpcUaSubscription(subscription: subscription)),
                cancellationToken: this.CancellationToken.Token);
        }

        /// <summary>
        /// Close the OPC UA session
        /// </summary>
        /// <param name="deleteSubscriptions"></param>
        /// <exception cref="ServiceResultException"></exception>
        public void CloseSession(bool deleteSubscriptions = true)
        {
            if (this.Session != null && this.Session.Connected)
                _ = this.Session.CloseSession(requestHeader: null,
                    deleteSubscriptions: deleteSubscriptions);
        }

        /// <summary>
        /// Create monitored item
        /// </summary>
        /// <param name="opcUaNodeId"></param>
        /// <param name="notification"></param>
        /// <param name="namespaceIndex"></param>
        /// <returns></returns>
        public MonitoredItem CreateOpcUaMonitoredItem(string opcUaNodeId,
            MonitoredItemNotificationEventHandler notification,
            ushort namespaceIndex = 2)
        {
            if (string.IsNullOrWhiteSpace(opcUaNodeId)) throw new ArgumentNullException(nameof(opcUaNodeId));
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var monitoredItem = new MonitoredItem()
            {
                NodeClass = NodeClass.Variable,
                StartNodeId = new NodeId(value: opcUaNodeId, namespaceIndex: namespaceIndex),
            };

            // add notifications for monitored items
            monitoredItem.Notification += notification;

            return monitoredItem;
        }

        /// <summary>
        /// Create subscription
        /// </summary>
        /// <param name="monitoredItems"></param>
        /// <param name="publishingInterval"></param>
        /// <param name="prioritize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public Subscription CreateOpcUaSubscription(IEnumerable<MonitoredItem> monitoredItems,
            int publishingInterval,
            bool prioritize = false)
        {
            if (monitoredItems == null) throw new ArgumentNullException(nameof(monitoredItems));
            if (publishingInterval == 0) throw new ArgumentException(nameof(publishingInterval));

            this.VerifySessionCreated();

            if (monitoredItems.Count() == 0) return null;

            // create subscription
            var subscription = new Subscription(template: this.Session.DefaultSubscription)
            {
                PublishingInterval = publishingInterval,
                DisplayName = Guid.NewGuid().ToString(),
                Priority = prioritize ? Convert.ToByte(255) : Convert.ToByte(0)
            };

            // add monitored items
            subscription.AddItems(monitoredItems: monitoredItems);

            // add subscription to session
            _ = this.Session.AddSubscription(subscription: subscription);

            subscription.Create();

            return subscription;
        }

        /// <summary>
        /// Create OPC UA client configuration
        /// </summary>
        /// <returns></returns>
        /// <param name="clientName"></param>
        /// <param name="configPath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<ApplicationConfiguration> CreateOpcUaConfiguration(string clientName,
            string configPath)
        {
            if (string.IsNullOrWhiteSpace(clientName)) throw new ArgumentNullException(nameof(clientName));
            if (string.IsNullOrWhiteSpace(configPath)) throw new ArgumentNullException(nameof(configPath));

            var application = new ApplicationInstance
            {
                ApplicationName = clientName,
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = clientName,
            };

            var clientApplicationConfig = await application.LoadApplicationConfiguration(filePath: configPath,
                silent: false);

            var haveAppCertificate = await application.CheckApplicationInstanceCertificate(silent: false,
                minimumKeySize: 0);

            if (!haveAppCertificate) throw new Exception(Text.InvalidCertificate);

            clientApplicationConfig.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(certificate: clientApplicationConfig.SecurityConfiguration.ApplicationCertificate.Certificate);

            clientApplicationConfig.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(this.SessionAutoAcceptCertificate_Event);

            return clientApplicationConfig;
        }

        /// <summary>
        /// Create OPC UA Client Endpoint
        /// </summary>
        /// <returns></returns>
        /// <param name="opcUrl"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public EndpointDescription CreateOpcUaEndpoint(string opcUrl)
        {
            if (string.IsNullOrWhiteSpace(opcUrl)) throw new ArgumentNullException(nameof(opcUrl));

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(discoveryUrl: opcUrl,
                useSecurity: false,
                discoverTimeout: 15000);

            return selectedEndpoint;
        }

        /// <summary>
        /// Create OPC UA client session
        /// </summary>
        /// <param name="applicationConfig"></param>
        /// <param name="selectedEndpoint"></param>
        /// <param name="sessionName"></param>
        /// <param name="sessionTimeout"></param>
        /// <param name="keepAlive"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public async Task CreateOpcUaSession(ApplicationConfiguration applicationConfig,
            EndpointDescription selectedEndpoint,
            string sessionName,
            uint sessionTimeout,
            KeepAliveEventHandler keepAlive = null)
        {
            if (applicationConfig == null) throw new ArgumentNullException(nameof(applicationConfig));
            if (selectedEndpoint == null) throw new ArgumentNullException(nameof(selectedEndpoint));
            if (string.IsNullOrWhiteSpace(sessionName)) throw new ArgumentNullException(nameof(sessionName));
            this.SessionTimeout = sessionTimeout == 0 ? throw new ArgumentException(nameof(sessionTimeout)) : sessionTimeout;

            var endpointConfiguration = EndpointConfiguration.Create(applicationConfiguration: applicationConfig);

            var endpoint = new ConfiguredEndpoint(collection: null,
                description: selectedEndpoint,
                configuration: endpointConfiguration);

            this.Session = await Session.Create(configuration: applicationConfig,
                endpoint: endpoint,
                updateBeforeConnect: false,
                sessionName: sessionName,
                sessionTimeout: this.SessionTimeout,
                identity: this.User,
                preferredLocales: null);

            this.RegisterKeepAlive(keepAlive: keepAlive);
        }

        /// <summary>
        /// Create OPC UA client session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="keepAlive"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public void CreateOpcUaSession(Session session,
            KeepAliveEventHandler keepAlive = null)
        {
            this.Session = session ?? throw new ArgumentNullException(nameof(session));

            this.RegisterKeepAlive(keepAlive: keepAlive);
        }

        /// <summary>
        /// Delete OPC UA subscription
        /// </summary>
        /// <param name="subscription"></param>
        public void DeleteOpcUaSubscription(Subscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            this.VerifySessionCreated();

            _ = this.Session.RemoveSubscription(subscription: subscription);
        }

        /// <summary>
        /// Delete all the subscriptions from the OPC UA session
        /// </summary>
        /// <exception cref="ServiceResultException"></exception>
        public void DeleteOpcUaSubscriptions()
        {
            this.VerifySessionCreated();

            // enumerate to prevent collection was modified while enumerating exceptions
            _ = Task.Run(action: () => Parallel.ForEach(this.Session.Subscriptions.ToList(), subscription => this.DeleteOpcUaSubscription(subscription: subscription)),
                cancellationToken: this.CancellationToken.Token);
        }

        /// <summary>
        /// Returns whether or not the Session keep alive is stopped
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServiceResultException"></exception>
        public bool GetKeepAliveStopped()
        {
            this.VerifySessionCreated();

            return this.Session.KeepAliveStopped;
        }

        /// <summary>
        /// Returns the namespace URIs on the session
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServiceResultException"></exception>
        public NamespaceTable GetOpcUaNamespaceUris()
        {
            this.VerifySessionCreated();

            return this.Session.NamespaceUris;
        }

        /// <summary>
        /// Get the current OPC UA subscription count
        /// </summary>
        public int GetOpcUaSubscriptionCount()
        {
            this.VerifySessionCreated();

            var currentSubCountNodeId = new ExpandedNodeId(nodeId: new NodeId(namespaceIndex: 1, value: Text.SubCount));

            var value = this.ReadValue(expandedNodeId: currentSubCountNodeId);

            // couldn't read the value from the opc server - likely aren't connected
            return value == null
                ? int.MaxValue
                : Convert.ToInt32(value.Value);
        }

        /// <summary>
        /// Returns the references for a <see cref="NodeId"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public ReferenceDescriptionCollection GetOpcUaReferences(NodeId nodeId)
        {
            if (nodeId == null) throw new ArgumentNullException(nameof(nodeId));

            this.VerifySessionCreated();

            return this.Session.FetchReferences(nodeId: nodeId);
        }

        /// <summary>
        /// Initialize a new OPC UA client connection
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="configPath"></param>
        /// <param name="opcUaUrl"></param>
        /// <param name="sessionName"></param>
        /// <param name="sessionTimeout"></param>
        /// <param name="keepAlive"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public async Task InitializeOpcUaClientConnection(string clientName,
            string configPath,
            string opcUaUrl,
            string sessionName,
            uint sessionTimeout,
            KeepAliveEventHandler keepAlive = null)
        {
            this.ClientName = string.IsNullOrWhiteSpace(clientName) ? throw new ArgumentNullException(nameof(clientName)) : clientName;
            this.ConfigPath = string.IsNullOrWhiteSpace(configPath) ? throw new ArgumentNullException(nameof(configPath)) : configPath;
            this.OpcUaUrl = string.IsNullOrWhiteSpace(opcUaUrl) ? throw new ArgumentNullException(nameof(opcUaUrl)) : opcUaUrl;
            this.SessionName = string.IsNullOrWhiteSpace(sessionName) ? throw new ArgumentNullException(nameof(sessionName)) : sessionName;
            if (sessionTimeout == 0) throw new ArgumentException(nameof(sessionTimeout));

            var clientApplicationConfig = await this.CreateOpcUaConfiguration(clientName: clientName,
                configPath: configPath);

            var selectedEndpoint = this.CreateOpcUaEndpoint(opcUrl: opcUaUrl);

            await this.CreateOpcUaSession(applicationConfig: clientApplicationConfig,
                selectedEndpoint: selectedEndpoint,
                sessionName: sessionName,
                sessionTimeout: sessionTimeout,
                keepAlive: keepAlive);
        }

        /// <summary>
        /// Check to see if the <see cref="ReferenceDescription"/> is of type <see cref="BuiltInType.ByteString"/>
        /// </summary>
        /// <param name="referenceDescription"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public bool IsByteString(ReferenceDescription referenceDescription)
        {
            if (referenceDescription == null) throw new ArgumentNullException(nameof(referenceDescription));

            // get the node id
            var nodeId = ExpandedNodeId.ToNodeId(nodeId: referenceDescription.NodeId,
                namespaceTable: this.GetOpcUaNamespaceUris());

            // get the references
            var references = this.GetOpcUaReferences(nodeId: nodeId);

            // is NodeClass.Variable, has references > 0, references that are not forward are not > 0
            return referenceDescription.NodeClass == NodeClass.Variable &&
                references.Count() > 0 &&
                !(references.Where(r => !r.IsForward).Count() > 0);
        }

        /// <summary>
        /// Raise the <see cref="NewSession"/> event
        /// </summary>
        /// <param name="eventArgs"></param>
        private void NewSessionEvent(EventArgs eventArgs)
        {
            NewSession?.Invoke(sender: this, e: eventArgs);
        }

        /// <summary>
        /// Read a value from OPC UA server
        /// </summary>
        /// <param name="expandedNodeId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public DataValue ReadValue(ExpandedNodeId expandedNodeId)
        {
            if (expandedNodeId == null) throw new ArgumentNullException(nameof(expandedNodeId));

            this.VerifySessionCreated();

            return this.Session.ReadValue(nodeId: ExpandedNodeId.ToNodeId(nodeId: expandedNodeId, namespaceTable: this.GetOpcUaNamespaceUris()));
        }

        /// <summary>
        /// Register the keep alive
        /// </summary>
        /// <param name="keepAlive"></param>
        private void RegisterKeepAlive(KeepAliveEventHandler keepAlive)
        {
            // register keep alive handler
            if (keepAlive == null)
                this.Session.KeepAlive += this.SessionKeepAlive_Event;
            // register what was passed in
            else
                this.Session.KeepAlive += keepAlive;
        }

        /// <summary>
        /// Remove monitored items from a subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="monitoredItems"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ServiceResultException"></exception>
        public void RemoveMonitoredItemsFromSubscription(Subscription subscription,
            IEnumerable<MonitoredItem> monitoredItems)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (monitoredItems == null) throw new ArgumentNullException(nameof(monitoredItems));

            this.VerifySessionCreated();

            // remove items from subscription
            subscription.RemoveItems(monitoredItems: monitoredItems);

            subscription.ApplyChanges();
        }

        /// <summary>
        /// Auto accept certificates
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="eventArgs"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SessionAutoAcceptCertificate_Event(CertificateValidator validator,
            CertificateValidationEventArgs eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

            eventArgs.Accept = true;
        }

        /// <summary>
        /// Keep the OPC UA connection alive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void SessionKeepAlive_Event(Session sender,
            KeepAliveEventArgs eventArgs)
        {
            if (eventArgs.Status != null && ServiceResult.IsNotGood(status: eventArgs.Status))
            {
                if (this.ReconnectHandler == null)
                    this.SessionReconnect(session: sender);
            }
        }

        /// <summary>
        /// Reconnect the OPC UA session
        /// </summary>
        /// <param name="session"></param>
        private void SessionReconnect(Session session)
        {
            this.ReconnectTimer?.Start();

            this.ReconnectHandler = new SessionReconnectHandler();

            this.ReconnectHandler.BeginReconnect(session: session,
                reconnectPeriod: 5000,
                callback: this.SessionReconnectComplete_Event);
        }

        /// <summary>
        /// Reconnect to the source OPC UA server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void SessionReconnectComplete_Event(object sender,
            EventArgs eventArgs)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            // ignore callbacks from discarded objects
            if (!object.ReferenceEquals(sender, this.ReconnectHandler)) return;

            this.CreateOpcUaSession(session: this.ReconnectHandler.Session);

            this.SessionResetReconnect();
        }

        /// <summary>
        /// Session reconnect timer has expired - create new session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private async Task SessionReconnectExpired(object sender,
            EventArgs eventArgs)
        {
            try
            {
                // create new connection
                await this.InitializeOpcUaClientConnection(clientName: this.ClientName,
                    configPath: this.ConfigPath,
                    opcUaUrl: this.OpcUaUrl,
                    sessionName: Guid.NewGuid().ToString(),
                    sessionTimeout: this.SessionTimeout,
                    keepAlive: this.SessionKeepAlive_Event);

                this.SessionResetReconnect();
            }
            // unable to recreate session
            catch (ServiceResultException)
            {
                // restart the reconnect timer
                this.ReconnectTimer.Start();

                return;
            }

            // invoke new session event
            this.NewSessionEvent(eventArgs: new EventArgs());
        }

        /// <summary>
        /// Reset reconnection logic
        /// </summary>
        private void SessionResetReconnect()
        {
            this.ReconnectTimer?.Stop();

            this.ReconnectHandler?.Dispose();

            this.ReconnectHandler = null;
        }

        /// <summary>
        /// Verify that session is not null
        /// </summary>
        /// <exception cref="SessionNotCreatedException"></exception>
        private void VerifySessionCreated()
        {
            if (this.Session == null) throw new SessionNotCreatedException();
        }

        /// <summary>
        /// Write all the values currently in the <see cref="WriteValueCollection"/>
        /// </summary>
        private void WriteAllValuesInCollection()
        {
            _ = Task.Run(action: async () =>
            {
                try
                {
                    this.VerifySessionCreated();

                    lock (this.WriteValueCollection)
                    {
                        _ = this.Session.Write(requestHeader: null,
                            nodesToWrite: this.WriteValueCollection,
                            results: out _,
                            diagnosticInfos: out _);

                        this.WriteValueCollection.Clear();
                    }
                }
                catch (SessionNotCreatedException)
                {
                }

                try
                {
                    await Task.Delay(delay: TimeSpan.FromSeconds(value: 1),
                        cancellationToken: this.CancellationToken.Token);
                }
                catch (TaskCanceledException)
                {
                }
            }, cancellationToken: this.CancellationToken.Token);
        }
    }
}