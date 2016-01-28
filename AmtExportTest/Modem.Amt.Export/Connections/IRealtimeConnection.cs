using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modem.Amt.Export.Data;

namespace Modem.Amt.Export.Connections
{
    public interface IRealtimeConnection
    {
        void ConfigureConnection(long wellboreId, List<Parameter> parameters);
        System.Threading.Tasks.Task<decimal[]> GetNewData();
    }
}
