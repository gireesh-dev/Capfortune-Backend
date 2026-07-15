using CapfortuneBE.Interface;
using CapfortuneBE.Models;
using CapfortuneBE.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CapfortuneBE.Models.EnquiryDTO;

namespace CapfortuneBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnquiryController : ControllerBase
    {
        private readonly EnquiryService _enquiryService;
        private readonly IUserDataAccess _userDataAccess;

        public EnquiryController(EnquiryService enquiryService,IUserDataAccess userDataAccess)
        {
            _enquiryService = enquiryService;
            _userDataAccess = userDataAccess;
        }

        /// <summary>
        /// Creates a new customer enquiry.
        /// </summary>
        /// <param name="request">The enquiry data to create.</param>
        /// <returns>The newly created enquiry.</returns>
        /// <response code="201">Enquiry created successfully.</response>
        /// <response code="500">Internal server error.</response>
        [AllowAnonymous]
        [HttpPost("CreateEnquiry")]
        public async Task<IActionResult> CreateEnquiryAsync([FromBody] CreateEnquiryRequest request)
        {
            try
            {
                var result = await _enquiryService.CreateEnquiry(request);
                return ApiResponse.Created(result, "Enquiry created successfully.");
            }
            catch (Exception ex)
            {
                await _userDataAccess.LogErrorAsync(Constants.Constants.Layers.Controller, nameof(CreateEnquiryAsync), ex, request);
                return ApiResponse.Error(ex,null,Constants.Constants.Messages.ErrorMessage);
            }
        }

        /// <summary>
        /// Retrieves a paginated list of all enquiries, sorted by creation date descending.
        /// </summary>
        /// <param name="parameters">Pagination parameters (page, pageSize).</param>
        /// <returns>A paged list of enquiries.</returns>
        /// <response code="200">Enquiries retrieved successfully.</response>
        [HttpGet("GetEnquiriesList")]
        public async Task<IActionResult> GetEnquiriesListAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var result = await _enquiryService.GetEnquiriesList(page, pageSize, search);
                return ApiResponse.Paged(result.Items, result.Total, page, pageSize);
            }
            catch (Exception ex)
            {
                await _userDataAccess.LogErrorAsync(Constants.Constants.Layers.Controller, nameof(GetEnquiriesListAsync), ex, new{page,pageSize,search});
                return ApiResponse.Error(ex, null, Constants.Constants.Messages.ErrorMessage);
            }
        }

        /// <summary>
        /// Retrieves a single enquiry by its identifier.
        /// </summary>
        /// <param name="id">The enquiry identifier.</param>
        /// <returns>The enquiry with the specified identifier.</returns>
        /// <response code="200">Enquiry retrieved successfully.</response>
        /// <response code="404">Enquiry not found.</response>
        [HttpGet("GetEnquiryById")]
        public async Task<IActionResult> GetEnquiryByIdAsync([FromQuery] int id)
        {
            try
            {
                var result = await _enquiryService.GetEnquiryById(id);
                if (result == null)
                    return ApiResponse.NotFound("Enquiry not found.");
                return ApiResponse.Ok(result, "Enquiry retrieved successfully.");
            }
            catch (Exception ex)
            {
                await _userDataAccess.LogErrorAsync(Constants.Constants.Layers.Controller, nameof(GetEnquiryByIdAsync), ex, new { id });
                return ApiResponse.Error(ex, null, Constants.Constants.Messages.ErrorMessage);
            }
        }

        /// <summary>
        /// Updates the status of an existing enquiry.
        /// </summary>
        /// <param name="id">The enquiry identifier.</param>
        /// <param name="status">The new status value.</param>
        /// <returns>The updated enquiry.</returns>
        /// <response code="200">Status updated successfully.</response>
        /// <response code="404">Enquiry not found.</response>
        [HttpPut("UpdateEnquiryStatus")]
        public async Task<IActionResult> UpdateEnquiryStatusAsync([FromQuery] int id, [FromQuery] string status)
        {
            try
            {
                var result = await _enquiryService.UpdateEnquiryStatus(id, status);
                if (result == null)
                    return ApiResponse.NotFound("Enquiry not found.");
                return ApiResponse.Ok(result, "Status updated successfully.");
            }
            catch (Exception ex)
            {
                await _userDataAccess.LogErrorAsync(Constants.Constants.Layers.Controller, nameof(UpdateEnquiryStatusAsync), ex, new { id, status});
                return ApiResponse.Error(ex, null, Constants.Constants.Messages.ErrorMessage);
            }
        }
        /// <summary>
        /// Retrieves all enquiries by a given source.
        /// </summary>
        /// <param name="source">The source identifier to filter by.</param>
        /// <returns>A list of enquiries matching the source.</returns>
        /// <response code="200">Enquiries retrieved successfully.</response>
        [HttpGet("GetEnquiriesBySource")]
        public async Task<IActionResult> GetEnquiriesBySourceAsync([FromQuery] string source)
        {
            try
            {
                var result = await _enquiryService.GetEnquiriesBySource(source);
                return ApiResponse.Ok(result, "Enquiries retrieved successfully.");
            }
            catch (Exception ex)
            {
                await _userDataAccess.LogErrorAsync(Constants.Constants.Layers.Controller, nameof(GetEnquiriesBySourceAsync), ex, new { source });
                return ApiResponse.Error(ex, null, Constants.Constants.Messages.ErrorMessage);
            }
        }
    }
}
