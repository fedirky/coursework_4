using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading;

// Переконайтеся, що цей namespace відповідає namespace в вашому файлі BSBI.
namespace BSBI
{
    class Server
    {
        private readonly BSBI bsbi;

        public Server(BSBI bsbi)
        {
            this.bsbi = bsbi;
        }

        public void Start(string ipAddress, int port)
        {
            var listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            listener.Start();

            try
            {
                while (true)
                {
                    Console.WriteLine("Очікування на з'єднання...");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Підключено!");

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.Start(client);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();

            try
            {
                byte[] buffer = new byte[153600];
                int bytes = stream.Read(buffer, 0, buffer.Length);
                string searchTerm = Encoding.UTF8.GetString(buffer, 0, bytes);

                string searchResults = bsbi.PerformSearch(searchTerm); 
                //Console.WriteLine(searchResults);
                byte[] responseData = Encoding.UTF8.GetBytes(searchResults);
                stream.Write(responseData, 0, responseData.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Помилка: {e.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var bsbi = new BSBI();
            bsbi.BSBIIndexConstruction(@"C:\Users\fedir\PycharmProjects\files_creator\selected_files");

            var server = new Server(bsbi);
            server.Start("127.0.0.1", 8080);
        }
    }
}
