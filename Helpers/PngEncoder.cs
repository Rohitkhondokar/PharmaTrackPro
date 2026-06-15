using System.IO.Compression;

namespace PharmaTrackPro.Helpers
{
    /// <summary>
    /// Writes a minimal valid 8-bit grayscale PNG from a monochrome pixel grid.
    /// Exists so barcode rendering doesn't require System.Drawing.Common or an
    /// extra rendering package — ZXing.Net gives us a pixel matrix; this turns
    /// it into a real, openable .png file using only the .NET BCL.
    /// </summary>
    public static class PngEncoder
    {
        private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
        private static readonly uint[] CrcTable = BuildCrcTable();

        /// <param name="pixels">true = black, false = white</param>
        public static byte[] EncodeGrayscale(bool[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            using var output = new MemoryStream();
            output.Write(Signature);

            // IHDR: width, height, bit depth 8, color type 0 (grayscale), compression 0, filter 0, interlace 0
            var ihdr = new byte[13];
            WriteUInt32BE(ihdr, 0, (uint)width);
            WriteUInt32BE(ihdr, 4, (uint)height);
            ihdr[8] = 8;  // bit depth
            ihdr[9] = 0;  // color type: grayscale
            ihdr[10] = 0; // compression method
            ihdr[11] = 0; // filter method
            ihdr[12] = 0; // interlace method
            WriteChunk(output, "IHDR", ihdr);

            // Raw scanlines: each row prefixed with filter-type byte 0 (None)
            using var rawStream = new MemoryStream();
            for (int y = 0; y < height; y++)
            {
                rawStream.WriteByte(0); // no filter
                for (int x = 0; x < width; x++)
                {
                    rawStream.WriteByte((byte)(pixels[x, y] ? 0 : 255));
                }
            }

            using var compressed = new MemoryStream();
            using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            {
                rawStream.Position = 0;
                rawStream.CopyTo(zlib);
            }
            WriteChunk(output, "IDAT", compressed.ToArray());

            WriteChunk(output, "IEND", Array.Empty<byte>());

            return output.ToArray();
        }

        private static void WriteChunk(Stream output, string type, byte[] data)
        {
            var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);

            var lengthBytes = new byte[4];
            WriteUInt32BE(lengthBytes, 0, (uint)data.Length);
            output.Write(lengthBytes);

            output.Write(typeBytes);
            output.Write(data);

            var crcInput = new byte[typeBytes.Length + data.Length];
            Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
            Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);

            var crcBytes = new byte[4];
            WriteUInt32BE(crcBytes, 0, Crc32(crcInput));
            output.Write(crcBytes);
        }

        private static void WriteUInt32BE(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)value;
        }

        private static uint[] BuildCrcTable()
        {
            var table = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                {
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                }
                table[n] = c;
            }
            return table;
        }

        private static uint Crc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (var b in data)
            {
                crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
            }
            return crc ^ 0xFFFFFFFF;
        }
    }
}
