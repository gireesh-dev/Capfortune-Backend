using static CapfortuneBE.Models.EnquiryDTO;

namespace CapfortuneBE.Interface
{
    public interface IEnquiryDataAccess
    {
        Task<Enquiry> CreateEnquiry(CreateEnquiryRequest request);
        Task<PagedResult<Enquiry>> GetEnquiriesList(int page, int pageSize, string? search);
        Task<Enquiry?> GetEnquiryById(int id);
        Task<Enquiry?> UpdateEnquiryStatus(int id, string status);
        Task<List<Enquiry>> GetEnquiriesBySource(string source);
    }
}
