﻿using System;
using System.Threading.Tasks;
using Coderr.Server.Abstractions.Security;
using Coderr.Server.Domain.Core.Incidents;
using Coderr.Server.Domain.Core.Incidents.Events;
using Coderr.Server.Domain.Modules.History;
using DotNetCqs;

namespace Coderr.Server.App.Modules.History.Events
{
    public class IncidentReOpenedHandler : IMessageHandler<IncidentReOpened>
    {
        private readonly IHistoryRepository _repository;

        public IncidentReOpenedHandler(IHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(IMessageContext context, IncidentReOpened message)
        {
            int? accountId;

            try
            {
                if (context.Principal.IsInRole(CoderrRoles.System))
                    accountId = null;
                else
                    accountId = context.Principal.GetAccountId();
            }
            catch (Exception ex)
            {
                ex.Data["Principal"] = context.Principal.ToFriendlyString();
                throw;
            }

            var e = new HistoryEntry(message.IncidentId, accountId, IncidentState.ReOpened)
            {
                ApplicationVersion = message.ApplicationVersion
            };
            await _repository.CreateAsync(e);
        }
    }
}