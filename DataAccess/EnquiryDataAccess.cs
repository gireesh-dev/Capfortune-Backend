using CapfortuneBE.Interface;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static CapfortuneBE.Models.EnquiryDTO;

namespace CapfortuneBE.DataAccess
{
    public class EnquiryDataAccess : IEnquiryDataAccess
    {
        public readonly ILogger<EnquiryDataAccess> _logger;
        private readonly IConfiguration _configuration;

        public EnquiryDataAccess(ILogger<EnquiryDataAccess> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
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
                    OUTPUT
                        INSERTED.Id,
                        INSERTED.CustomerName,
                        INSERTED.CustomerNumber,
                        INSERTED.CustomerEmail,
                        INSERTED.Description,
                        INSERTED.Status,
                        INSERTED.Source,
                        INSERTED.CreatedDate
                    VALUES
                    (
                        @CustomerName,
                        @CustomerNumber,
                        @CustomerEmail,
                        @Description,
                        @Status,
                        @Source,
                        GETDATE()
                    );
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
                        OR CustomerName LIKE '%' + @Search + '%'
                        OR CustomerEmail LIKE '%' + @Search + '%'
                        OR CustomerNumber LIKE '%' + @Search + '%')";

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
                        OR CustomerName LIKE '%' + @Search + '%'
                        OR CustomerEmail LIKE '%' + @Search + '%'
                        OR CustomerNumber LIKE '%' + @Search + '%')
                    ORDER BY CreatedDate DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

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
