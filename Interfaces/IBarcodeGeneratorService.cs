namespace PharmaTrackPro.Interfaces
{
    public interface IBarcodeGeneratorService
    {
        /// <summary>Generates a random 12-digit numeric candidate value. Uniqueness is checked by the caller.</summary>
        string GenerateCandidateValue();

        /// <summary>Renders a Code128 barcode for the given value as PNG bytes.</summary>
        byte[] GenerateBarcodeImage(string value);

        /// <summary>Renders a QR code for the given value as PNG bytes.</summary>
        byte[] GenerateQrCodeImage(string value);
    }
}
