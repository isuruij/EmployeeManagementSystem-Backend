namespace EMS.Entities.DTO
{
    public class UploadEmployeeImageRequest
    {
        // The file property. Ensure the property name matches your intended name ("photo").
        public required IFormFile photo { get; set; }

        // The ID property.
        public int id { get; set; }

    }
}
