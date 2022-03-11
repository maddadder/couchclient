﻿using System;
namespace couchclient.Models
{
    public class ProfileListRequestQuery
    {
        public string Search { get; set; }
        public int Limit { get; set; } = 100;
        public int Skip { get; set; } 
    }
}
