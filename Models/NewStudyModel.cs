using Microsoft.AspNetCore.Http;

namespace dicloud_api.Models
{
    public class NewStudyModel
    {
        public string PatientName { get; set; }
        public string PatientID { get; set; }
        public IFormFile Image { get; set; }
    }
}
