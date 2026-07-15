namespace CapfortuneBE.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public string Layer { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
