using Azure.Core;
using EMS.Data;
using EMS.Entities;
using EMS.Entities.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Controllers
{
    [Route("users/")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly DataContext _context;

        public EmployeeController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<SendEmployeeDto>>> GetAllEmployees()
        {
            var employeeDtos = await _context.Employees
                .Include(e => e.Department) // Ensure Department data is loaded
                .Select(e => new SendEmployeeDto
                {
                    id = e.id,
                    name = e.name,
                    email = e.email,
                    age = e.age,
                    address = e.address,
                    joinedDate = e.joinedDate,
                    photoFile = e.photoFile,
                    department = e.Department != null ? e.Department.name : "No Department"
                })
                .ToListAsync();

            return Ok(employeeDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<SendEmployeeDto>> GetEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department) // Include Department details
                .FirstOrDefaultAsync(e => e.id == id);

            if (employee is null)
            {
                return NotFound("Employee Not Found");
            }

            var employeeDto = new SendEmployeeDto
            {
                id = employee.id,
                name = employee.name,
                email = employee.email,
                age = employee.age,
                address = employee.address,
                joinedDate = employee.joinedDate,
                photoFile = employee.photoFile,
                department = employee.Department?.name ?? "No Department"
            };


            return Ok(employeeDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, AddEmployeeDto addEmployeeDto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee is null)
            {
                return NotFound("Employee Not Found");
            }

            var department = await _context.Departments.FindAsync(addEmployeeDto.DepartmentId);
            if (department is null)
            {
                return NotFound("Department Not Found");
            }

            employee.name = addEmployeeDto.name;
            employee.age = addEmployeeDto.age;
            employee.address = addEmployeeDto.address;
            employee.email = addEmployeeDto.email;
            employee.joinedDate = addEmployeeDto.joinedDate;
            employee.DepartmentId = addEmployeeDto.DepartmentId;

            await _context.SaveChangesAsync();
            return Ok(employee);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee is null)
            {
                return NotFound("Employee Not Found");
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(employee);
        }


        [HttpPost("uploadphoto")]
        public async Task<IActionResult> UploadEmployeeImage([FromForm] UploadEmployeeImageRequest request)
        {

            var file = request.photo;
            var id = request.id;

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee is null)
            {
                return NotFound("Employee Not Found");
            }

            // Define the path
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Create a unique file name
            var fileName = $"{id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            var fileUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            employee.photoFile = fileUrl;
            await _context.SaveChangesAsync();

            return Ok(new { FileUrl = fileUrl, EmployeeId = id });
        }


        [HttpPost("register")]
        public async Task<ActionResult<Employee>> AddEmployee([FromForm] AddEmployeeDto addEmployeeDto)
        {
            var department = await _context.Departments.FindAsync(addEmployeeDto.DepartmentId);
            if (department is null)
            {
                return NotFound("Department Not Found");
            }

            var employeeEntity = new Employee()
            {
                name = addEmployeeDto.name,
                age = addEmployeeDto.age,
                address = addEmployeeDto.address,
                email = addEmployeeDto.email,
                joinedDate = addEmployeeDto.joinedDate,
                DepartmentId = addEmployeeDto.DepartmentId
            };

            await _context.Employees.AddAsync(employeeEntity);
            await _context.SaveChangesAsync();


            var file = addEmployeeDto.photo;
            if (file == null || file.Length == 0)
            {
                return Ok(employeeEntity);
            }

            // Define the path
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Create a unique file name
            var fileName = $"{employeeEntity.id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var fileUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            employeeEntity.photoFile = fileUrl;
            await _context.SaveChangesAsync();
            return Ok(employeeEntity);
        }

    }



    }

