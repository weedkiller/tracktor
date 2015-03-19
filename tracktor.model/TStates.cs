// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

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
