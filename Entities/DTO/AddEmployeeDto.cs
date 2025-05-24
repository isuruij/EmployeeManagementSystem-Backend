namespace EMS.Entities.DTO
{
    public class AddEmployeeDto
    {
        public required string name { get; set; }
        public required string email { get; set; }
        public int age { get; set; }
        public string? address { get; set; }
        public required string joinedDate { get; set; }

        // Use DepartmentId instead of a string department name
        public int DepartmentId { get; set; }

        public IFormFile? photo { get; set; }
    }
}
