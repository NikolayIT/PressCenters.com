namespace PressCenters.Web.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Messaging;
    using PressCenters.Web.ViewModels.Contacts;

    public class ContactsController : BaseController
    {
        private const string Redirected = "Redirected";

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

            await this.emailSender.SendEmailAsync(
                model.Email,
                model.Name,
                GlobalConstants.SystemEmail,
                model.Title,
                model.Content);

            this.TempData[Redirected] = true;

            return this.RedirectToAction("ThankYou");
        }

        public IActionResult ThankYou()
        {
            if (this.TempData[Redirected] == null)
            {
                return this.NotFound();
            }

            return this.View();
        }
    }
}
