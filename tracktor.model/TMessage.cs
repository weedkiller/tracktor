using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tracktor.model
{
    public enum TMessageType
    {
        NotSet = 0,
        Start = 1,
        Stop = 2
    }

    public class TMessage
    {
        public TMessageType TMessageType { get; set; }
        public int TaskId { get; set; }
    }
}
