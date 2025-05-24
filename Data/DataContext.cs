using EMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace EMS.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Admin> Admins { get; set; }
    }
}
  