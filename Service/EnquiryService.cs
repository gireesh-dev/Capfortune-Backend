using CapfortuneBE.DataAccess;
using CapfortuneBE.Interface;
using CapfortuneBE.Templates;
using Microsoft.Extensions.Configuration;
using System.Data;
using static CapfortuneBE.Models.EnquiryDTO;

namespace CapfortuneBE.Service
{
    public class EnquiryService
    {
        private readonly IEnquiryDataAccess _enquiryDataAccess;
        private readonly ILogger<EnquiryService> _logger;
        private readonly MailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly IUserDataAccess _errorLogDataAccess;

        public EnquiryService(IEnquiryDataAccess enquiryDataAccess, ILogger<EnquiryService> logger, MailService mailService, IConfiguration configuration, IUserDataAccess errorLogDataAccess)
        {
            _enquiryDataAccess = enquiryDataAccess;
            _logger = logger;
            _mailService = mailService;
            _configuration = configuration;
            _errorLogDataAccess = errorLogDataAccess;
        }

        public async Task<Enquiry> CreateEnquiry(CreateEnquiryRequest request)
        {
            try
            {
                request.Status = Constants.Constants.EnquiryStatus.PENDING;

                var createdEnquiry = await _enquiryDataAccess.CreateEnquiry(request);

                try
                {
                    var adminEmail = _configuration["AdminSettings:NotificationEmail"] ?? "surendrachagantipati@gmail.com";
                    var adminBccEmail = _configuration["AdminSettings:NotificationBcc"];
                    await _mailService.SendMailAsync(
                        toEmail: adminEmail,
                        toName: "Capfortune Admin",
                        subject: "New Enquiry Submitted",
                        htmlBody: EmailTemplates.NewEnquiryAdminNotification(
                            createdEnquiry.CustomerName,
                            createdEnquiry.CustomerNumber,
                            createdEnquiry.CustomerEmail,
                            createdEnquiry.Description,
                            createdEnquiry.Source),
                        bccEmail: adminBccEmail
                    );
                }
                catch (Exception mailEx)
                {
                    _logger.LogError(mailEx, "Enquiry {EnquiryId} was created but the admin notification email failed to send.", createdEnquiry.Id);
                    await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(CreateEnquiry), mailEx);
                }

                try
                {
                    await _mailService.SendMailAsync(
                        toEmail: createdEnquiry.CustomerEmail,
                        toName: createdEnquiry.CustomerName,
                        subject: "We've received your enquiry",
                        htmlBody: EmailTemplates.EnquiryConfirmation(
                            createdEnquiry.CustomerName,
                            createdEnquiry.Description ?? string.Empty)
                    );
                }
                catch (Exception mailEx)
                {
                    _logger.LogError(mailEx, "Enquiry {EnquiryId} was created but the customer confirmation email failed to send.", createdEnquiry.Id);
                    await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(CreateEnquiry), mailEx);
                }

                return createdEnquiry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an enquiry.");
                await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(CreateEnquiry), ex);
                throw;
            }
        }
        public async Task<PagedResult<Enquiry>> GetEnquiriesList(int page, int pageSize, string? search)
        {
            try
            {
                var result = await _enquiryDataAccess.GetEnquiriesList(page, pageSize, search);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving enquiries list.");
                await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(GetEnquiriesList), ex);
                throw;
            }
        }
        public async Task<Enquiry?> GetEnquiryById(int id)
        {
            try
            {
                return await _enquiryDataAccess.GetEnquiryById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving enquiry by id.");
                await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(GetEnquiryById), ex);
                throw;
            }
        }
        public async Task<Enquiry?> UpdateEnquiryStatus(int id, string status)
        {
            try
            {
                var updatedEnquiry = await _enquiryDataAccess.UpdateEnquiryStatus(id, status);

                if (updatedEnquiry != null)
                {
                    try
                    {
                        await _mailService.SendMailAsync(
                            toEmail: updatedEnquiry.CustomerEmail,
                            toName: updatedEnquiry.CustomerName,
                            subject: "Your enquiry status has been updated",
                            htmlBody: EmailTemplates.EnquiryStatusUpdate(
                                updatedEnquiry.CustomerName,
                                updatedEnquiry.Status)
                        );
                    }
                    catch (Exception mailEx)
                    {
                        _logger.LogError(mailEx, "Enquiry {EnquiryId} status was updated but the customer notification email failed to send.", id);
                        await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(UpdateEnquiryStatus), mailEx);
                    }
                }

                return updatedEnquiry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating enquiry status.");
                await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(UpdateEnquiryStatus), ex);
                throw;
            }
        }
        public async Task<Enquiry?> ReplyToEnquiry(ReplyToEnquiryRequest request)
        {
            try
            {
                var enquiry = await _enquiryDataAccess.GetEnquiryById(request.EnquiryId);
                if (enquiry == null)
                {
                    return null;
                }

                await _mailService.SendMailAsync(
                    toEmail: enquiry.CustomerEmail,
                    toName: enquiry.CustomerName,
                    subject: request.Subject,
                    htmlBody: EmailTemplates.CustomReply(request.Message)
                );

                return await _enquiryDataAccess.SaveReply(request.EnquiryId, request.Subject, request.Message, Constants.Constants.EnquiryStatus.REPLIED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while replying to enquiry {EnquiryId}.", request.EnquiryId);
                await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(ReplyToEnquiry), ex);
                throw;
            }
        }
        public async Task<List<Enquiry>> GetEnquiriesBySource(string source)
        {
            try
            {
                return await _enquiryDataAccess.GetEnquiriesBySource(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating enquiry status.");
                await _errorLogDataAccess.LogErrorAsync(Constants.Constants.Layers.Service, nameof(GetEnquiriesBySource), ex);
                throw;
            }
        }
    }
}
