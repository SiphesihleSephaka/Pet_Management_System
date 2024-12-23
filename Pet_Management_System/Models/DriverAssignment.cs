using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Pet_Management_System.Models
{
    public class DriverAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssDrivId { get; set; }
        [ForeignKey("OrderedProduct")]
        [Column(Order = 0)]
        public int ProductId { get; set; }
        [ForeignKey("OrderedProduct")]
        [Column(Order = 1)]
        public int CustomerOrderId { get; set; }
        public int DrivId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        [DataType(DataType.Date)]
        public string DeliveryDate { get; set; }
        [DataType(DataType.Time)]
        public string DeliveryTime { get; set; }
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
        [DisplayName("Delivery due by")]
        public string GenDeliveryDate { get; set; }
        [DisplayName("Preffared Time")]
        public string preffaredTime { get; set; }
        public virtual OrderedProduct OrderedProduct { get; set; }
        [ForeignKey("DrivId")]
        public virtual Driver Driver { get; set; }
    }
}