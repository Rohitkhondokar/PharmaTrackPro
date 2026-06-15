using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using QRCoder;
using ZXing;
using ZXing.Common;

namespace PharmaTrackPro.Services
{
    public class BarcodeGeneratorService : IBarcodeGeneratorService
    {
        private static readonly Random Random = new();

        public string GenerateCandidateValue()
        {
            // 12 random digits, e.g. "048213765920" — plain numeric keeps
            // Code128 encoding simple and reads cleanly on any barcode scanner.
            var digits = new char[12];
            for (int i = 0; i < digits.Length; i++)
            {
                digits[i] = (char)('0' + Random.Next(0, 10));
            }
            return new string(digits);
        }

        public byte[] GenerateBarcodeImage(string value)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = 320,
                    Height = 110,
                    Margin = 10,
                    PureBarcode = true // the pure pixel-data writer has no font renderer for human-readable text;
                                       // we show the numeric value as ordinary HTML text next to the image instead
                }
            };

            var pixelData = writer.Write(value);

            // ZXing gives us BGRA32 pixel bytes; convert to a bool grid (true = black)
            // and hand off to our own PNG encoder — no System.Drawing dependency needed.
            var pixels = new bool[pixelData.Width, pixelData.Height];
            for (int y = 0; y < pixelData.Height; y++)
            {
                for (int x = 0; x < pixelData.Width; x++)
                {
                    int offset = (y * pixelData.Width + x) * 4;
                    byte blue = pixelData.Pixels[offset];
                    pixels[x, y] = blue < 128; // dark pixel = bar
                }
            }

            return PngEncoder.EncodeGrayscale(pixels);
        }

        public byte[] GenerateQrCodeImage(string value)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(8);
        }
    }
}
