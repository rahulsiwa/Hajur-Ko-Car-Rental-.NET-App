using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalHajurKo.Models
{
    public class DamageRequest
    {
        public Guid Id { get; set; }
        public string OrderNo { get; set; }
        public string CustomerID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Model{ get; set; }

        public string DamagePart { get; set; }


        public string PhotoUrl { get; set; }
        [NotMapped]

        public IFormFile DamagedPartPhoto { get; set; }
    }
}
