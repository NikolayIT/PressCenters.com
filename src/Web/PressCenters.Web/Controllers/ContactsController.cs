namespace PressCenters.Web.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Web.ViewModels.Contacts;

    public class ContactsController : BaseController
    {
        private readonly IRepository<ContactFormEntry> contactsRepository;

        private readonly IEmailSender emailSender;

        public ContactsController(IRepository<ContactFormEntry> contactsRepository, IEmailSender emailSender)
        {
            this.contactsRepository = contactsRepository;
            this.emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ContactFormViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // TODO: Extract to IP provider (service)
            var ip = this.HttpContext.Connection.RemoteIpAddress.ToString();
            var contactFormEntry = new ContactFormEntry
                                   {
                                       Name = model.Name,
                                       Email = model.Email,
                                       Title = model.Title,
                                       Content = model.Content,
                                       Ip = ip,
                                   };
            await this.contactsRepository.AddAsync(contactFormEntry);
            await this.contactsRepository.SaveChangesAsync();

            // TODO: Extract email address to appsettings.json
            await this.emailSender.SendEmailAsync("presscenters@nikolay.it", model.Title, $"New message from {model.Name} ({model.Email}): {model.Content}");

            return this.View("ThankYou");
        }
    }
}
