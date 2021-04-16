using System;
using System.IO;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.InitialData
{
    public static class FileHelper
    {
        public static string GetTextFromFile(string path)
        {
            string result = String.Empty;

            if (File.Exists(path))
            {
                using (var file = new FileStream(path, FileMode.Open))
                {
                    byte[] bytes = new byte[file.Length];
                    int numBytesToRead = (int)file.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        int n = file.Read(bytes, numBytesRead, numBytesToRead);

                        if (n == 0) break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }

                    result = Encoding.UTF8.GetString(bytes);
                }
            }

            return result;
        }
    }
}
