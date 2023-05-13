using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalHajurKo.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string OrderNo { get; set; }
        public string CustomerID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public int Rate { get; set; }   
        public DateTime OrderDate { get; set; }
        public DateTime ReturnDate { get; set; }

        public string PhotoUrl { get; set; }
        [NotMapped]

        public IFormFile Photo { get; set; }


        
    }
}
