using CapfortuneBE.Interface;
using Dapper;
using System.Data;
using static CapfortuneBE.Models.EnquiryDTO;

namespace CapfortuneBE.DataAccess
{
    public class EnquiryDataAccess : IEnquiryDataAccess
    {
        public readonly ILogger<EnquiryDataAccess> _logger;
        private readonly DapperContext _context;

        public EnquiryDataAccess(ILogger<EnquiryDataAccess> logger, DapperContext context)
        {
            _logger = logger;
            _context = context;
        }
        private IDbConnection CreateConnection()
        {
            return _context.CreateConnection();
        }
        public async Task<Enquiry> CreateEnquiry(CreateEnquiryRequest request)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
                    INSERT INTO Customer_Enquiries
                    (
                        CustomerName,
                        CustomerNumber,
                        CustomerEmail,
                        Description,
                        Status,
                        Source,
                        CreatedDate
                    )
                    VALUES
                    (
                        @CustomerName,
                        @CustomerNumber,
                        @CustomerEmail,
                        @Description,
                        @Status,
                        @Source,
                        NOW()
                    );
                    SELECT
                        Id,
                        CustomerName,
                        CustomerNumber,
                        CustomerEmail,
                        Description,
                        Status,
                        Source,
                        CreatedDate
                    FROM Customer_Enquiries
                    WHERE Id = LAST_INSERT_ID();
                    ";

                var enquiry = await connection.QuerySingleAsync<Enquiry>(
                    query,
                    new
                    {
                        request.CustomerName,
                        request.CustomerNumber,
                        request.CustomerEmail,
                        request.Description,
                        request.Status,
                        request.Source
                    });

                return enquiry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an enquiry.");
                throw;
            }
        }
        public async Task<PagedResult<Enquiry>> GetEnquiriesList(int page, int pageSize, string? search)
        {
            try
            {
                using var connection = CreateConnection();

                var offset = (page - 1) * pageSize;

                string countQuery = @"
                    SELECT COUNT(*)
                    FROM Customer_Enquiries
                    WHERE (@Search IS NULL
                        OR CustomerName LIKE CONCAT('%', @Search, '%')
                        OR CustomerEmail LIKE CONCAT('%', @Search, '%')
                        OR CustomerNumber LIKE CONCAT('%', @Search, '%'))";

                string dataQuery = @"
                    SELECT
                        Id,
                        CustomerName,
                        CustomerNumber,
                        CustomerEmail,
                        Description,
                        Status,
                        Source,
                        CreatedDate
                    FROM Customer_Enquiries
                    WHERE (@Search IS NULL
                        OR CustomerName LIKE CONCAT('%', @Search, '%')
                        OR CustomerEmail LIKE CONCAT('%', @Search, '%')
                        OR CustomerNumber LIKE CONCAT('%', @Search, '%'))
                    ORDER BY CreatedDate DESC
                    LIMIT @PageSize OFFSET @Offset";

                var parameters = new
                {
                    Search = search,
                    Offset = offset,
                    PageSize = pageSize
                };

                var total = await connection.ExecuteScalarAsync<int>(countQuery, parameters);

                var items = await connection.QueryAsync<Enquiry>(dataQuery, parameters);

                return new PagedResult<Enquiry>
                {
                    Items = items.ToList(),
                    Total = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving enquiries list.");
                throw;
            }
        }
        public async Task<Enquiry?> GetEnquiryById(int id)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
            SELECT
                Id,
                CustomerName,
                CustomerNumber,
                CustomerEmail,
                Description,
                Status,
                Source,
                CreatedDate
            FROM Customer_Enquiries
            WHERE Id = @Id";

                var enquiry = await connection.QuerySingleOrDefaultAsync<Enquiry>(query, new { Id = id });
                return enquiry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving enquiry by id.");
                throw;
            }
        }
        public async Task<Enquiry?> UpdateEnquiryStatus(int id, string status)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
            UPDATE Customer_Enquiries
            SET Status = @Status
            WHERE Id = @Id;

            SELECT
                Id,
                CustomerName,
                CustomerNumber,
                CustomerEmail,
                Description,
                Status,
                Source,
                CreatedDate
            FROM Customer_Enquiries
            WHERE Id = @Id";

                var enquiry = await connection.QuerySingleOrDefaultAsync<Enquiry>(query, new { Id = id, Status = status });
                return enquiry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating enquiry status.");
                throw;
            }
        }
        public async Task<List<Enquiry>> GetEnquiriesBySource(string source)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
            SELECT
                Id,
                CustomerName,
                CustomerNumber,
                CustomerEmail,
                Description,
                Status,
                Source,
                CreatedDate
            FROM Customer_Enquiries
            WHERE Source = @Source
            ORDER BY CreatedDate DESC";

                var enquiries = await connection.QueryAsync<Enquiry>(query, new { Source = source });
                return enquiries.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving enquiries by source.");
                throw;
            }
        }
    }
}
