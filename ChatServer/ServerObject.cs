using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Text.Json;
using System.IO;

namespace ChatServer
{
    public class Server
    {
        [JsonProperty("ip")]
        public string ip { set; get; }
        [JsonProperty("data")]
        public string data { set; get; }
        [JsonProperty("port")]
        public int port { set; get; }
    }
    public class ServerObject
    {
        static TcpListener tcpListener; 
        public List<ClientObject> clients = new List<ClientObject>();
        public List<String> clients_names = new List<String>();
        private string ip;
        private string data;
        private int port;
        public ServerObject()
        {
            using (StreamReader file = File.OpenText("C:\\Users\\User\\Desktop\\chat\\ChatTCPWPF\\ChatServer\\bin\\Debug\\netcoreapp3.1\\conf.json"))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                Server serv = (Server)serializer.Deserialize(file, typeof(Server));
                this.port = serv.port;
                this.ip = serv.ip;
                this.data = serv.data;
            }
        }
        public void SetNewName(String name)
        {
            clients_names.Add(name);
            BroadcastMessage("Users: " + String.Join(" ", clients_names) + " ", null);
        }
        public void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
            return;
        }
        public void RemoveConnection(string id, string userName)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                try
                {
                    clients.Remove(client);
                    clients_names.Remove(userName);
                    BroadcastMessage("Users: " + String.Join(" ", clients_names) + " ", null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return;
        }
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(ip), port);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Stream.Write(data, 0, data.Length); 
            }
        }

        protected internal void BroadcastMessageInd(string name, string message, string userName)
        {
            message = String.Format("{0} лично тебе: {1}", userName, message);
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].userName == name) 
                {
                    clients[i].Stream.Write(data, 0, data.Length); 
                }
            }
        }
        protected internal void Disconnect()
        {
            tcpListener.Stop(); 

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}