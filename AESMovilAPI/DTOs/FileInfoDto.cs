namespace AESMovilAPI.DTOs
{
    public class FileInfoDto
    {
        public string Type { get; set; }
        public string DocumentNumber { get; set; }
        public bool Legacy { get; set; } = false;
    }
}
