using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.AMF3
{
    public interface IAmf3Encode
    {
        Sinan.Collections.BytesSegment Encode(int command, IList objs);
    }
}
