﻿using System.Threading.Tasks;
using Coderr.Server.ReportAnalyzer.Abstractions.Inbound.Whitelists.Commands;
using DotNetCqs;
using Griffin.Data;
using Griffin.Data.Mapper;

namespace Coderr.Server.ReportAnalyzer.Inbound.Whitelist
{
    internal class RemoveDomainHandler : IMessageHandler<RemoveEntry>
    {
        private readonly IAdoNetUnitOfWork _uow;

        public RemoveDomainHandler(IAdoNetUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task HandleAsync(IMessageContext context, RemoveEntry message)
        {
            var item = await _uow.FirstAsync<WhitelistedDomain>("Id = @id", new {message.Id});
            await _uow.DeleteAsync(item);
        }
    }
}