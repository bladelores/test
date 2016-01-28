using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modem.Amt.Export.Data
{
    public class ParameterWithLimit: Parameter
    {
        public decimal LimitPoint { get; set; }
    }
}
