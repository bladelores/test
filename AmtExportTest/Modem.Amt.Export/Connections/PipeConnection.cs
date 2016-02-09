using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modem.Amt.Export;
using Modem.Amt.Export.Data;
using Modem.Amt.Export.Utility;
using System.IO;
using System.IO.Pipes;

namespace Modem.Amt.Export.Connections
{
    public class PipeConnection: IRealtimeConnection
    {
        public NamedPipeClientStream PipeClient { get; set; }
        private long wellboreId;
        private List<Parameter> parameters;
		private List<decimal> limitPoints;

        public void ConfigureConnection(long wellboreId, List<Parameter> parameters, List<decimal> limitPoints)
        {
            this.wellboreId = wellboreId;
            this.parametersWithLimit = parametersWithLimit;
			this.limitPoints = limitPoints;
        }
        
        public async System.Threading.Tasks.Task<decimal[]> GetNewData()
        {
            var data = await Task<decimal[]>.Run(() =>
            {
                StreamString reader = new StreamString(PipeClient);
                var s = reader.ReadString();
                if (s.Equals("end")) return null;
                List<string> parsedNumbers = s.Split(' ').ToList();

                List<decimal?> numbers = new List<decimal?>();
                decimal value;
                parsedNumbers.ForEach(x =>
                {
                    if (Decimal.TryParse(x, out value))
                        numbers.Add(value);
                    else
                        numbers.Add(null);
                });

				List<decimal> processedData = DataProcess.ProcessNewData(numbers, parameters, limitPoints);
				
				limitPoints = processedData;
				
                return processedData.ToArray();
            });
            Task.WaitAll();
            return data;
        }
    }
}
