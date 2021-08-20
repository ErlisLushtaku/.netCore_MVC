using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        // [Display(Name="Company Name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string StreetAddress { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(50)]
        public string State { get; set; }

        [MaxLength(50)]
        public string PostalCode { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }
    }
}
