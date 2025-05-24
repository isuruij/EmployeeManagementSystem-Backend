using System.ComponentModel.DataAnnotations;

namespace EMS.Entities
{
    public class Department
    {
        public int id { get; set; }
        public required string name { get; set; }

    }
}
