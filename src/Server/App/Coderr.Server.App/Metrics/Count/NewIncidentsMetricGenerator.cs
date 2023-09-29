﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coderr.Server.Api.Insights.Queries;
using Coderr.Server.App.Metrics.DbEntities;
using Griffin.Data;
using Griffin.Data.Mapper;

namespace Coderr.Server.App.Metrics.Count
{
    [Metric(Name)]
    public class NewIncidentsMetricGenerator : IMetricGenerator
    {
        public const string Name = "NewIncidents";

        private readonly IAdoNetUnitOfWork _unitOfWork;

        public NewIncidentsMetricGenerator(IAdoNetUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public MetricDefinition Definition => new MetricDefinition()
        {
            DisplayName = "New errors",
            Description = "Shows the number of new errors"
        };

        public async Task Generate(IMetricGenerationContext context)
        {
            var sqlDateFormat = context.Configuration.Period.TimeType.GetSqlDateFormat();

            var ids = string.Join(",", context.ApplicationIds);
            var sql = $@"select ApplicationId, {sqlDateFormat} AS Date, count(*) Count
                        from CommonIncidentProgressTracking
                        where CreatedAtUtc <= @to
                        AND CreatedAtUtc >= @from
                        AND ApplicationId IN ({ids})
                        group by {sqlDateFormat}, ApplicationId";

            var rows = await _unitOfWork.ToListAsync<CountRow>(sql, new { from = context.From, to = context.To });

            if (context.Configuration.GenerateMetric) GenerateMetric(context, rows);

            if (context.Configuration.GenerateChart) GenerateChart(context, rows);
        }

        // Generate a chart for:
        // 1. Last X days
        // 2. Last X weeks
        // 3. Last X months.
        private void GenerateChart(IMetricGenerationContext context, IReadOnlyCollection<CountRow> rows)
        {
            var dates = context.GenerateDates();


            var trendLines = new Dictionary<int, TrendLine>();

            // Fill each application trend line so dates without values have 0.
            foreach (var applicationId in context.ApplicationIds)
            {
                var line = new TrendLine(applicationId, dates.Length, 0);
                trendLines[applicationId] = line;
            }

            // Add the system trend line (average per application).
            var systemLine = new TrendLine(0, dates.Length, 0) { IsCompanyAverage = true };
            trendLines[0] = systemLine;

            // Now fill all values.
            for (var dateIndex = 0; dateIndex < dates.Length; dateIndex++)
            {
                var appRows = rows
                    .Where(x => x.Date == dates[dateIndex])
                    .GroupBy(x => x.ApplicationId)
                    .ToList();

                if (!appRows.Any()) continue;

                var normalized = appRows.Average(x => x.Sum(y => y.Count) / context.GetNumberOfFtes(x.Key.Value));
                systemLine.Assign(dateIndex,
                    appRows.Average(x => x.Sum(y => y.Count)), normalized);

                foreach (var appRow in appRows)
                {
                    if (!appRow.Any()) continue;

                    trendLines[appRow.Key.Value].Assign(dateIndex,
                        appRow.Sum(x => x.Count),
                        appRow.Sum(x => x.Count / context.GetNumberOfFtes(appRow.Key.Value)));
                }
            }

            foreach (var trendLine in trendLines)
                context.Add(trendLine.Value);
        }


        private static void GenerateMetric(IMetricGenerationContext context, IEnumerable<CountRow> rows)
        {
            // We either have only one application, or want to get one average counter for all our apps.
            var ourMetrics = new List<Metric>();
            var perApp = rows.GroupBy(x => x.ApplicationId);
            foreach (var app in perApp)
            {
                var avg = app.Average(x => x.Count);
                var metric = new Metric(Name)
                {
                    ApplicationId = app.Key.Value,
                    Value = avg,
                    NormalizedValue = avg / context.GetNumberOfFtes(app.Key.Value)
                };
                ourMetrics.Add(metric);

                // We'll only add each one if we said so,
                // otherwise we'll only use them to generate the company average.
                if (!context.Configuration.SummarizeApplications) context.Add(metric);
            }

            var companyMetric = new Metric(Name)
            {
                ApplicationId = 0,
                Value = ourMetrics.Average(x => (decimal)x.NormalizedValue)
            };
            context.Add(companyMetric);
        }
    }
}