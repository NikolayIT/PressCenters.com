namespace PressCenters.Services.Messaging
{
    using System.Collections.Generic;
    using System.Linq;

    public class EmailBuilder
    {
        private Email email;

        public EmailBuilder()
        {
            this.email = new Email();
        }

        public EmailBuilder AddFromAddress(string fromAddress)
        {
            this.email.From = fromAddress;
            return this;
        }

        public EmailBuilder AddFromName(string fromName)
        {
            this.email.FromName = fromName;
            return this;
        }

        public EmailBuilder AddToAddress(string toAdress)
        {
            this.email.To = toAdress;
            return this;
        }

        public EmailBuilder AddSubject(string subject)
        {
            this.email.Subject = subject;
            return this;
        }

        public EmailBuilder AddHtmlContent(string htmlContent)
        {
            this.email.HtmlContent = htmlContent;
            return this;
        }

        public EmailBuilder AddAttachments(IEnumerable<EmailAttachment> attachments)
        {
            if (attachments.Any())
            {
                this.email.Attachments = attachments;
            }

            return this;
        }

        public Email BuildEmail()
        {
            return this.email;
        }
    }
}
