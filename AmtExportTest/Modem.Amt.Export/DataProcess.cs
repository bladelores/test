using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Modem.Amt.Export.Data;

namespace Modem.Amt.Export
{
    public static class DataProcess
    {
        public static List<decimal> ProcessNewData(List<decimal?> newData, List<ParameterWithLimit> parametersWithLimit)
        {
            var processedData = new List<decimal>();
            for (int i = 0; i < parametersWithLimit.Count; i++)
                processedData[i] = newData[i] == null
                    ? parametersWithLimit[i].LimitPoint
                    : Convert.ToDecimal(newData[i]) / parametersWithLimit[i].Multiplier;
            return processedData;
        }
    }
}
