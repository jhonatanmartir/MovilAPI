namespace AESMovilAPI.Utilities
{
    public class FilesCache
    {
        private readonly IWebHostEnvironment _env;
        private readonly Dictionary<string, byte[]> _imageCache = new();
        private byte[] _pdfTemplate;

        // Constructor que recibe IWebHostEnvironment
        public FilesCache(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Precargar imágenes
        public void PreloadResources()
        {
            string basePath = Path.Combine(_env.ContentRootPath, "Sources");

            _imageCache[Constants.CAESS_NAME] = File.ReadAllBytes(Path.Combine(basePath, "Images", $"{Constants.CAESS_NAME}-logo.png"));
            _imageCache[Constants.DEUSEM_NAME] = File.ReadAllBytes(Path.Combine(basePath, "Images", $"{Constants.DEUSEM_NAME}-logo.png"));
            _imageCache[Constants.EEO_NAME] = File.ReadAllBytes(Path.Combine(basePath, "Images", $"{Constants.EEO_NAME}-logo.png"));
            _imageCache[Constants.CLESA_NAME] = File.ReadAllBytes(Path.Combine(basePath, "Images", $"{Constants.CLESA_NAME}-logo.png"));

            // Precargar PDF plantilla
            string pdfTemplatePath = Path.Combine(basePath, "Template", "FACT122024a.pdf");
            if (File.Exists(pdfTemplatePath))
            {
                _pdfTemplate = File.ReadAllBytes(pdfTemplatePath);
            }
        }

        // Obtener imagen en bytes
        public byte[] GetImageBytes(string imageKey)
        {
            return _imageCache.TryGetValue(imageKey, out var bytes) ? bytes : null;
        }

        // Obtener el PDF plantilla en bytes
        public byte[] GetPdfTemplate()
        {
            return _pdfTemplate;
        }
    }
}
