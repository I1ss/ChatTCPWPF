using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ChatServer
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comm { get; set; }
    }
    class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; private set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=helloappdb;Trusted_Connection=True;");
        }
    }
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        protected internal string userName { get; private set; }
        TcpClient client;
        ServerObject server; 

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                string message;
                while (true)
                {
                    bool ans = true;
                    message = GetMessage();
                    for (int i = 0; i < server.clients_names.Count; i++)
                    {
                        if (message == server.clients_names[i])
                        {
                            ans = false;
                            break;
                        }
                    }
                    if (ans == false)
                    {
                        Stream.Write(Encoding.Unicode.GetBytes("Такой никнейм уже занят!"));
                    }
                    else
                    {
                        if (message == "")
                        {
                            throw new Exception("bad nickname");
                        }
                        userName = message;
                        server.AddConnection(this);
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            User user1 = new User { Name = userName, Comm = "Client" };
                            db.Users.Add(user1);
                            db.SaveChanges();
                            Console.WriteLine("Объект успешно сохранен.");
                        }
                        break;
                    }
                }
                server.SetNewName(message);
                message = userName + " вошел в чат";
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        string temp = message;
                        if (message == "")
                        {
                            throw new Exception("bad msg");
                        }
                        StringBuilder name = new StringBuilder(), msg = new StringBuilder();
                        message = String.Format("{0}: {1}", userName, message);
                        if (temp.Length >= 4 && temp.Substring(0, 4) == "Send") 
                        {
                            int k = 5;
                            for (int j = 5; temp[j] != ' '; j++)
                            {
                                name.Append(temp[j]);
                                k = j;
                            }
                            k += 2;
                            for (int j = k; j < temp.Length; j++)
                            {
                                msg.Append(temp[j]);
                            }
                            server.BroadcastMessageInd(name.ToString(), msg.ToString(), userName);
                        }
                        else if (temp == "List")
                        {
                            using (ApplicationContext db = new ApplicationContext())
                            {
                                var users = db.Users.ToList();
                                Console.WriteLine("Список объектов:");
                                foreach (User u in users)
                                {
                                    Console.WriteLine($"{u.Id}: {u.Name}, {u.Comm}");
                                }
                            }
                        }
                        else
                        {
                            server.BroadcastMessage(message, this.Id);
                        }
                    }
                    catch
                    {
                        message = string.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            var users = db.Users.ToList();
                            Console.WriteLine("Список объектов:");
                            foreach (User u in users)
                            {
                                if (u.Name == this.userName)
                                {
                                    db.Users.Remove(u);
                                    users.Remove(u);
                                    db.SaveChanges();
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id, this.userName);
                Close();
            }
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}