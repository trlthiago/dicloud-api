using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dicloud_api.Models
{
    public class DatasetModel
    {
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string PatientAge { get; set; }
        public string CommandField { get; set; }
        public string CommandDatasetType { get; set; }
        public string QueryRetrieveLevel { get; set; }
        public string ModalitiesInStudy { get; set; }
        public string StudyDescription { get; set; }
        public string PatientBirthDate { get; set; }
        public string StudyID { get; set; }
        public string NumberOfStudyRelatedSeries { get; set; }
        public string NumberOfStudyRelatedInstances { get; set; }
        public string StudyInstanceUID { get; set; }
    }
}
