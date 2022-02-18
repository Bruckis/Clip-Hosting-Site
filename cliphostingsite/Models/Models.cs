using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace cliphostingsite.Models
{
    public class loginModel
    {
        [Required(ErrorMessage = "Required")]
        public string username { get; set; }
        [Required(ErrorMessage = "Required")]
        public string password { get; set; }
        public string returnUrl { get; set; }
        
    }

    public class registerModel
    {
        [Required(ErrorMessage = "Required")]
        public string username { get; set; }
        [Required(ErrorMessage = "Required")]
        public string email { get; set; }
        [Required(ErrorMessage = "Required")]
        public string password { get; set; }
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string confirmPassword { get; set; }
    }

    public class clipModel
    {
        public string title { get; set; }
        public string description { get; set; }
        public string clipid { get; set; }

    }

    public class findClip
    {
        public string name { get; set; }
    }
    
    public class userModel
    {
        public string username { get; set; }
        public string avatar { get; set; }
        public string email { get; set; }
        public string loggedIn { get; set; }
    }
    public class boolModel
    {
        public bool Bool { get; set; }
    }
}