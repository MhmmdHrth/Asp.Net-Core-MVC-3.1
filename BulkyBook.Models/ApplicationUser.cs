using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public int? CompanyId { get; set; } //nullable

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }

        [NotMapped] //means these properties will not be pushed to the database
        public string Role { get; set; }
    }
}