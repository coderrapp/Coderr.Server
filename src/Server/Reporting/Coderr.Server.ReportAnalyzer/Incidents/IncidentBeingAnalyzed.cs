﻿using System;
using Coderr.Server.Domain.Core.ErrorReports;
using Coderr.Server.Infrastructure;
using Coderr.Server.ReportAnalyzer.ErrorReports;

namespace Coderr.Server.ReportAnalyzer.Incidents
{
    /// <summary>
    ///     Keeps track of all report occurrences for a single incident (i.e. error reports which generates the same hash code)
    /// </summary>
    public class IncidentBeingAnalyzed
    {
        private string _description;

        /// <summary>
        ///     Creates a new instance of <see cref="IncidentBeingAnalyzed" />.
        /// </summary>
        protected IncidentBeingAnalyzed()
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="IncidentBeingAnalyzed" />.
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="ArgumentNullException">entity</exception>
        /// <exception cref="ArgumentException">entity have no hashcode</exception>
        public IncidentBeingAnalyzed(ErrorReportEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (string.IsNullOrEmpty(entity.ReportHashCode))
                throw new ArgumentException("ReportHashCode must be specified to be able to identify duplicates.");
            Description = entity.Exception.Message;
            FullName = entity.Exception.FullName;
            StackTrace = entity.Exception.StackTrace;

            AddReport(entity);
            ReportHashCode = entity.ReportHashCode;
            HashCodeIdentifier = entity.GenerateHashCodeIdentifier();
            ApplicationId = entity.ApplicationId;
            UpdatedAtUtc = entity.CreatedAtUtc;
            CreatedAtUtc = entity.CreatedAtUtc;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="IncidentBeingAnalyzed" />.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="exception">exception to analyze</param>
        /// <exception cref="ArgumentNullException">entity; exception</exception>
        /// <exception cref="ArgumentException">entity.hashcode is null</exception>
        public IncidentBeingAnalyzed(ErrorReportEntity entity, ErrorReportException exception)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (exception == null) throw new ArgumentNullException("exception");
            if (string.IsNullOrEmpty(entity.ReportHashCode))
                throw new ArgumentException("ReportHashCode must be specified to be able to identify duplicates.");

            Description = exception.Message;
            FullName = exception.FullName;
            StackTrace = HashCodeGenerator.CleanStackTrace(exception.StackTrace);

            AddReport(entity);
            ReportHashCode = entity.ReportHashCode;
            HashCodeIdentifier = entity.GenerateHashCodeIdentifier();
            ApplicationId = entity.ApplicationId;
            UpdatedAtUtc = entity.CreatedAtUtc;
            CreatedAtUtc = entity.CreatedAtUtc;
        }

        /// <summary>
        ///     Application that the report belongs in
        /// </summary>
        public int ApplicationId { get; private set; }

        /// <summary>
        ///     When report was created
        /// </summary>
        public DateTime CreatedAtUtc { get; private set; }

        /// <summary>
        ///     Incident description
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_description))
                    return "Ooops Error!";

                return _description;
            }
            set => _description = value;
        }

        /// <summary>
        ///     List of all environment names that the developer specified when reporting the errors.
        /// </summary>
        public string[] EnvironmentNames { get; set; }

        /// <summary>
        ///     Full name of the exception message.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        ///     Used to identify this incident when the hash code is the same as for other incidents.
        /// </summary>
        /// <returns></returns>
        public string HashCodeIdentifier { get; private set; }

        /// <summary>
        ///     primary key
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        ///     Version that this incident should not be ignored in
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Both used in close incident and ignore incident
        ///     </para>
        /// </remarks>
        public string IgnoredUntilVersion { get; set; }

        /// <summary>
        ///     Incident have been solved (bug as been identified and corrected)
        /// </summary>
        public bool IsClosed => State == AnalyzedIncidentState.Closed;

        /// <summary>
        ///     Incident is ignored, i.e. do not track any more reports or send any notifications.
        /// </summary>
        public bool IsIgnored => State == AnalyzedIncidentState.Ignored;

        /// <summary>
        ///     Incident is opened again after being closed.
        /// </summary>
        public bool IsReOpened { get; set; }

        /// <summary>
        ///     When we received a report (just to keep track of how fresh this incident is).
        /// </summary>
        public DateTime LastReportAtUtc { get; set; }

        /// <summary>
        ///     When we received a report that we actual stored.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This field is used to determine if we should store another report today, while <see cref="LastReportAtUtc" />
        ///         is just used to keep track of when we received the most recent report.
        ///     </para>
        /// </remarks>
        public DateTime LastStoredReportUtc { get; set; }

        /// <summary>
        ///     Set if incident was closed and a solution was written
        /// </summary>
        public DateTime PreviousSolutionAtUtc { get; set; }

        /// <summary>
        ///     When the report was opened again
        /// </summary>
        /// <seealso cref="IsReOpened" />
        /// -
        public DateTime ReOpenedAtUtc { get; set; }

        /// <summary>
        ///     Total number of reports for this incident (including those who was ignored)
        /// </summary>
        public int ReportCount { get; set; }

        /// <summary>
        ///     Hashcode identifying this incident
        /// </summary>
        public string ReportHashCode { get; private set; }

        /// <summary>
        ///     When the solution was written
        /// </summary>
        public DateTime SolvedAtUtc { get; set; }

        /// <summary>
        ///     Stack trace from the exception
        /// </summary>
        public string StackTrace { get; set; }

        public AnalyzedIncidentState State { get; private set; }


        /// <summary>
        ///     Incident has been updated (received a new report or by an action by a user)
        /// </summary>
        public DateTime UpdatedAtUtc { get; private set; }

        /// <summary>
        ///     Add another report.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <exception cref="ArgumentNullException">entity</exception>
        public void AddReport(ErrorReportEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            if (string.IsNullOrWhiteSpace(StackTrace) && entity.Exception != null)
            {
                Description = entity.Exception.Message;
                FullName = entity.Exception.FullName;
                StackTrace = entity.Exception.StackTrace;
            }

            LastReportAtUtc = entity.CreatedAtUtc;
            ReportCount++;
        }

        /// <summary>
        ///     Check if this incident is ignored in the version that we received in the newest error report.
        /// </summary>
        /// <param name="reportedVersion">Version in the report that we just received.</param>
        /// <returns></returns>
        public bool IsReportIgnored(string reportedVersion)
        {
            if (reportedVersion == null) throw new ArgumentNullException(nameof(reportedVersion));
            var comparer = new ApplicationVersionComparer();
            return comparer.Compare(reportedVersion, IgnoredUntilVersion) < 0;
        }

        /// <summary>
        ///     Open a closed incident.
        /// </summary>
        /// <seealso cref="IsReOpened" />
        public void ReOpen()
        {
            PreviousSolutionAtUtc = SolvedAtUtc;
            State = AnalyzedIncidentState.New;
            ReOpenedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
            IsReOpened = true;
        }

        /// <summary>
        ///     Just ignored a new report
        /// </summary>
        public void WasJustIgnored()
        {
            LastReportAtUtc = DateTime.UtcNow;
            ReportCount++;
        }
    }
}