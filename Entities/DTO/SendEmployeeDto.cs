namespace EMS.Entities.DTO
{
    public class SendEmployeeDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int age { get; set; }
        public string? address { get; set; }
        public string joinedDate { get; set; }
        public string? photoFile { get; set; }
        public string department { get; set; }
    }
}
