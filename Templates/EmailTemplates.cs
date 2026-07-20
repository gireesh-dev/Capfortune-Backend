namespace CapfortuneBE.Templates
{
    public static class EmailTemplates
    {
        public static string EnquiryConfirmation(string customerName, string description)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #4A90E2;'>Thank you for your Enquiry!</h2>
                        <p>Dear <strong>{customerName}</strong>,</p>
                        <p>We have received your enquiry and our team will get back to you shortly.</p>
                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; margin: 20px 0;'>
                            <p><strong>Your Enquiry:</strong></p>
                            <p>{description}</p>
                        </div>
                        <p>If you have any questions, feel free to reach out to us.</p>
                        <br/>
                        <p>Best Regards,</p>
                        <p><strong>Capfortune Team</strong></p>
                    </div>
                </body>
                </html>";
        }

        public static string NewEnquiryAdminNotification(string customerName, string customerNumber, string customerEmail, string? description, string? source)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #4A90E2;'>New Enquiry Submitted</h2>
                        <p>A new enquiry has been submitted on the website.</p>
                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; margin: 20px 0;'>
                            <p><strong>Customer Name:</strong> {customerName}</p>
                            <p><strong>Customer Number:</strong> {customerNumber}</p>
                            <p><strong>Customer Email:</strong> {customerEmail}</p>
                            <p><strong>Source:</strong> {source}</p>
                            <p><strong>Description:</strong></p>
                            <p>{description}</p>
                        </div>
                        <p>Please follow up with the customer at your earliest convenience.</p>
                        <br/>
                        <p><strong>Capfortune System</strong></p>
                    </div>
                </body>
                </html>";
        }

        public static string EnquiryStatusUpdate(string customerName, string status)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #4A90E2;'>Enquiry Status Update</h2>
                        <p>Dear <strong>{customerName}</strong>,</p>
                        <p>Your enquiry status has been updated.</p>
                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; margin: 20px 0;'>
                            <p><strong>New Status:</strong> {status}</p>
                        </div>
                        <p>If you have any questions, feel free to reach out to us.</p>
                        <br/>
                        <p>Best Regards,</p>
                        <p><strong>Capfortune Team</strong></p>
                    </div>
                </body>
                </html>";
        }
    }
}