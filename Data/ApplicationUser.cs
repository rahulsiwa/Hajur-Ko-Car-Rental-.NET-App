using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalHajurKo.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public string PhotoUrl { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }

        public string FileUrl { get; set; }
        [NotMapped]
        public IFormFile LicenseFile { get; set; }
    }
}
