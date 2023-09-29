﻿using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Coderr.Server.Abstractions;
using Coderr.Server.Abstractions.Boot;
using Coderr.Server.Abstractions.Config;
using Coderr.Server.Abstractions.Reports;
using Griffin.ApplicationServices;
using Griffin.Data;
using Griffin.Data.Mapper;
using log4net;

namespace Coderr.Server.App.Core.Incidents.Jobs
{
    /// <summary>
    ///     Delete incidents where all reports have been deleted (due to retention days).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         There are other jobs where old reports are removed. This job is to make sure that old incidents are being
    ///         deleted
    ///         when there are no reports for them. Do note that ignored incidents will not be deleted.
    ///     </para>
    /// </remarks>
    [ContainerService(RegisterAsSelf = true)]
    internal class DeleteOldIncidents : IBackgroundJobAsync
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(DeleteOldIncidents));
        private readonly IDbConnection _connection;
        private readonly IConfiguration<ReportConfig> _reportConfiguration;

        /// <summary>
        ///     Creates a new instance of <see cref="DeleteOldIncidents" />.
        /// </summary>
        /// <param name="connection">Used for SQL queries</param>
        public DeleteOldIncidents(IDbConnection connection, IConfiguration<ReportConfig> reportConfiguration)
        {
            _connection = connection;
            _reportConfiguration = reportConfiguration;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync()
        {
            if (HostConfig.Instance.IsDemo)
                return;

            using (var cmd = _connection.CreateDbCommand())
            {
                cmd.CommandText =
                    $@"CREATE TABLE #ItemsToDelete 
                            ( 
                                Id int NOT NULL PRIMARY KEY
                            )

                            INSERT #ItemsToDelete (Id)
                            SELECT TOP(500) Id
                            FROM Incidents WITH (ReadPast)
                            WHERE CreatedAtUtc < @retentionDays AND Incidents.State = 0
                            declare @counter int = 0;

                            IF @@ROWCOUNT <> 0 
                            BEGIN 
                                DECLARE ItemsToDeleteCursor CURSOR LOCAL FORWARD_ONLY READ_ONLY
                                FOR SELECT Id FROM #ItemsToDelete
                                set @counter = 1

                                DECLARE @IdToDelete int
                                OPEN ItemsToDeleteCursor
                                FETCH NEXT FROM ItemsToDeleteCursor INTO @IdToDelete

                                WHILE @@FETCH_STATUS = 0 
                                BEGIN
                                    set @counter = @counter + 1
                                    DELETE FROM Incidents WHERE Id = @IdToDelete
                                    FETCH NEXT FROM ItemsToDeleteCursor INTO @IdToDelete
                                END

                                CLOSE ItemsToDeleteCursor
                                DEALLOCATE ItemsToDeleteCursor
                            END 
                            DROP TABLE #ItemsToDelete
                            select @counter;";

                // OLD:
                // Wait until no reports have been received for the specified report save time
                // and then make sure during another period that no new reports have been received.
                //
                // NEW:
                // We'll delete incidents when they was created long time ago and no one have started to work on them.
                // In that way, those that still are happening will be brought to top, and all aggregated data will be cleaned.
                var incidentRetention = _reportConfiguration.Value.RetentionDaysIncidents * 2;

                cmd.AddParameter("retentionDays", DateTime.Today.AddDays(-incidentRetention));
                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows > 0)
                {
                    _logger.Debug("Deleted " + rows + " empty incidents.");
                }
            }
        }
    }
}