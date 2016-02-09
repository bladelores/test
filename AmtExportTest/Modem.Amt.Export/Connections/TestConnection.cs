﻿using System;
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
            this.parametersWithLimit = parametersWithLimit;
			this.limitPoints = limitPoints;
        }
        
        public async System.Threading.Tasks.Task<decimal[]> GetNewData()
        {
            var random = new Random();
            var randomData = await Task<decimal[]>.Run(() =>
            {

                var randomArray = new decimal[5];
                for (int i = 0; i < 5; i++) randomArray[i] = random.Next(0, 100);
                return randomArray;
            });
            Task.WaitAll();

            //var amtData = new AmtData();
            //amtData.Data.Add(randomData);
            if (random.Next(0, 10) == 9)
                return null;
            else
                return randomData;
        }
    }
}
