﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Coderr.Server.Abstractions.Boot;
using Coderr.Server.Domain.Core.Incidents;
using Coderr.Server.SqlServer.Tools;
using Griffin.Data;
using Griffin.Data.Mapper;

namespace Coderr.Server.SqlServer.Core.Incidents
{
    [ContainerService]
    public class IncidentRepository : IIncidentRepository
    {
        private readonly IAdoNetUnitOfWork _uow;

        public IncidentRepository(IAdoNetUnitOfWork uow)
        {
            if (uow == null) throw new ArgumentNullException("uow");

            _uow = uow;
        }

        public async Task UpdateAsync(Incident incident)
        {
            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText =
                    @"UPDATE Incidents SET 
                        ApplicationId = @ApplicationId,
                        UpdatedAtUtc = @UpdatedAtUtc,
                        Description = @Description,
                        Solution = @Solution,
                        SolvedAtUtc = @solvedAt,
                        IsSolutionShared = @IsSolutionShared,
                        AssignedToId = @AssignedTo,
                        AssignedAtUtc = @AssignedAtUtc,
                        State = @state,
                        IgnoringReportsSinceUtc = @IgnoringReportsSinceUtc,
                        IgnoringRequestedBy = @IgnoringRequestedBy
                        WHERE Id = @id";
                cmd.AddParameter("Id", incident.Id);
                cmd.AddParameter("ApplicationId", incident.ApplicationId);
                cmd.AddParameter("UpdatedAtUtc", incident.UpdatedAtUtc);
                cmd.AddParameter("Description", incident.Description);
                cmd.AddParameter("State", (int)incident.State);
                cmd.AddParameter("AssignedTo", incident.AssignedToId);
                cmd.AddParameter("AssignedAtUtc", (object)incident.AssignedAtUtc ?? DBNull.Value);
                cmd.AddParameter("solvedAt", incident.SolvedAtUtc.ToDbNullable());
                cmd.AddParameter("IgnoringReportsSinceUtc", incident.IgnoringReportsSinceUtc.ToDbNullable());
                cmd.AddParameter("IgnoringRequestedBy", incident.IgnoringRequestedBy);
                cmd.AddParameter("Solution",
                    incident.Solution == null ? null : EntitySerializer.Serialize(incident.Solution));
                cmd.AddParameter("IsSolutionShared", incident.IsSolutionShared);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task MapCorrelationId(int incidentId, string correlationId)
        {
            var sql = @"select @id = Id FROM CorrelationIds WHERE Value = @value;
                        if (@id is NULL)
                        BEGIN
                            INSERT INTO CorrelationIds(Value) VALUES(@value);
                            set @id = scope_identity();
                        END;
                        BEGIN TRY
                          INSERT INTO IncidentCorrelations (CorrelationId, IncidentId) VALUES (@id, @incidentId);  
                        END TRY
                        BEGIN CATCH
                          IF ERROR_NUMBER() NOT IN (2601, 2627) 
                            THROW;
                        END CATCH";
            using (var cmd = _uow.CreateDbCommand())
            {
                cmd.CommandText = sql;
                cmd.AddParameter("value", correlationId);
                cmd.AddParameter("incidentId", incidentId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> GetTotalCountForAppInfoAsync(int applicationId)
        {
            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT CAST(count(*) as int) FROM Incidents WHERE ApplicationId = @ApplicationId";
                cmd.AddParameter("ApplicationId", applicationId);
                var result = (int)await cmd.ExecuteScalarAsync();
                return result;
            }
        }

        public Task<IList<Incident>> GetManyAsync(IEnumerable<int> incidentIds)
        {
            if (incidentIds == null) throw new ArgumentNullException(nameof(incidentIds));
            var ids = string.Join(",", incidentIds);
            if (ids == "")
                throw new ArgumentException("No incident IDs were specified.", nameof(incidentIds));

            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText =
                    $"SELECT * FROM Incidents WHERE Id IN ({ids})";
                return cmd.ToListAsync(new IncidentMapper());
            }
        }

        public async Task Delete(int incidentId)
        {
            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM Incidents WHERE Id = @id";
                cmd.AddParameter("Id", incidentId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public Task<Incident> GetAsync(int id)
        {
            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT TOP 1 * FROM Incidents WHERE Id = @id";

                cmd.AddParameter("id", id);
                return cmd.FirstAsync(new IncidentMapper());
            }
        }

        public Incident Find(int id)
        {
            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT TOP 1 * FROM Incidents WHERE Id = @id";

                cmd.AddParameter("id", id);
                return cmd.FirstOrDefault(new IncidentMapper());
            }
        }

        public Incident Get(int id)
        {
            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT TOP 3 * FROM Incidents WHERE Id = @id";

                cmd.AddParameter("id", id);
                return cmd.First(new IncidentMapper());
            }
        }
    }
}