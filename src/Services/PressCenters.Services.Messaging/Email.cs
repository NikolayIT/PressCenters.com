namespace PressCenters.Services.Messaging
{
    using System.Collections.Generic;
    using System.Linq;

    public class Email
    {
        public Email()
        {
            this.Attachments = Enumerable.Empty<EmailAttachment>();
        }

        public string From { get; set; }

        public string FromName { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string HtmlContent { get; set; }

        public IEnumerable<EmailAttachment> Attachments { get; set; }
    }
}
