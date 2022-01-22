using System.IO;

namespace GeneralLibrary.File
{
    public class Transmitted
    {
        private readonly Convertation _fileDetails;
        private readonly string _pathToFile;

        // Импорт методов и свойст класса Convertation
        private Transmitted()
        {
            _fileDetails = new Convertation();
        }

        // Инициализация информации о файле
        public Transmitted(string pathToFile) : this()
        {
            _pathToFile = pathToFile;

            var fileInfo = new FileInfo(pathToFile);
            _fileDetails.Name = Path.GetFileNameWithoutExtension(fileInfo.Name);
            _fileDetails.Extension = Path.GetExtension(fileInfo.Name);
        }

        // Преобразование из массива байт в файл
        public Transmitted(byte[] byteArrayFileDetails) : this()
        {
            _fileDetails = Convertation.ConvertToFileDetails(byteArrayFileDetails);
        }

        // Преобразование из файла в массив байт 
        public byte[] GetByteArrayFileDetails()
        {
            return Convertation.ConvertToByteArray(_fileDetails);
        }

        // Получение размера файла
        public long GetFileContentLength()
        {
            var fileInfo = new FileInfo(_pathToFile);
            return fileInfo.Length;
        }

        // Чтение данных в буфер
        public void ReadBytes(byte[] buffer, int bufferOffset, int count, long fileOffset)
        {
            using (var fileStream = new FileStream(_pathToFile,
                FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(fileOffset, SeekOrigin.Begin);
                fileStream.Read(buffer, bufferOffset, count);
            }
        }

        // Запись данных из буфера в файл
        public void WriteBytes(string pathToFolder, byte[] content, int bufferOffset, int count)
        {
            using (var fileStream = new FileStream(pathToFolder + _fileDetails.Name
                + _fileDetails.Extension, FileMode.Append,
                FileAccess.Write, FileShare.Write))
            {
                fileStream.Write(content, bufferOffset, count);
            }
        }
    }
}
