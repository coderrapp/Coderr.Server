﻿using System;
using System.Threading.Tasks;
using Coderr.Server.Api.Core.Applications.Commands;
using Coderr.Server.Api.Core.Applications.Events;
using Coderr.Server.Domain.Core.Applications;
using Coderr.Server.Domain.Core.User;
using DotNetCqs;


namespace Coderr.Server.App.Core.Applications.CommandHandlers
{
    internal class CreateApplicationHandler : IMessageHandler<CreateApplication>
    {
        private readonly IApplicationRepository _repository;
        private readonly IUserRepository _userRepository;

        public CreateApplicationHandler(IApplicationRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task HandleAsync(IMessageContext context, CreateApplication command)
        {
            var app = new Application(command.UserId, command.Name)
            {
                ApplicationType =
                    (TypeOfApplication)Enum.Parse(typeof(TypeOfApplication), command.TypeOfApplication.ToString())
            };
            if (command.ApplicationKey != null)
                app.AppKey = command.ApplicationKey;


            if (command.NumberOfDevelopers > 0)
            {
                app.AddStatsBase(command.NumberOfDevelopers, command.NumberOfErrors);
            }

            var creator = await _userRepository.GetUserAsync(command.UserId);
            await _repository.CreateAsync(app);
            await _repository.CreateAsync(new ApplicationTeamMember(app.Id, creator.AccountId, creator.UserName)
            {
                UserName = creator.UserName,
                Roles = new[] { ApplicationRole.Admin, ApplicationRole.Member },
            });

            var evt = new ApplicationCreated(app.Id, app.Name, command.UserId, app.AppKey, app.SharedSecret);
            await context.SendAsync(evt);
        }
    }
}