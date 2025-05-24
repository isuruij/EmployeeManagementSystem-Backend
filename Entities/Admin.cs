using System.ComponentModel.DataAnnotations;

namespace EMS.Entities
{
    public class Admin
    {
        public int id { get; set; }
        public required string name { get; set; }
        public required string email { get; set; }

        public required string mobileNumber { get; set; }

        public required string password { get; set; }
        public bool isEmailVerified { get; set; } = false;
        public bool isMobileVerified { get; set; } = false;

        public string? otp { get; set; }
    }
}
