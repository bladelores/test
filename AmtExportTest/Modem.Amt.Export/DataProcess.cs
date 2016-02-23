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
        public static List<decimal[]> ProcessNewData(List<string[]> newData, List<Parameter> parameters, List<decimal> limitPoints)
        {
            var processedData = new List<decimal[]>();

            var oldValues = new decimal[parameters.Count];
            for (var i = 0; i < parameters.Count; ++i)
                oldValues[i] = limitPoints[i];

            foreach (string[] row in newData)
            {
                var r = new decimal[parameters.Count + 1];
                
                for (var i = 0; i < parameters.Count; ++i)
                {
                    var val = row[i];
                    r[i] = val == "" ? oldValues[i] : Convert.ToDecimal(val) / parameters[i].Multiplier;
                }
                r[parameters.Count] = Convert.ToDateTime(row[parameters.Count]).Ticks;               
                
                oldValues = r;

                processedData.Add(r);
            }
            
            return processedData;
        }
    }
}
