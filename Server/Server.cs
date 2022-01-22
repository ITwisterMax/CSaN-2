using GeneralLibrary.Server;
using System;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        static int Main(string[] args)
        {
            // Ввод данных
            Console.Write("Enter the IP address: ");
            string ipAddress = Console.ReadLine();

            Console.Write("Enter port: ");
            int port;
            try
            {
                port = Convert.ToInt32(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("Error! Wrong port was entered.");
                Console.Write("Press ENTER to exit...");
                Console.ReadLine();
                return 1;
            }

            Console.Write("Enter the folder to save the received files: ");
            string pathToFolder = Console.ReadLine() + @"\";

            // Запуск сервера
            ServerTCP newServer = null;           
            try
            {
                newServer = new ServerTCP(pathToFolder);
                Task.Run(() => newServer.Start(ipAddress, port));
            }
            catch
            {
                Console.WriteLine("Error! Server isn't running.");
                Console.Write("Press ENTER to exit...");
                Console.ReadLine();
                return 1;
            }
            Console.WriteLine("Server is running!");
 
            // Остановка сервера
            Console.Write("Press ENTER to stop server...");
            Console.ReadLine();
            try
            {
                newServer.Stop();
            }
            catch
            {
                Console.WriteLine("The server is stopped!");
            }
   
            return 0;
        }
    }
}
