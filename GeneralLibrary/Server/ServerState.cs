using System.Net.Sockets;

namespace GeneralLibrary.Server
{
    class ServerState
    {
        public const int BufferSize = 4096;

        public Socket WorkSocket { get; set; }
        public byte[] Buffer { get; set; }

        public long NeedToReceivedBytes { get; set; }
        public long ByteReceived { get; set; }
        public bool IsFirstBlockReceived { get; set; }

        // Информация о текущем сокете, буфере и принимающем файле
        public ServerState()
        {
            WorkSocket = null;
            Buffer = new byte[BufferSize];

            NeedToReceivedBytes = 0;
            ByteReceived = 0;
            IsFirstBlockReceived = true;
        }
    }
}
