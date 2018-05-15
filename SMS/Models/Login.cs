using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models
{
    public class Login
    {


        [Required(ErrorMessage = "Please enter your password")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string password { get; set; }
        [Required (ErrorMessage ="Username required")]
        [Display(Name = "Username")]
        public string user { get; set; }
        [Display(Name ="User")]
        [Required(ErrorMessage = "Please select a user type")]
        public string type { get; set; }

        

        
    }
}