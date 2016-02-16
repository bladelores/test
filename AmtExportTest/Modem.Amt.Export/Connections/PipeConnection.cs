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
            this.parameters = parameters;
			this.limitPoints = limitPoints;
        }
        
        public async System.Threading.Tasks.Task<List<decimal[]>> GetNewData()
        {
            var data = await Task<List<decimal[]>>.Run(() =>
            {
                StreamString reader = new StreamString(PipeClient);
                List<decimal?[]> inputData = new List<decimal?[]>();

                var s = reader.ReadString();

                if (s.Equals("end")) return null;

                while (!s.Equals("send"))
                {
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

                    inputData.Add(numbers.ToArray());

                    s = reader.ReadString();
                }

				List<decimal[]> processedData = DataProcess.ProcessNewData(inputData, parameters, limitPoints);
					
                return processedData;
            });
            Task.WaitAll();
            return data;
        }
    }
}
