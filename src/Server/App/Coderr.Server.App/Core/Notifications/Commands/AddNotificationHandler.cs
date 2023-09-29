﻿using System.Threading.Tasks;
using Coderr.Server.Api.Core.Notifications;
using DotNetCqs;


namespace Coderr.Server.App.Core.Notifications.Commands
{
    /// <summary>
    /// Handler for <see cref="AddNotification"/>.
    /// </summary>
    public class AddNotificationHandler : IMessageHandler<AddNotification>
    {
        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="command">cmd</param>
        /// <returns>task</returns>
        public Task HandleAsync(IMessageContext context, AddNotification command)
        {
            //TODO: Implement
            return Task.FromResult<object>(null);
        }
    }
}
