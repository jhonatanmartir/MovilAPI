using AESMovilAPI.DTOs;
using Barcoder.Code128;
using Barcoder.Renderer.Image;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using QRCoder;
using SkiaSharp;

namespace AESMovilAPI.Utilities
{
    public static class Graphic
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

        public static byte[]? CreateBarChartByte(List<IdValueDto> data, string promedio)
        {
            try
            {
                // Extraer los valores numéricos y convertirlos a double
                var numericValues = data.Select(d => double.TryParse(d.Value, out var v) ? v : 0).ToList();

                // Extraer las etiquetas del eje X (fechas)
                var labels = data.Select(d => d.Id).ToList();

                // Crear serie de datos de barras
                var series = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = numericValues,
                        Fill = new SolidColorPaint(SKColors.Black),  // Color de las barras
                        DataLabelsPosition = DataLabelsPosition.Top, // Posición encima de las barras
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black), // Color de las etiquetas
                        DataLabelsSize = 28,
                        MaxBarWidth = 32
                    }
                };

                // Configuración del eje X con etiquetas personalizadas
                var xAxis = new Axis
                {
                    Labels = labels,                            // Etiquetas del eje X extraídas de la lista
                    Name = "FECHA DE LECTURA",
                    NameTextSize = 28,
                    NamePaint = new SolidColorPaint             // Personalización del texto del nombre del eje
                    {
                        Color = SKColors.Black,                 // Color del texto del nombre
                        SKTypeface = SKTypeface.FromFamilyName(
                            "Helvetica", SKFontStyle.Normal)      // Fuente en negrita para el nombre
                    },
                    TextSize = 24,                              // Texto de las etiquetas
                    MinStep = 1,                                // Asegura que cada etiqueta sea visible
                    LabelsPaint = new SolidColorPaint           // Personalización del texto de las etiquetas
                    {
                        Color = SKColors.Black,
                        SKTypeface = SKTypeface.FromFamilyName(
                            "Helvetica", SKFontStyle.Bold)      // Tipo de letra en negrita
                    },
                    SeparatorsPaint = new SolidColorPaint
                    {
                        Color = SKColors.LightGray,             // Color de las líneas verticales
                        StrokeThickness = 1
                    }
                };

                var yAxis = new Axis
                {
                    Name = "KWH",
                    NameTextSize = 28,
                    NamePaint = new SolidColorPaint             // Personalización del texto del nombre del eje
                    {
                        Color = SKColors.Black,                 // Color del texto del nombre
                        SKTypeface = SKTypeface.FromFamilyName(
                            "Helvetica", SKFontStyle.Normal)      // Fuente en negrita para el nombre
                    },
                    MinLimit = 0,                               // Asegura que el eje Y comience desde cero
                    TextSize = 28,
                    LabelsPaint = new SolidColorPaint           // Personalización del texto de las etiquetas
                    {
                        Color = SKColors.Black,
                        SKTypeface = SKTypeface.FromFamilyName(
                            "Helvetica", SKFontStyle.Bold)      // Tipo de letra en negrita
                    },
                    SeparatorsPaint = new SolidColorPaint
                    {
                        Color = SKColors.LightGray,             // Color de las líneas horizontales
                        StrokeThickness = 1
                    }
                };

                var chart = new SKCartesianChart
                {
                    Series = series,
                    XAxes = new[] { xAxis },
                    YAxes = new[] { yAxis },
                    Width = 800,
                    Height = (int)(800 / 2)
                };

                // Renderizar en un bitmap
                using var bitmap = new SKBitmap(chart.Width, chart.Height);
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.White);
                chart.DrawOnCanvas(canvas);

                // Codificar el bitmap como PNG y devolver el array de bytes
                using var image = SKImage.FromBitmap(bitmap);
                using var dataImg = image.Encode(SKEncodedImageFormat.Png, 100);
                return dataImg.ToArray();
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
