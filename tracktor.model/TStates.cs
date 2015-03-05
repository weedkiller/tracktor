using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tracktor.model
{
    public enum TState
    {
        Idle = 0,
        InProgress = 1
    }

    public enum TTrigger
    {
        Start = 1,
        Stop = 2
    }
}
