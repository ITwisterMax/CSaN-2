using GeneralLibrary.File;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GeneralLibrary.Server
{
    public class ServerTCP
    {
        // Максимальное количество клиентов
        private const int MaxConnections = 10;
        // Переменная управления состоянием потока
        private readonly ManualResetEvent allDone;
        // Путь к файлу
        private readonly string saveFolderPath;
        // Передаваемый файл
        private Transmitted transmittedFile;
        // Сокет сервера
        private Socket server;

        public ServerTCP(string FolderPath)
        {
            // Установка начальных параметров
            allDone = new ManualResetEvent(false);
            saveFolderPath = FolderPath;
        }

        public void Start(string ipString, int port)
        {
            try
            {
                // Генерация связки ip-адрес + порт
                var ipAddress = IPAddress.Parse(ipString);
                var endPoint = new IPEndPoint(ipAddress, port);

                // Создание нового сокета
                server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Связка сокета с ip-адрес + порт
                server.Bind(endPoint);

                // Указание максимального количества клиентов
                server.Listen(MaxConnections);

                while (true)
                {
                    // Блокировка работы потока
                    allDone.Reset();
                    // Установка запроса асинхронного приема
                    server.BeginAccept(new AsyncCallback(AcceptConnection), server);
                    // Завершение работы потока
                    allDone.WaitOne();
                }
            }
            catch
            {

            }
        }

        private void AcceptConnection(IAsyncResult asyncResult)
        {
            // Установка для потока в положение работы
            allDone.Set();

            // Завершение запроса асинхронного соединения
            var listener = (Socket)asyncResult.AsyncState;
            var handler = listener.EndAccept(asyncResult);

            var listenerState = new ServerState
            {
                WorkSocket = handler
            };

            // Асинхронный прием данных
            const int bufferOffset = 0;
            handler.BeginReceive(listenerState.Buffer, bufferOffset,
                ServerState.BufferSize, SocketFlags.None,
                new AsyncCallback(EndReceive), listenerState);
        }

        private void EndReceive(IAsyncResult asyncResult)
        {
            // Завершение запроса асинхронного приема
            var listenerState = (ServerState)asyncResult.AsyncState;
            var handler = listenerState.WorkSocket;
            int bytesRead = handler.EndReceive(asyncResult);

            // Если есть, что читать
            if (bytesRead > 0)
            {
                // Проверка на первый блок и прием информации о файле
                if (listenerState.IsFirstBlockReceived)
                {   
                    transmittedFile = ReceiveFileDetails(listenerState);

                    Array.Clear(listenerState.Buffer, 0, listenerState.Buffer.Length);
                    listenerState.IsFirstBlockReceived = false;
                }
                else
                {
                    // Количество байт, которые нужно записать
                    int numberOfBytesToWrite = ((listenerState.NeedToReceivedBytes - listenerState.ByteReceived)
                        / ServerState.BufferSize > 0) ? ServerState.BufferSize
                        : (int)(listenerState.NeedToReceivedBytes - listenerState.ByteReceived);

                    // Записываем данные из буфера в файл
                    const int bufferOffset = 0;
                    transmittedFile.WriteBytes(saveFolderPath, listenerState.Buffer,
                        bufferOffset, numberOfBytesToWrite);

                    // Чистим буфер и обновляем информацию о количестве принятых байт 
                    const int startIndex = 0;
                    Array.Clear(listenerState.Buffer, startIndex, listenerState.Buffer.Length);
                    listenerState.ByteReceived += numberOfBytesToWrite;
                }
                
                // Если не все байты дошли, досылаем
                if (listenerState.NeedToReceivedBytes != listenerState.ByteReceived)
                {
                    const int bufferOffset = 0;
                    handler.BeginReceive(listenerState.Buffer, bufferOffset,
                        ServerState.BufferSize, SocketFlags.None,
                        new AsyncCallback(EndReceive), listenerState);
                }
            }
        }

        private Transmitted ReceiveFileDetails(ServerState listenerState)
        {
            const int reservedСellSize = 8;
            const int startIndex = 0;

            // Длина под информацию о данных о файле в соответствии с размером ячейки
            long fileDetailsLength = BitConverter.ToInt64(listenerState.Buffer.Take(reservedСellSize).ToArray(), startIndex);
            // Сами данные
            byte[] fileDetails = listenerState.Buffer.Skip(reservedСellSize).Take((int)fileDetailsLength).ToArray();
            // Количество байт, необходимых получить
            listenerState.NeedToReceivedBytes = BitConverter.ToInt64(listenerState.Buffer.Skip(reservedСellSize + fileDetails.Length).ToArray(), startIndex);

            // Формируем файл
            return new Transmitted(fileDetails);
        }

        public void Stop()
        {
            try
            {
                // Закрытие соединения в обе стороны
                server.Shutdown(SocketShutdown.Both);
                server.Close();
            }
            catch
            {

            }
        }
    }
}
