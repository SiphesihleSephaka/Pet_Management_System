using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Pet_Management_System.Models
{
    public class Driver
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DrivId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public bool IsAvailable { get; set; }
        [DisplayName("Vehicle Name")]
        public string CarName { get; set; }
        [DisplayName("Vehicle Model")]
        public string CarModel { get; set; }
        [DisplayName("Vehicle Registration")]
        public string CarReg { get; set; }
        [DisplayName("Vehicle Type")]
        public string CarType{ get; set; }
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}