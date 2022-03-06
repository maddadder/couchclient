﻿using System;
namespace couchclient.Models
{
    public class ProfileCreateRequestCommand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public Profile GetProfile()
        {
            return new Profile
            {
		        Pid = new Guid(),
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Password = this.Password
            };
        }
    }
}
