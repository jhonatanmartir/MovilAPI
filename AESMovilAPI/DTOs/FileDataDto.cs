namespace AESMovilAPI.DTOs
{
    public class FileDataDto
    {
        public byte[]? DataByte { get; set; } = null;
        public string Name { get; set; }
        public long InterfaceTime { get; set; }
        public long BuildTime { get; set; }
    }
}
