using Barcoder.Code128;
using Barcoder.Renderer.Image;
using QRCoder;

namespace AESMovilAPI.Utilities
{
    public class Graphic
    {
        public static byte[]? GenerateSimpleQRByte(string text, string name, int pixelSize = 2)
        {
            try
            {
                QRCodeGenerator qrGen = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.H);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                return qrCode.GetGraphic(pixelSize, false);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public static byte[]? GenerateBarCodeByte(string text, string name, int pixelSize = 2)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var barcode = Code128Encoder.Encode(text);
                var renderer = new ImageRenderer(new ImageRendererOptions { ImageFormat = ImageFormat.Png, PixelSize = pixelSize, CustomMargin = 1 });

                using (var stream = new MemoryStream())
                {
                    renderer.Render(barcode, stream);
                    return stream.ToArray();
                }
            }

            return null;
        }
    }
}
