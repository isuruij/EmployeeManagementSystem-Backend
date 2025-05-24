using EMS.Data;
using EMS.Entities;
using EMS.Entities.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EMS.Controllers
{
    [Route("departments/")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly DataContext _context;

        public DepartmentController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Department>>> GetAllDepartments()
        {
            var departments = await _context.Departments.ToListAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department is null)
            {
                return NotFound("Department Not Found");
            }
            return Ok(department);
        }

        [HttpPost("register")]
        public async Task<ActionResult<Department>> AddDepartment(AddDepartmentDto addDepartmentDto)
        {
            bool departmentExists = await _context.Departments
                .AnyAsync(d => d.name == addDepartmentDto.name);

            if (departmentExists)
            {
                return Conflict("Department name already exists.");
            }

            var departmentEntity = new Department()
            {
                name = addDepartmentDto.name
            };

            await _context.Departments.AddAsync(departmentEntity);
            await _context.SaveChangesAsync();

            return Ok(departmentEntity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, AddDepartmentDto addDepartmentDto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department is null)
            {
                return NotFound("Department Not Found");
            }

            // Check if another department already exists with the same name
            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.name == addDepartmentDto.name && d.id != id);

            if (existingDepartment != null)
            {
                return Conflict("Department name already exists. Please choose a different name.");
            }

            department.name = addDepartmentDto.name;

            await _context.SaveChangesAsync();
            return Ok(department);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.id == id);

            if (department is null)
            {
                return NotFound("Department Not Found");
            }

            // Check if there are associated employees
            bool hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == id);
            if (hasEmployees)
            {
                return BadRequest("Cannot delete department. Employees are associated with this department.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Ok(department);
        }

    }
}
