﻿using System;
using System.Threading.Tasks;
using Coderr.Server.Api.Core.Accounts.Queries;
using Coderr.Server.Domain.Core.Account;
using DotNetCqs;


namespace Coderr.Server.App.Core.Accounts.Queries
{
    /// <summary>
    ///     Handler of <see cref="FindAccountByUserName" />.
    /// </summary>
    public class FindAccountByUserNameHandler : IQueryHandler<FindAccountByUserName, FindAccountByUserNameResult>
    {
        private readonly IAccountRepository _repository;

        /// <summary>
        ///     Creates a new instance of <see cref="FindAccountByUserName" />.
        /// </summary>
        /// <param name="repository"></param>
        public FindAccountByUserNameHandler(IAccountRepository repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            _repository = repository;
        }

        /// <summary>
        ///     Method used to execute the query
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <returns>
        ///     Task which will contain the result once completed.
        /// </returns>
        public async Task<FindAccountByUserNameResult> HandleAsync(IMessageContext context, FindAccountByUserName query)
        {
            if (query == null) throw new ArgumentNullException("query");

            var user = await _repository.FindByUserNameAsync(query.UserName);
            return user == null ? null : new FindAccountByUserNameResult(user.Id, user.UserName);
        }
    }
}