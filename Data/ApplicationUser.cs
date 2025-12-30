using Microsoft.AspNetCore.Identity;

namespace FP.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
    }
}
