using System.Net.Sockets;
using Moq;
using NUnit.Framework;
using ChatServer;

namespace Tests
{
    public class ConnectionManagerTests
    {
        private Mock<ClientObject> _mockClientObj;
        private ServerObject _serverObj;

        [SetUp]
        public void Setup()
        {
            var tcpClient = new TcpClient();
            var serverObject = new ServerObject();
            _serverObj = new ServerObject();
            _mockClientObj = new Mock<ClientObject>(tcpClient, serverObject);
        }

        [Test]
        public void Add()
        {
            _serverObj.AddConnection(_mockClientObj.Object);
            Assert.IsNotEmpty(_serverObj.clients);
        }

        [Test]
        public void Delete()
        {
            _serverObj.AddConnection(_mockClientObj.Object);
            int size = _serverObj.clients.Count;
            _serverObj.RemoveConnection(_mockClientObj.Object.Id, _mockClientObj.Object.userName);
            Assert.That(_serverObj.clients.Count != size);
        }

        // a matching constructor for the given arguments was not found on the mocked type.
    }
}