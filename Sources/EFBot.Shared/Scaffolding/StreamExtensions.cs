using System.IO;

namespace EFBot.Shared.Scaffolding {
    public static class StreamExtensions
    {
        public static byte[] ReadToEnd(this Stream instance)
        {
            byte[] buffer = new byte[2048];
            using (var ms = new MemoryStream())
            {
                int bytesRead;
                while ((bytesRead = instance.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }

                return ms.ToArray();
            }
        }
    }
}