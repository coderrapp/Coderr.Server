﻿using System.Threading.Tasks;
using Coderr.Server.Abstractions.WorkItems;
using Coderr.Server.Api.Core.Incidents.Events;
using Coderr.Server.Domain;
using DotNetCqs;
using log4net;

namespace Coderr.Server.App.WorkItems.Events
{
    internal class AssignWorkItemWhenIncidentAssigned : IMessageHandler<IncidentAssigned>
    {
        private readonly IWorkItemServiceProvider _itemServiceProvider;
        private readonly IWorkItemRepository _workItemRepository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(AssignWorkItemWhenIncidentAssigned));
        public AssignWorkItemWhenIncidentAssigned(IWorkItemServiceProvider itemServiceProvider,
            IWorkItemRepository workItemRepository)
        {
            _itemServiceProvider = itemServiceProvider;
            _workItemRepository = workItemRepository;
        }


        public async Task HandleAsync(IMessageContext context, IncidentAssigned message)
        {
            var workItem = await _workItemRepository.Find(message.IncidentId);
            if (workItem == null)
                return;

            var service = await _itemServiceProvider.FindService(workItem.ApplicationId);
            try
            {
                await service.Assign(workItem.ApplicationId, workItem.WorkItemId, message.AssignedToId);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.Error($"Failed to find {workItem.WorkItemId} in azure, invalid mapping for incident {message.IncidentId}.", ex);

                // Been deleted in Azure DevOps
                await _workItemRepository.Delete(workItem);
            }
        }
    }
}