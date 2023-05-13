using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalHajurKo.Models
{
    public class DamageReport
    {

        public Guid Id { get; set; }
        public string OrderNo { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string CarModel { get; set; }

        public string DamagePart { get; set; }

        public int DamageFee { get; set; }


    }
}
