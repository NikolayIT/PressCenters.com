namespace PressCenters.Services.Messaging
{
    using System.Threading.Tasks;

    public class NullMessageSender : IEmailSender
    {
        public EmailBuilder EmailBuilder()
        {
            return new EmailBuilder();
        }

        public Task SendEmailAsync(Email email)
        {
            return Task.CompletedTask;
        }
    }
}
