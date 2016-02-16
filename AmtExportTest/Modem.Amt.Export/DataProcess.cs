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
        public static List<decimal[]> ProcessNewData(List<decimal?[]> newData, List<Parameter> parameters, List<decimal> limitPoints)
        {
            var processedData = new List<decimal[]>();

            var oldValues = new decimal[parameters.Count];
            for (var i = 0; i < parameters.Count; ++i)
                oldValues[i] = limitPoints[i];

            foreach (decimal?[] row in newData)
            {
                var r = new decimal[parameters.Count];
                
                for (var i = 0; i < parameters.Count; ++i)
                {
                    r[i] = row[i] == null ? oldValues[i] : Convert.ToDecimal(row[i]) / parameters[i].Multiplier;
                }
                
                oldValues = r;

                processedData.Add(r);
            }
            
            return processedData;
        }
    }
}
