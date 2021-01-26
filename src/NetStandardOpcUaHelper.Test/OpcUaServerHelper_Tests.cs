using NetStandardTestHelper.Xunit;
using Opc.Ua;
using System;
using Xunit;

namespace NetStandardOpcUaHelper.Test
{
    /// <summary>
    /// Collection fixture
    /// </summary>
    /// https://xunit.net/docs/shared-context
    public class OpcUaServerHelper_Fixture : IDisposable
    {
        /// <summary>
        /// OpcUaServerHelper
        /// </summary>
        public OpcUaServerHelper OpcUaServerHelper { get; }

        /// <summary>
        /// Do "global" initialization here; Called before every test method.
        /// </summary>
        public OpcUaServerHelper_Fixture()
        {
            this.OpcUaServerHelper = new OpcUaServerHelper(server: TestValues.Server,
                port: TestValues.Port);

            var path = TestValues.GetPath();

            _ = new ConfigFile(path: path);

            this.OpcUaServerHelper.InitializeOpcUaServer(configPath: path).Wait();
        }

        /// <summary>
        /// Do "global" teardown here; Called after every test method.
        /// </summary>
        public void Dispose()
        {
        }
    }

    /// <summary>
    /// OpcUaServerHelper Collection
    /// </summary>
    [CollectionDefinition("OpcUaServerHelper")]
    public class OpcUaServerHelper_Collection : ICollectionFixture<OpcUaServerHelper_Fixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    /// <summary>
    /// Test enum
    /// </summary>
    public enum TestEnum
    {
        /// <summary>
        /// Test 1
        /// </summary>
        Test1 = 1
    }

    /// <summary>
    /// Test the <see cref="OpcUaServerHelper"/> class
    /// </summary>
    [Collection("OpcUaServerHelper")]
    public class OpcUaServerHelper_Tests
    {
        private OpcUaServerHelper OpcUaServerHelper_Initialized { get; }

        /// <summary>
        /// Run the setup once
        /// </summary>
        /// <param name="fixture"></param>
        public OpcUaServerHelper_Tests(OpcUaServerHelper_Fixture fixture)
        {
            this.OpcUaServerHelper_Initialized = fixture.OpcUaServerHelper;
        }

        #region Constructor

        /// <summary>
        /// Test that constructor throws on null server
        /// </summary>
        [Fact]
        public void Constructor_Null_Server()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new OpcUaServerHelper(server: TestValues.Server_Null,
                port: TestValues.Port));

            TestHelper.AssertArgumentNullException(ex,
                "server");
        }

        /// <summary>
        /// Test that constructor throws on zero port
        /// </summary>
        [Fact]
        public void Constructor_Zero_Port()
        {
            var ex = Assert.Throws<ArgumentException>(() => new OpcUaServerHelper(server: TestValues.Server,
                port: 0));

            TestHelper.AssertExceptionText(ex,
                "port");
        }

        /// <summary>
        /// Test that constructor is created successfully
        /// </summary>
        [Fact]
        public void Constructor()
        {
            var helper = TestValues.OpcUaServerHelper;

            Assert.NotNull(helper);
        }

        #endregion Constructor

        #region AddPredefinedNode

        /// <summary>
        /// Test that function throws on null root node
        /// </summary>
        [Fact]
        public void AddPredefinedNode_Null_RootNode()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaServerHelper.AddPredefinedNode(rootNode: null));

            TestHelper.AssertArgumentNullException(ex,
                "rootNode");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void AddPredefinedNode_Not_Initialized()
        {
            var ex = Assert.Throws<NodeManagerNotCreatedException>(() => TestValues.OpcUaServerHelper.AddPredefinedNode(rootNode: new Opc.Ua.FolderState(parent: null)));

            TestHelper.AssertExceptionText(ex,
                Text.NodeManagerNotCreated);
        }

        #endregion AddPredefinedNode

        #region ConvertCSharpTypeToOpcType

        /// <summary>
        /// Test that function returns correctly for boolean
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Boolean()
        {
            object boolean = true;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref boolean);

            Assert.Equal(BuiltInType.Boolean,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for byte
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Byte()
        {
            object value = (byte)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Byte,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for DateTime
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_DateTime()
        {
            object dateTime = DateTime.Now;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref dateTime);

            Assert.Equal(BuiltInType.DateTime,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for double
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Double()
        {
            object value = (double)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Double,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for enum
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Enum()
        {
            object value = TestEnum.Test1;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Enumeration,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for float
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Float()
        {
            object value = (float)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Float,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for guid
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Guid()
        {
            object value = Guid.NewGuid();

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Guid,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for int 16
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Int16()
        {
            object value = (short)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Int16,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for int 32
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Int32()
        {
            object value = (int)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Int32,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for int 64
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Int64()
        {
            object value = (long)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Int64,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for sbyte
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_SByte()
        {
            object value = (sbyte)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.SByte,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for string
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_String()
        {
            object value = "value";

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.String,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for uint16
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_UInt16()
        {
            object value = (ushort)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.UInt16,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for uint32
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_UInt32()
        {
            object value = (uint)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.UInt32,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for uint64
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_UInt64()
        {
            object value = (ulong)7;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.UInt64,
                result);
        }

        /// <summary>
        /// Test that function returns correctly for null
        /// </summary>
        [Fact]
        public void ConvertCSharpTypeToOpcType_Default()
        {
            object value = null;

            var result = TestValues.OpcUaServerHelper.ConvertCSharpTypeToOpcUaType(value: ref value);

            Assert.Equal(BuiltInType.Null,
                result);
        }

        #endregion ConvertCSharpTypeToOpcType

        #region DeleteNode

        /// <summary>
        /// Test that function throws on null node id
        /// </summary>
        [Fact]
        public void DeleteNode_Null_NodeId()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaServerHelper.DeleteNode(nodeId: null));

            TestHelper.AssertArgumentNullException(ex,
                "nodeId");
        }

        /// <summary>
        /// Test that function throws on null node identifier
        /// </summary>
        [Fact]
        public void DeleteNode_Null_NodeIdentifier()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TestValues.OpcUaServerHelper.DeleteNode(nodeIdIdentifier: NetStandardTestHelper.TestValues.StringEmpty));

            TestHelper.AssertArgumentNullException(ex,
                "nodeIdIdentifier");
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void DeleteNode_Not_Initialized_NodeId()
        {
            var ex = Assert.Throws<NodeManagerNotCreatedException>(() => TestValues.OpcUaServerHelper.DeleteNode(nodeId: new NodeId()));

            TestHelper.AssertExceptionText(ex,
                Text.NodeManagerNotCreated);
        }

        /// <summary>
        /// Test that function throws for session not being initialized
        /// </summary>
        [Fact]
        public void DeleteNode_Not_Initialized_NodeIdentifier()
        {
            var ex = Assert.Throws<NodeManagerNotCreatedException>(() => TestValues.OpcUaServerHelper.DeleteNode(nodeIdIdentifier: TestValues.ObjectName));

            TestHelper.AssertExceptionText(ex,
                Text.NodeManagerNotCreated);
        }

        #endregion DeleteNode

        #region InitializeOpcServer

        /// <summary>
        /// Test that function throws on null config path
        /// </summary>
        [Fact]
        public async void InitializeOpcServer_Null_ConfigPath()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => TestValues.OpcUaServerHelper.InitializeOpcUaServer(configPath: NetStandardTestHelper.TestValues.StringEmpty));

            TestHelper.AssertArgumentNullException(ex,
                "configPath");
        }

        #endregion InitializeOpcServer
    }
}