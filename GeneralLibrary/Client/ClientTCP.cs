using GeneralLibrary.File;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GeneralLibrary.Client
{
    public class ClientTCP
    {
        // Размер буфера
        private const int BufferSize = 4096;
        // Состояние потоков установки соединения и передачи файла
        private readonly ManualResetEvent connectDone;
        private readonly ManualResetEvent sendDone;
        // Клиентский сокет
        private Socket client;

        public ClientTCP()
        {
            // Задание начального состояния потоков установки соединения и передачи файла
            connectDone = new ManualResetEvent(false);
            sendDone = new ManualResetEvent(false);
        }

        public void Start(string ipString, int port)
        {
            try
            {
                // Генерация связки ip-адрес + порт
                var ipAddress = IPAddress.Parse(ipString);
                var endPoint = new IPEndPoint(ipAddress, port);

                // Создание нового сокета
                client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Установка запроса асинхронного соединения
                client.BeginConnect(endPoint, new AsyncCallback(ConnectCallback), client);
                // Завершение работы потока
                connectDone.WaitOne();
            }
            catch
            {

            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Завершение запроса асинхронного соединения
                var res = (Socket)asyncResult.AsyncState;
                res.EndConnect(asyncResult);
                // Продолжение работы потока
                connectDone.Set();
            }
            catch
            {

            }
        }

        public void SendFile(string pathToFile)
        {
            // Получение информации о файле
            var transmittedFile = new Transmitted(pathToFile);

            // Отправка информации о файле
            SendFileDetails(transmittedFile);

            // Инициализаци параметров
            byte[] fileContentBuffer = new byte[BufferSize];
            long fileContentLength = transmittedFile.GetFileContentLength();
            long fileContentByteSent = 0;

            // Пока не переданы все байты
            while (fileContentByteSent != fileContentLength)
            {
                // Определяем очередной блок
                int numberOfBytesToSend = ((fileContentLength - fileContentByteSent) / BufferSize > 0) ?
                    BufferSize : (int)(fileContentLength - fileContentByteSent);

                // Читаем в буфер
                const int bufferOffset = 0;
                transmittedFile.ReadBytes(fileContentBuffer, bufferOffset,
                    numberOfBytesToSend, fileContentByteSent);

                // Отправляем блок данных
                client.BeginSend(fileContentBuffer, bufferOffset,
                    fileContentBuffer.Length, SocketFlags.None,
                    new AsyncCallback(SendFileCallback), client);

                // Обновляем информацию о переданных байтах
                fileContentByteSent += numberOfBytesToSend;
                Array.Clear(fileContentBuffer, bufferOffset, fileContentBuffer.Length);
            }
        }

        private void SendFileDetails(Transmitted transmittedFile)
        {
            // Инициализаци параметров
            byte[] fileDetails = transmittedFile.GetByteArrayFileDetails();
            byte[] fileDetailsLength = BitConverter.GetBytes((long)fileDetails.Length);
            byte[] fileContentLength = BitConverter.GetBytes(transmittedFile.GetFileContentLength());
            byte[] fileDetailsBuffer = new byte[fileDetailsLength.Length
                + fileDetails.Length + fileContentLength.Length];

            // Копирование данных
            const int stertIndex = 0;
            fileDetailsLength.CopyTo(fileDetailsBuffer, stertIndex);
            fileDetails.CopyTo(fileDetailsBuffer, fileDetailsLength.Length);
            fileContentLength.CopyTo(fileDetailsBuffer,
                fileDetailsLength.Length + fileDetails.Length);

            // Отправка информации о файле
            const int offset = 0;
            client.BeginSend(fileDetailsBuffer, offset,
                fileDetailsBuffer.Length, SocketFlags.None,
                new AsyncCallback(SendFileCallback), client);
        }

        private void SendFileCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Завершение потока передачи
                var sender = (Socket)asyncResult.AsyncState;
                int bytesSent = sender.EndSend(asyncResult);
                sendDone.Set();
            }
            catch
            {
                
            }
        }

        public void Stop()
        {
            try
            {
                // Закрытие соединения в обе стороны
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch
            {

            }
        }
    }
}
