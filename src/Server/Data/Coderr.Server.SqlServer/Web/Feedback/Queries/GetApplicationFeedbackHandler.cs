using System.Linq;
using System.Threading.Tasks;
using Coderr.Server.Api.Web.Feedback.Queries;
using DotNetCqs;
using Coderr.Server.ReportAnalyzer.Abstractions;
using Griffin.Data;
using Griffin.Data.Mapper;

namespace Coderr.Server.SqlServer.Web.Feedback.Queries
{
    public class GetApplicationFeedbackHandler :
        IQueryHandler<GetFeedbackForApplicationPage, GetFeedbackForApplicationPageResult>
    {
        private readonly IAdoNetUnitOfWork _unitOfWork;

        public GetApplicationFeedbackHandler(IAdoNetUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetFeedbackForApplicationPageResult> HandleAsync(IMessageContext context, GetFeedbackForApplicationPage query)
        {
            using (var cmd = _unitOfWork.CreateCommand())
            {
                cmd.CommandText =
                    @"select IncidentFeedback.Description Message, IncidentFeedback.EmailAddress, IncidentFeedback.CreatedAtUtc as WrittenAtUtc, 
       Incidents.Description as IncidentName, IncidentId
from IncidentFeedback
join Incidents on (IncidentId = Incidents.Id)
WHERE IncidentFeedback.ApplicationId = @appId
";
                cmd.AddParameter("appId", query.ApplicationId);
                var items = await cmd.ToListAsync<GetFeedbackForApplicationPageResultItem>();
                return new GetFeedbackForApplicationPageResult
                {
                    Items = items.Where(x => !string.IsNullOrEmpty(x.Message)).ToArray(),
                    Emails =
                        items.Where(x => !string.IsNullOrEmpty(x.EmailAddress)).Select(x => x.EmailAddress).ToList<string>()
                };
            }
        }
    }
}