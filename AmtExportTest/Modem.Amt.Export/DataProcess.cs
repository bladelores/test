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
        public static List<decimal> ProcessNewData(List<decimal?> newData, List<Parameter> parameters, List<decimal> limitPoints)
        {
            var processedData = new List<decimal>();
            for (int i = 0; i < parameters.Count; i++)
                processedData[i] = newData[i] == null
                    ? limitPoints[i]
                    : Convert.ToDecimal(newData[i]) / parameters[i].Multiplier;
            return processedData;
        }
    }
}
