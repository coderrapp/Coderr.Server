﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Coderr.Server.Domain.Core.Incidents.Events;
using Coderr.Server.Domain.Modules.Tags;
using DotNetCqs;

namespace Coderr.Server.ReportAnalyzer.Tagging.Handlers
{
    /// <summary>
    ///     Adds a "incident-reopened" tag
    /// </summary>
    public class IncidentReopenedHandler : IMessageHandler<IncidentReOpened>
    {
        private readonly ITagsRepository _repository;

        /// <summary>
        ///     Creates a new instance of <see cref="IncidentReopenedHandler" />.
        /// </summary>
        /// <param name="repository">repos</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public IncidentReopenedHandler(ITagsRepository repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task HandleAsync(IMessageContext context, IncidentReOpened e)
        {
            var tags = await _repository.GetIncidentTagsAsync(e.IncidentId);
            if (tags.Any(x => x.Name == "incident-reopened"))
                return;

            await _repository.AddAsync(e.IncidentId, new[] {new Tag("incident-reopened", 1)});
        }
    }
}