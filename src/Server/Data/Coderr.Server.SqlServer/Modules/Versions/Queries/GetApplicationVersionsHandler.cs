﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coderr.Server.Api.Modules.Versions.Queries;
using Coderr.Server.Infrastructure;
using DotNetCqs;
using Griffin.Data;

namespace Coderr.Server.SqlServer.Modules.Versions.Queries
{
    internal class GetApplicationVersionsHandler : IQueryHandler<GetApplicationVersions, GetApplicationVersionsResult>
    {
        private readonly IAdoNetUnitOfWork _uow;

        public GetApplicationVersionsHandler(IAdoNetUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<GetApplicationVersionsResult> HandleAsync(IMessageContext context,
            GetApplicationVersions query)
        {
            var sql =
                @"SELECT version, sum(incidentcount) incidentcount, sum(reportcount) reportcount, min(FirstReportDate) as FirstReportDate, max(LastReportDate)as LastReportDate
  FROM ApplicationVersions WITH (NoLock)
  join ApplicationVersionMonths WITH (NoLock) on (versionid=applicationversions.id)
  where applicationid=@appId
  group by version
  order by version
";
            using (var cmd = _uow.CreateDbCommand())
            {
                cmd.CommandText = sql;
                cmd.AddParameter("appId", query.ApplicationId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var items = new List<GetApplicationVersionsResultItem>();
                    while (await reader.ReadAsync())
                    {
                        var item = new GetApplicationVersionsResultItem
                        {
                            Version = reader[0].ToString(),
                            FirstReportReceivedAtUtc = (DateTime) reader[3],
                            LastReportReceivedAtUtc = (DateTime) reader[4],
                            IncidentCount = (int) reader[1],
                            ReportCount = (int) reader[2]
                        };
                        items.Add(item);
                    }

                    var comparer = new ApplicationVersionComparer();
                    var sortedItems = items.OrderByDescending(x => x.Version, comparer).ToArray();
                    return new GetApplicationVersionsResult {Items = sortedItems};
                }
            }
        }
    }
}