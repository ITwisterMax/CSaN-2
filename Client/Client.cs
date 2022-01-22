using GeneralLibrary.Client;
using System;

namespace Sender
{
    class Client
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

            // Установка соединения с сервером и передача файла
            try
            {
                var newClient = new ClientTCP();
                newClient.Start(ipAddress, port);

                Console.Write("Enter path to file: ");
                string pathToFile = Console.ReadLine();

                newClient.SendFile(pathToFile);

                newClient.Stop();
            }
            catch
            {
                Console.WriteLine("Error! File couldn't be sent.");
                Console.Write("Press ENTER to exit...");
                Console.ReadLine();
                return 1;
            }

            Console.WriteLine("File sent successfully!");
            Console.Write("Press ENTER to exit...");
            Console.ReadLine();
            return 0;
        }
    }
}
