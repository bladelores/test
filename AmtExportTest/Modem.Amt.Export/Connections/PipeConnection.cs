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
        
        public async System.Threading.Tasks.Task<List<string[]>> GetNewData()
        {
            var data = await Task<List<string[]>>.Run(() =>
            {
                StreamString reader = new StreamString(PipeClient);
                List<string[]> inputData = new List<string[]>();

                var s = reader.ReadString();

                if (s.Equals("end")) return null;

                while (!s.Equals("send"))
                {
                    string[] parsedData = s.Split(',').ToArray();

                    string[] resultArray = new string[parsedData.Length];
                    resultArray[parsedData.Length - 1] = parsedData[parsedData.Length - 1];

                    decimal value;
                    for (int i = 0; i < parsedData.Length - 1; i++)
                    {
                        if (Decimal.TryParse(parsedData[i], out value))
                            resultArray[i] = value.ToString();
                        else
                            resultArray[i] = "";
                    }                  

                    inputData.Add(resultArray);

                    s = reader.ReadString();
                }

					
                return inputData;
            });
            Task.WaitAll();
            return data;
        }
    }
}
