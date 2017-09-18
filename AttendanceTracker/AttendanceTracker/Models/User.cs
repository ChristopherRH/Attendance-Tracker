using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendanceTracker.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool Highlight { get; set; }
    }
}