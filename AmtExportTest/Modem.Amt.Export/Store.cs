using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.Data.Common;
using System.Configuration;
using Modem.Amt.Export.Data;
using Modem.Amt.Export.Data.Mappings;

namespace Modem.Amt.Export
{
    public class Store : DbContext
    {
        #region Queries
        private const string ActualDataFunctionQuery = @"
            SELECT 
                    get_actual_data_on_time_limit(:p_wellbore_id,:p_time,:p_parameter_list, :p_limit)  
                FROM dual";
        
        #endregion
        public const decimal NoValue = -999666;
        protected IDbTransaction transaction;

        public IDbConnection Connection { set; get; }     
        
        public DbSet<Wellbore> Wellbores { set; get; }
        public DbSet<WellboreState> WellboreStates { set; get; }
        public DbSet<Unit> Units { set; get; }
        public DbSet<State> States { set; get; }
        public DbSet<Parameter> Parameters { set; get; }
        public DbSet<ContinuousInterval> ContinuousIntervals { set; get; }

        public struct QueryParam
        {
            public string Name { set; get; }
            public object Value { set; get; }
        }

        public delegate T MapperDelegate<T>(IDataReader record);

        static Store()
        {
            Database.SetInitializer<Store>(null);
        }
        
        public Store(): base("AmtEF")
        {
            var cs = ConfigurationManager.ConnectionStrings["Amt"];
            var factory = DbProviderFactories.GetFactory(cs.ProviderName);
            var con = factory.CreateConnection();
            con.ConnectionString = cs.ConnectionString;
            con.Open();

            Connection = con;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new UnitMapping());
            modelBuilder.Configurations.Add(new WellboreMapping());
            modelBuilder.Configurations.Add(new WellboreStateMapping());
            modelBuilder.Configurations.Add(new StateMapping());
            modelBuilder.Configurations.Add(new ParameterMapping());
            modelBuilder.Configurations.Add(new ContinuousIntervalMapping());
        }


        protected IEnumerable<T> Map<T>(IDataReader query, MapperDelegate<T> recordMapper)
        {
            while (query.Read())
            {
                yield return recordMapper(query);
            }
        }

        protected IDbCommand CreateCommand(string query, params QueryParam[] queryParams)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Transaction = transaction;

            foreach (var param in queryParams)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = param.Name;
                p.Value = param.Value ?? DBNull.Value;

                cmd.Parameters.Add(p);
            }

            return cmd;
        }

        protected QueryParam[] CreateParams(object queryParameters)
        {
            if (queryParameters == null)
                return new QueryParam[0];

            var props = queryParameters.GetType().GetProperties();

            QueryParam[] pars = new QueryParam[props.Length];

            for (int i = 0; i < props.Length; ++i)
            {
                pars[i] = new QueryParam { Name = ":" + props[i].Name, Value = props[i].GetValue(queryParameters, null) };
            }
            return pars;
        }


        protected IDataReader Query(string query, params QueryParam[] queryParams)
        {
            using (var cmd = CreateCommand(query, queryParams))
            {
                return cmd.ExecuteReader();
            }
        }

        protected IDataReader Query(string query, object queryParameters)
        {
            return Query(query, CreateParams(queryParameters));
        }

        public IEnumerable<T> QueryAndMap<T>(string query, object queryParameters, MapperDelegate<T> recordMapper)
        {
            return Map(Query(query, queryParameters), recordMapper);
        }

        public T ConvertNullable<T>(object value)
        {
            if (value == DBNull.Value)
                return default(T);

            return (T)System.Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
        }

        public void FillParameter(Parameter p)
        {
            p = Parameters.Where(x => x.Id == p.Id).SingleOrDefault();
        }

        public List<decimal> GetLimitPoints(long wellboreId, DateTime time, List<Parameter> parameters, StringBuilder queryString)
        {          
            string resultStringArray = QueryAndMap(ActualDataFunctionQuery, new { p_wellbore_id = wellboreId, p_time = time, p_parameter_list = queryString.Remove(0, 1).ToString(), p_limit = time.AddMinutes(-10) },
                x => ConvertNullable<string>(x[0])).SingleOrDefault();
            
            var parsedList = new List<decimal>();
            foreach (var parameter in parameters)
            {
                if (resultStringArray.Contains(parameter.Code))
                {
                    int symbolPos = resultStringArray.IndexOf(parameter.Code);
                    parsedList.Add(decimal.Parse(resultStringArray.Substring(symbolPos + parameter.Code.Length + 2, resultStringArray.IndexOf('\"', symbolPos) - symbolPos - parameter.Code.Length)));
                }
                else
                    parsedList.Add(NoValue);
            }
            return parsedList;
        }

        public List<decimal[]> GetData(List<Parameter> parameters, Wellbore wellbore, DateTime start, DateTime end)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (wellbore == null)
                throw new ArgumentNullException("wellbore");

            parameters.ForEach(x => FillParameter(x));

            StringBuilder queryBuilder = new StringBuilder();
            parameters.ForEach(x => queryBuilder.Append(", ").Append(x.Code));
            var limitCorrector = GetLimitPoints(wellbore.Id, start, parameters, queryBuilder);

            queryBuilder.Insert(0, "select time, ");
            queryBuilder.Append(" from temporal_measuring where wellbore_id = :wellboreId and time between :startTime and :endTime");

            var oldValues = new decimal[parameters.Count];
            for (var i = 0; i < parameters.Count; ++i)
                oldValues[i] = limitCorrector[i];

            MapperDelegate<decimal[]> mapper = x =>
            {
                var r = new decimal[parameters.Count + 1];

                for (var i = 0; i < parameters.Count; ++i)
                {
                    var val = x[i + 1];
                    r[i] = val == DBNull.Value ? oldValues[i] : Convert.ToDecimal(val) / parameters[i].Multiplier;
                }
                r[parameters.Count] = Convert.ToDateTime(x[0]).Ticks;

                oldValues = r;
                return r;
            };

            return new List<decimal[]>(QueryAndMap(queryBuilder.ToString(), new { wellboreId = wellbore.Id, startTime = start, endTime = end }, mapper));
        }
    }
}
