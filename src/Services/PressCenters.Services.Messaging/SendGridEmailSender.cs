namespace PressCenters.Services.Messaging
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using SendGrid;
    using SendGrid.Helpers.Mail;

    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridClient client;

        public SendGridEmailSender(string apiKey)
        {
            this.client = new SendGridClient(apiKey);
        }

        public EmailBuilder EmailBuilder()
        {
            return new EmailBuilder();
        }

        public async Task SendEmailAsync(Email email)
        {
            if (string.IsNullOrWhiteSpace(email.Subject) && string.IsNullOrWhiteSpace(email.HtmlContent))
            {
                throw new ArgumentException("Subject and message should be provided.");
            }

            var fromAddress = new EmailAddress(email.From, email.FromName);
            var toAddress = new EmailAddress(email.To);
            var message = MailHelper.CreateSingleEmail(fromAddress, toAddress, email.Subject, null, email.HtmlContent);
            if (email.Attachments?.Any() == true)
            {
                foreach (var attachment in email.Attachments)
                {
                    message.AddAttachment(attachment.FileName, Convert.ToBase64String(attachment.Content), attachment.MimeType);
                }
            }

            try
            {
                var response = await this.client.SendEmailAsync(message);
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Body.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
