﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Modem.Amt.Export;
using Modem.Amt.Export.Data;
using Modem.Amt.Export.Connections;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace AmtExportTest.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetData(long wellboreId, DateTime startTime, DateTime? endTime, List<Parameter> parameters)
        {
            Response.BufferOutput = false;

            //IRealtimeConnection testConnection;
            var pipeConnection = new PipeConnection();
            var limitPoints = new List<decimal>();

            using (var store = new Store())
            {
                StringBuilder queryBuilder = new StringBuilder();
                parameters.ForEach(x => store.FillParameter(x));
                parameters.ForEach(x => queryBuilder.Append(",").Append(x.Code));
                limitPoints = store.GetLimitPoints(wellboreId, startTime, parameters, queryBuilder);

                var data = store.GetData(parameters, new Wellbore { Id = wellboreId }, startTime, (DateTime)endTime);
                var processedData = DataProcess.ProcessNewData(data, parameters, limitPoints);

                limitPoints = processedData.Last().Take(processedData.Last().Count - 1).ToList();

                Response.Write(processedData);
                Response.Flush();
            }

            if (endTime == null)
                return Json(new { success = true });

            pipeConnection.ConfigureConnection(wellboreId, parameters, limitPoints);

            pipeConnection.PipeClient =
                       new NamedPipeClientStream(".", "PipeConnection",
                           PipeDirection.InOut, PipeOptions.Asynchronous);
            pipeConnection.PipeClient.Connect();

            while (true)
            {
                var newData = await pipeConnection.GetNewData();
                if (newData == null) break;

                var processedRealTimeData = DataProcess.ProcessNewData(newData, parameters, limitPoints);
                limitPoints = processedRealTimeData.Last().Take(processedData.Last().Count - 1).ToList();

                Response.Write(DataProcess.ProcessNewData(newData, parameters, limitPoints));
                Response.Flush();
            }
            
            Response.End();
            pipeConnection.PipeClient.Close();
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetParameters()
        {
            using (var store = new Store())
                return Json(store.Parameters.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWellbores(DateTime startTime, DateTime? endTime)
        {
            using (var store = new Store())
            {
                var correctIntervals = new List<ContinuousInterval>();
                
                foreach (var wellbore in store.Wellbores)
                {
                    var interval = store.ContinuousIntervals.Where(x => x.WellboreId == wellbore.Id).SingleOrDefault();
                    if (endTime == null) endTime = interval.StartTime;
                    if (interval != null && endTime >= interval.StartTime && startTime <= interval.FinishTime)
                        correctIntervals.Add(interval);
                }
                return Json(correctIntervals.Select(x => new
                {
                    wellboreId = x.WellboreId,
                    wellboreStateId = store.WellboreStates.Any(y => y.WellboreId == x.WellboreId)
                                      ? (long?)store.WellboreStates.Where(y => y.WellboreId == x.WellboreId).FirstOrDefault().StateId
                                      : null,
                    startTime = x.StartTime,
                    endTime = x.FinishTime
                })
                    .ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetWellboreStates()
        {
            using (var store = new Store())
                return Json(store.WellboreStates.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnits()
        {
            using (var store = new Store())
                return Json(store.Units.ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}