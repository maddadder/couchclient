using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class HashedUserProfile
    {
        [Required]
        public Guid Pid { get; set; }
        public string __T {get;set;}
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [EmailAddress]
        public string PreferredUsername { get; set; }
        [Required]
        public string Password {get;set;}
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }
    }
}
