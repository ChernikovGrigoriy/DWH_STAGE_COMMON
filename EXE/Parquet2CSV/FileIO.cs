using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parquet2CSV
{
    public static class FileIO
    {
        public static byte[] ConvertFileToByteArray(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' was not found.");
            }

            // Read all bytes from the file
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return fileBytes;
        }
    }
}
