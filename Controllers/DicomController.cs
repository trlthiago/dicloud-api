using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using dicloud_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dicloud_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DicomController : ControllerBase
    {
        private readonly Infra.MongoContext _db;
        private readonly ILogger<DicomController> _logger;

        public DicomController(ILogger<DicomController> logger, Infra.MongoContext db)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        [Route("search/patient/all")]
        public async Task<IEnumerable<DatasetModel>> SearchByPatientId()
        {
            var items = await _db.GetAllStudies();
            var results = items.Select(x => Load(x)).ToList();
            return results;
        }

        [HttpGet]
        [Route("search/patient/name/{name}")]
        public async Task<IEnumerable<DatasetModel>> SearchByPatientName(string name)
        {
            var items = await _db.GetStudiesByPatientName(name);

            var results = items.Select(x => Load(x)).ToList();

            return results;
        }

        [HttpGet]
        [Route("search/patient/id/{id}")]
        public async Task<IEnumerable<DatasetModel>> SearchByPatientId(string id)
        {
            var items = await _db.GetStudiesByPatientId(id);
            var results = items.Select(x => Load(x)).ToList();
            return results;
        }

        [HttpGet]
        [Route("download/{studyId}/{instanceNumber}")]
        public async Task<IActionResult> Download(string studyId, string instanceNumber)
        {
            //var path = GetPathForImage(id);

            var path = await _db.GetStudyPath(studyId, instanceNumber);

            await _db.UpdateLastAccessDate(studyId, instanceNumber);

            using Stream stream = System.IO.File.OpenRead(path);

            if (stream == null)
                return NotFound();

            return File(stream, "application/octet-stream");
        }

        [HttpGet]
        [Route("downloadzip/{studyId}/{instanceNumber}")]
        public async Task<IActionResult> DownloadZip(string studyId, string instanceNumber)
        {
            //var path = GetPathForImage(id);

            var path = await _db.GetStudyPath(studyId, instanceNumber);

            await _db.UpdateLastAccessDate(studyId, instanceNumber);

            if (!path.EndsWith(".7z"))
                path = await CompressionProvider.CompressAsync(path);

            using Stream stream = System.IO.File.OpenRead(path);

            if (stream == null)
                return NotFound();

            return File(stream, "application/octet-stream");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] NewStudyModel model)
        {
            if (!Request.Form.ContainsKey("SOPInstanceUID"))
                return UnprocessableEntity(new { message = "Missing SOPInstanceUID" });

            //if (await _db.StudyExists(Request.Form["StudyInstanceUID"], Request.Form["InstanceNumber"]))
            if (!Request.Form.ContainsKey("Test") && await _db.StudyExists(Request.Form["SOPInstanceUID"]))
                return Conflict();

            if (model.Image == null || model.Image.Length == 0)
                throw new Exception("File not found on POST");

            var path = @"C:\DICLOUD-API\" + Guid.NewGuid(); //model.Image.FileName;

            if (model.Image.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }
            }

            var image = new ImageModel
            {
                Path = path,
            };

            foreach (var item in Request.Form)
            {
                image.Tags.Add(item.Key, item.Value);
            }

            await _db.InsertStudy(image);

            return Ok(new { status = true, message = "Student Posted Successfully" });
        }

        private string GetPathForImage(string studyUid)
        {
            var path = Path.GetFullPath(@"C:\DICLOUD-API");
            path = Path.Combine(path, studyUid);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, studyUid) + ".dcm";
            return path;
        }

        public static DatasetModel Load(ImageModel model)
        {
            var type = typeof(DatasetModel);
            var props = type.GetProperties();
            var datasetModel = new DatasetModel();
            foreach (var prop in props)
            {
                string value = null;
                model.Tags.TryGetValue(prop.Name, out value);
                if (value != null)
                {
                    type.GetProperty(prop.Name).SetValue(datasetModel, value);
                }
            }

            return datasetModel;
        }
    }
}
