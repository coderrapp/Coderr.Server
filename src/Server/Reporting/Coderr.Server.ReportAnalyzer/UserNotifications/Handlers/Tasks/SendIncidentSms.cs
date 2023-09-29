using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Coderr.Server.Domain.Core.User;
using Coderr.Server.Infrastructure.Configuration;
using Coderr.Server.ReportAnalyzer.Abstractions.ErrorReports;
using Coderr.Server.ReportAnalyzer.Abstractions.Incidents;

namespace Coderr.Server.ReportAnalyzer.UserNotifications.Handlers.Tasks
{
    /// <summary>
    ///     Send SMS regarding an incident
    /// </summary>
    public class SendIncidentSms
    {
        private readonly IUserRepository _userRepository;
        private readonly BaseConfiguration _baseConfiguration;

        /// <summary>
        ///     Creates a new instance of <see cref="SendIncidentSms" />.
        /// </summary>
        /// <param name="userRepository">to fetch phone number</param>
        /// <param name="baseConfiguration"></param>
        /// <exception cref="ArgumentNullException">userRepository</exception>
        public SendIncidentSms(IUserRepository userRepository, BaseConfiguration baseConfiguration)
        {
            if (userRepository == null) throw new ArgumentNullException("userRepository");
            _userRepository = userRepository;
            _baseConfiguration = baseConfiguration;
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="accountId">Account to send to</param>
        /// <param name="incident">Incident that the report belongs to</param>
        /// <param name="report">report being processed</param>
        /// <returns>task</returns>
        /// <exception cref="ArgumentNullException">incident;report</exception>
        /// <exception cref="ArgumentOutOfRangeException">accountId</exception>
        public async Task SendAsync(int accountId, IncidentSummaryDTO incident, ReportDTO report)
        {
            if (incident == null) throw new ArgumentNullException("incident");
            if (report == null) throw new ArgumentNullException("report");
            if (accountId <= 0) throw new ArgumentOutOfRangeException("accountId");

            var settings = await _userRepository.GetUserAsync(accountId);
            if (string.IsNullOrEmpty(settings.MobileNumber))
                return; //TODO: LOG

            var url = _baseConfiguration.BaseUrl;
            var shortName = incident.Name.Length > 20
                ? incident.Name.Substring(0, 20) + "..."
                : incident.Name;

            var exMsg = report.Exception.Message.Length > 100
                ? report.Exception.Message.Substring(0, 100)
                : report.Exception.Message;


            var baseUrl = _baseConfiguration.BaseUrl.ToString().TrimEnd('/');
            var incidentUrl =
                $"{baseUrl}/discover/incidents/{report.ApplicationId}/incident/{report.IncidentId}/";

            string msg;
            if (incident.IsReOpened)
            {
                msg = $@"ReOpened: {shortName}
{incidentUrl}

{exMsg}";
            }
            else if (incident.ReportCount == 1)
            {
                msg = $@"New: {shortName}
{incidentUrl}

{exMsg}";
            }
            else
            {
                msg = $@"Updated: {shortName}
ReportCount: {incident.ReportCount}
{incidentUrl}

{exMsg}";
            }

            var iso = Encoding.GetEncoding("ISO-8859-1");
            var utfBytes = Encoding.UTF8.GetBytes(msg);
            var isoBytes = Encoding.Convert(Encoding.UTF8, iso, utfBytes);
            msg = iso.GetString(isoBytes);

            var request =
                WebRequest.CreateHttp("https://web.smscom.se/sendsms.aspx?acc=ip1-755&pass=z35llww4&msg=" +
                                      Uri.EscapeDataString(msg) + "&to=" + settings.MobileNumber +
                                      "&from=Coderr&prio=2");
            request.ContentType = "application/json";
            request.Method = "GET";
            await request.GetResponseAsync();
        }
    }
}