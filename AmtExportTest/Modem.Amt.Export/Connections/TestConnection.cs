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

        public void ConfigureConnection(long wellboreId, List<Parameter> parameters) {
            this.wellboreId = wellboreId;
            this.parameters = parameters;
        }
        
        public async System.Threading.Tasks.Task<decimal[]> GetNewData()
        {         
            var randomData = await Task<decimal[]>.Run(() =>
            {     
                return DataProcess.TestConnectionProcess();
            });
            Task.WaitAll();

            //var amtData = new AmtData();
            //amtData.Data.Add(randomData);
            var random = new Random();
            if (random.Next(0, 10) == 9)
                return null;
            else
                return randomData;
        }
    }
}
