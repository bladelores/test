using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modem.Amt.Export;
using Modem.Amt.Export.Data;
using System.IO;
using System.IO.Pipes;


namespace Modem.Amt.Export.Connections
{
    public class PipeConnection: IRealtimeConnection
    {
        public NamedPipeClientStream PipeClient { get; set; }
        private long wellboreId;
        private List<Parameter> parameters;

        public void ConfigureConnection(long wellboreId, List<Parameter> parameters) {
            this.wellboreId = wellboreId;
            this.parameters = parameters;
        }
        
        public async System.Threading.Tasks.Task<decimal[]> GetNewData()
        {
            var data = await Task<decimal[]>.Run(() =>
            {
                return DataProcess.PipeConnectionProcess(PipeClient);              
            });
            Task.WaitAll();
            return data;
        }
    }
}
