using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pet_Management_System.Models
{
    public class OrderReturn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrdReturnId { get; set; }
        [ForeignKey("OrderedProduct")]
        [Column(Order = 0)]
        public int ProductId { get; set; }
        [ForeignKey("OrderedProduct")]
        [Column(Order = 1)]
        public int CustomerOrderId { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public bool IsReschedule { get; set; }
        public DateTime Created { get; set; }
        public string DeclineReason { get; set; }
        public string custSignature { get; set; }
        public string drivSignature { get; set; }
        public virtual OrderedProduct OrderedProduct { get; set; }
    }
}