﻿using System.Threading.Tasks;
using Coderr.Server.Api.Core.Environments.Commands;
using DotNetCqs;
using Griffin.Data;
using log4net;

namespace Coderr.Server.SqlServer.Core.Environments
{
    internal class ResetEnvironmentHandler : IMessageHandler<ResetEnvironment>
    {
        private readonly IAdoNetUnitOfWork _unitOfWork;
        private ILog _loggr = LogManager.GetLogger(typeof(ResetEnvironmentHandler));

        public ResetEnvironmentHandler(IAdoNetUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task HandleAsync(IMessageContext context, ResetEnvironment message)
        {

            var sql = @"WITH JustOurIncidents (IncidentId) AS
                            (
	                            select ie.IncidentId
	                            from IncidentEnvironments ie 
	                            join Incidents i ON (i.Id = ie.IncidentId)
	                            join Environments e ON (ie.EnvironmentId = e.Id)
	                            where i.ApplicationId = @applicationId
	                            group by ie.IncidentId
	                            having count(e.Id) = 1
                            )
                            DELETE IncidentEnvironments
                            FROM IncidentEnvironments
                            JOIN JustOurIncidents ON (JustOurIncidents.IncidentId = IncidentEnvironments.IncidentId)
                            WHERE IncidentEnvironments.EnvironmentId = @environmentId";

            _unitOfWork.ExecuteNonQuery(sql, new {message.ApplicationId, message.EnvironmentId});
            _loggr.Info("Resetting environmentId " + message.EnvironmentId + " for app " + message.ApplicationId);
            return Task.CompletedTask;
        }
    }
}