<h1 align="center">Net Standard OPC UA Helper</h1>

<div align="center">
    
<b>Collection of helper functions for interacting with OPC UA servers</b>
    
[![Build Status](https://dev.azure.com/kbrashears5/github/_apis/build/status/kbrashears5.net-standard-opc-ua-helper?branchName=master)](https://dev.azure.com/kbrashears5/github/_build/latest?definitionId=34&branchName=master)
[![Tests](https://img.shields.io/azure-devops/tests/kbrashears5/github/34)](https://img.shields.io/azure-devops/tests/kbrashears5/github/34)
[![Code Coverage](https://img.shields.io/azure-devops/coverage/kbrashears5/github/34)](https://img.shields.io/azure-devops/coverage/kbrashears5/github/34)

[![nuget](https://img.shields.io/nuget/v/NetStandardOpcUaHelper.svg)](https://www.nuget.org/packages/NetStandardOpcUaHelper/)
[![nuget](https://img.shields.io/nuget/dt/NetStandardOpcUaHelper)](https://img.shields.io/nuget/dt/NetStandardOpcUaHelper)
</div>

# Usage
## OPC UA Client Helper
### Base usage
```c#
var helper = new OpcUaClientHelper();

helper.InitializeOpcUaClientConnection(clientName: "MyOpcUaClient",
    configPath: @"C:\temp\client.xml",
    opcUaUrl: "opc.tcp://localhost:4840",
    sessionName: "MySessionName",
    sessionTimeout: 60000);

var monitoredItems = new List<MonitoredItems>()
{
    helper.CreateOpcUaMonitoredItem(opcUaNodeId: "node.attributeName",
        notification: this.NotificationEventHandler),
};

helper.CreateOpcUaSubscription(monitoredItems: monitoredItems,
    publishingInterval: 1000);
```

### Event Handler
```c#
public void NotificationEventHandler(MonitoredItem monitoredItem,
    EventArgs eventArgs)
{
    Console.WriteLine(monitoredItem.StartNodeId.Identifier.ToString());

    foreach (var value in monitoredItem.DequeueValues())
    {
        Console.WriteLine(value.Value);
        Console.WriteLine(value.SourceTimestamp.ToString());
    }
}
```

### Keep Alive
You can run your own keep alive code by providing a keep alive to the `InitializeOpcUaClientConnection` function
```c#
var helper = new OpcUaClientHelper();

helper.InitializeOpcUaClientConnection(clientName: "MyOpcUaClient",
    configPath: @"C:\temp\client.xml",
    opcUaUrl: "opc.tcp://localhost:4840",
    sessionName: "MySessionName",
    sessionTimeout: 60000,
    keepAlive: this.SessionKeepAlive_Event);
```
Where the keep alive event looks like:
```c#
private void SessionKeepAlive_Event(Session sender,
    KeepAliveEventArgs eventArgs)
{
    if (eventArgs.Status != null && ServiceResult.IsNotGood(status: eventArgs.Status))
    {
        // keep alive code
    }
}  
```

### NewSession Event
When unable to reconnect to the OPC UA server, the helper will try to create a new session until it succeeds

When this happens however, you'll lose the subscriptions that were previously there.

Subscribe to the `NewSession` event to recreate these subscriptions once the new session has been created