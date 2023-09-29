﻿using Coderr.Server.App.Modules.Versions;
using Coderr.Server.Domain.Modules.ApplicationVersions;
using Griffin.Data.Mapper;

namespace Coderr.Server.SqlServer.Modules.Versions
{
    public class ApplicationVersionMapper : CrudEntityMapper<ApplicationVersion>
    {
        public ApplicationVersionMapper() : base("ApplicationVersions")
        {
            Property(x => x.Id)
                .PrimaryKey(true);
            Property(x => x.ReceivedFirstReportAtUtc)
                .ColumnName("FirstReportDate");
            Property(x => x.ReceivedLastReportAtUtc)
                .ColumnName("LastReportDate");
        }
    }
}