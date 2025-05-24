using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.Entities
{
    public class Employee
    {
        public int id { get; set; }
        public required string name { get; set; }
        public required string email { get; set; }
        public int age { get; set; }
        public string? address { get; set; }
        public required string joinedDate { get; set; }

        // Foreign Key
        public int DepartmentId { get; set; }

        // Navigation Property (EF automatically maps this relationship)
        public Department? Department { get; set; }

        public string? photoFile { get; set; }
    }
}
