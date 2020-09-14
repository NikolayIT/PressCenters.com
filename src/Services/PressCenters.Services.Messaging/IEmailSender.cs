namespace PressCenters.Services.Messaging
{
    using System.Threading.Tasks;

    public interface IEmailSender
    {
        Task SendEmailAsync(Email email);

        EmailBuilder EmailBuilder();
    }
}
