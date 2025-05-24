namespace EMS.Entities.DTO
{
    public class SendAdminDto
    {
        public int id { get; set; }
        public required string name { get; set; }
        public required string email { get; set; }
        public required string mobileNumber { get; set; }
        public bool isMobileVerified { get; set; } = false;
    }
}
