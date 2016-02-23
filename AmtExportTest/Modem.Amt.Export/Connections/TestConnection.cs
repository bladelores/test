using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modem.Amt.Export;
using Modem.Amt.Export.Data;

namespace Modem.Amt.Export.Connections
{
    public class TestConnection: IRealtimeConnection
    {
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
            var random = new Random();
            var randomData = await Task<string[]>.Run(() =>
            {

                var randomArray = new string[5];
                for (int i = 0; i < 5; i++) randomArray[i] = random.Next(0, 100).ToString();
                return randomArray;
            });
            Task.WaitAll();

            var processedData = new List<string[]>();
            processedData.Add(randomData);
            //var amtData = new AmtData();
            //amtData.Data.Add(randomData);
            if (random.Next(0, 10) == 9)
                return null;
            else
                return processedData;
        }
    }
}
