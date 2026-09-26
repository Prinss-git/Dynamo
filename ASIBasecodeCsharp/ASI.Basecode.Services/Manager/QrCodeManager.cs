using ZXing;
using ZXing.QrCode;

namespace ASI.Basecode.Services.Manager
{
    /// <summary>
    /// QR Code Manager
    /// </summary>
    public static class QrCodeManager
    {
        /// <summary>
        /// Generates a QR code as inline SVG markup (no System.Drawing dependency).
        /// </summary>
        /// <param name="content">The text to encode.</param>
        /// <param name="size">Width and height in pixels.</param>
        /// <returns>SVG markup</returns>
        public static string GenerateSvg(string content, int size = 240)
        {
            var writer = new BarcodeWriterSvg
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = size,
                    Height = size,
                    Margin = 1,
                },
            };

            return writer.Write(content).Content;
        }
    }
}
