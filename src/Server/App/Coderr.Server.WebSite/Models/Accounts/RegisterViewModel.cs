﻿using System.ComponentModel.DataAnnotations;

namespace Coderr.Server.WebSite.Models.Accounts
{
    public class RegisterViewModel : LoginResult
    {
        [Required, Display(Description = "Used to send notification and password resets.")]
        public string Email { get; set; }


        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required, Compare("Password2")]
        public string Password { get; set; }

        [Display(Name = "Retype password")]
        public string Password2 { get; set; }

        [Required]
        public bool Terms { get; private set; }

        [Required]
        public string UserName { get; set; }

        public string ReturnUrl { get; set; }
    }
}