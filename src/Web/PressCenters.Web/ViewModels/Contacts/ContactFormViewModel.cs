namespace PressCenters.Web.ViewModels.Contacts
{
    using System.ComponentModel.DataAnnotations;

    using PressCenters.Web.Infrastructure;

    public class ContactFormViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Моля въведете вашите имена")]
        [Display(Name = "Вашите имена")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Моля въведете вашият email адрес")]
        [EmailAddress(ErrorMessage = "Моля въведете валиден email адрес")]
        [Display(Name = "Вашият email адрес")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Моля въведете заглавие на съобщението")]
        [StringLength(100, ErrorMessage = "Заглавието трябва да е поне {2} и не повече от {1} символа.", MinimumLength = 5)]
        [Display(Name = "Заглавие на съобщението")]
        public string Title { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Моля въведете съдържание на съобщението")]
        [StringLength(10000, ErrorMessage = "Съобщението трябва да е поне {2} символа.", MinimumLength = 20)]
        [Display(Name = "Съдържание на съобщението")]
        public string Content { get; set; }

        [GoogleReCaptchaValidation]
        public string RecaptchaValue { get; set; }
    }
}
