using System.ComponentModel.DataAnnotations;

namespace SmartExpense.Models
{
    public class ContactModel
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
