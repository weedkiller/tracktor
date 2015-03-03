using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tracktor.service
{
    [DataContract]
    public class TEntryDto
    {
        [DataMember]
        public int TEntryID { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
        [DataMember]
        public DateTime? EndDate { get; set; }
        [DataMember]
        public decimal Contrib { get; set; }
    }

    [DataContract]
    public class TTaskDto
    {
        [DataMember]
        public int TTaskID { get; set; }
        [DataMember]
        public int TProjectID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public bool IsObsolete { get; set; }
    }

    [DataContract]
    public class TProjectDto
    {
        [DataMember]
        public int TProjectID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public bool IsObsolete { get; set; }
        [DataMember]
        public List<TTaskDto> TTasks { get; set; }
    }

    [DataContract]
    public class TracktorReportDto
    {
        [DataMember]
        public Dictionary<DateTime, double> DayContribs { get; set; }
        [DataMember]
        public Dictionary<int, double> TaskContribs { get; set; }
        [DataMember]
        public DateTime? StartDate { get; set; }
        [DataMember]
        public DateTime EndDate { get; set; }
    }
}
