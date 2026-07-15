namespace CapfortuneBE.Models
{
    public class EnquiryDTO
    {
        public class CreateEnquiryRequest
        {
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerNumber { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? Source { get; set; }
        }
        public class Enquiry
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerNumber { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? Source { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
        }
        public class PagedResult<T>
        {
            public List<T> Items { get; set; } = new();
            public int Total { get; set; }
        }
    }
}
