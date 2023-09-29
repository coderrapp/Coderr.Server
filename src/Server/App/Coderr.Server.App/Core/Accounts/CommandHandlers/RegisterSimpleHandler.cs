﻿using System;
using System.Threading.Tasks;
using Coderr.Server.Abstractions.Config;
using Coderr.Server.Api.Core.Accounts;
using Coderr.Server.Api.Core.Accounts.Commands;
using Coderr.Server.Api.Core.Accounts.Events;
using Coderr.Server.Api.Core.Messaging;
using Coderr.Server.Api.Core.Messaging.Commands;
using Coderr.Server.Domain.Core.Account;
using Coderr.Server.Infrastructure.Configuration;
using DotNetCqs;

using log4net;

namespace Coderr.Server.App.Core.Accounts.CommandHandlers
{
    /// <summary>
    ///     Handler for <see cref="RegisterSimple" />.
    /// </summary>
    internal class RegisterSimpleHandler : IMessageHandler<RegisterSimple>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(RegisterSimpleHandler));
        private readonly IAccountRepository _repository;
        private readonly ConfigurationStore _configStore;

        public RegisterSimpleHandler(IAccountRepository repository, ConfigurationStore configStore)
        {
            _repository = repository;
            _configStore = configStore;
        }

        public async Task HandleAsync(IMessageContext context, RegisterSimple command)
        {
            var pos = command.EmailAddress.IndexOf('@');
            if (pos == -1)
            {
                _logger.Warn("Invalid email address: " + command.EmailAddress);
                throw new InvalidOperationException("Invalid email address");
            }

            var user = _repository.FindByEmailAsync(command.EmailAddress);
            if (user != null)
            {
                _logger.Warn("Email already taken, sending reset password: " + command.EmailAddress);
                await context.SendAsync(new RequestPasswordReset(command.EmailAddress));
            }

            var userName = await TryCreateUsernameAsync(command, pos);
            if (userName == null)
            {
                _logger.Error("Failed to generate user name for " + command.EmailAddress);
                return;
            }


            //var id = _idGeneratorClient.GetNextId(Account.SEQUENCE);
            var password = Guid.NewGuid().ToString("N").Substring(0, 10);
            var account = new Account(userName, password);
            account.SetVerifiedEmail(command.EmailAddress);
            await _repository.CreateAsync(account);

            await SendAccountEmail(context, account, password);

            var evt = new AccountRegistered(account.Id, account.UserName);
            await context.SendAsync(evt);
        }

        private Task SendAccountEmail(IMessageContext context, Account account, string password)
        {
            var config = _configStore.Load<BaseConfiguration>();
            //TODO: HTML email
            var msg = new EmailMessage
            {
                TextBody = string.Format(@"Welcome,


We have created your account.

UserName: {1}
Password: {2}

You can login using {0}/account/activate/{3}.

We recommend that you change your password before doing something useful.

Thanks,
  The Coderr Team", config.BaseUrl, account.UserName, password, account.ActivationKey),
                Subject = "Coderr activation"
            };
            msg.Recipients = new[] {new EmailAddress(account.Email)};

            return context.SendAsync(new SendEmail(msg));
        }

        private async Task<string> TryCreateUsernameAsync(RegisterSimple command, int pos)
        {
            var suggestedUserName = command.EmailAddress.Substring(0, pos);
            if (!await _repository.IsUserNameTakenAsync(suggestedUserName))
                return suggestedUserName;

            var counter = 100;
            var newUserName = suggestedUserName + counter;
            while (counter < 110)
            {
                if (!await _repository.IsUserNameTakenAsync(newUserName))
                {
                    suggestedUserName = newUserName;
                    return suggestedUserName;
                }

                counter++;
                newUserName = suggestedUserName + counter;
            }

            _logger.Error("Failed to generate userName: " + suggestedUserName);
            return null;
        }
    }
}