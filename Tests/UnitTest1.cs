using NUnit.Framework;
using ChatServer;
using System.Net.Sockets;

namespace Tests
{
    public class ConnectionManagerTests
    {
        [Test]
        public void AddConnectionTest()
        {
            ServerObject so = new ServerObject();
            ClientObject co = new ClientObject(new TcpClient(), so);
            so.AddConnection(co);
            Assert.That(so.clients.Count == 1);
        }

        [Test]
        public void RemoveConnectionTest()
        {
            ServerObject so = new ServerObject();
            ClientObject co = new ClientObject(new TcpClient(), so);
            so.AddConnection(co);
            so.RemoveConnection(co.Id, co.userName);
            Assert.That(so.clients.Count == 0);
        }

        [Test]
        public void SetNewNameTest()
        {
            ServerObject so = new ServerObject();
            so.SetNewName("Alex");
            Assert.That(so.clients_names[0] == "Alex");
        }
    }
}