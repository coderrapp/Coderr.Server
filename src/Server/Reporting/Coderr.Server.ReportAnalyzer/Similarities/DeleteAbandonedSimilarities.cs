﻿using System;
using System.Threading.Tasks;
using Coderr.Server.Abstractions.Boot;
using Griffin.ApplicationServices;
using Griffin.Data;

namespace Coderr.Server.ReportAnalyzer.Similarities
{
    [ContainerService(RegisterAsSelf = true)]
    internal class DeleteAbandonedSimilarities : IBackgroundJobAsync
    {
        private readonly IAdoNetUnitOfWork _unitOfWork;
        private static DateTime _executionDate = DateTime.Today;

        public DeleteAbandonedSimilarities(IAdoNetUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync()
        {
            if (_executionDate == DateTime.Today)
                return;

            _executionDate = DateTime.Today;


            using (var cmd = _unitOfWork.CreateDbCommand())
            {
                cmd.CommandText = @"DELETE FROM IncidentCorrelations
                                    FROM IncidentCorrelations
                                    LEFT JOIN Incidents ON (Incidents.Id = IncidentId)
                                    WHERE Incidents.Id IS NULL";
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}