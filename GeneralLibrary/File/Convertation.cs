using System;
using System.IO;
using System.Xml.Serialization;

namespace GeneralLibrary.File
{
    [Serializable]
    public class Convertation
    {
        public string Name { get; set; }
        public string Extension { get; set; }

        public static byte[] ConvertToByteArray(Convertation fileDetails)
        {
            byte[] serializedFileDetails;

            // xml-сериализация информации о файле
            using (var memoryStream = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(typeof(Convertation));
                xmlSerializer.Serialize(memoryStream, fileDetails);

                memoryStream.Position = 0;
                serializedFileDetails = new byte[memoryStream.Length];

                const int memoryStreamOffset = 0;
                memoryStream.Read(serializedFileDetails, memoryStreamOffset,
                    serializedFileDetails.Length);
            }

            // сериализованный массив байтов
            return serializedFileDetails;
        }

        public static Convertation ConvertToFileDetails(byte[] byteArrayFileDetails)
        {
            // xml-десериализация информации о файле
            using (var memoryStream = new MemoryStream())
            {
                const int memoryStreamOffset = 0;

                memoryStream.Write(byteArrayFileDetails, memoryStreamOffset,
                    byteArrayFileDetails.Length);
                memoryStream.Position = 0;

                var xmlSerializer = new XmlSerializer(typeof(Convertation));

                // десериализованная информация о файле
                return (Convertation)xmlSerializer.Deserialize(memoryStream);
            }
        }
    }
}
