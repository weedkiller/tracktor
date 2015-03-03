using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tracktor.service
{
    [DataContract]
    public class CContext
    {
        [DataMember]
        public int TUserID { get; set; }
        [DataMember]
        public int UTCOffset { get; set; }
    }

    [DataContract]
    public class CModel
    {
        [DataMember]
        public List<TProjectDto> Projects { get; set; }
    }
}
