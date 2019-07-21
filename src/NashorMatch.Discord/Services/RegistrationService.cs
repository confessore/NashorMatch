using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NashorMatch.Discord.Services
{
    public class RegistrationService
    {
        readonly IServiceProvider services;
        readonly CommandService commands;

        public RegistrationService(
            IServiceProvider services,
            CommandService commands)
        {
            this.services = services;
            this.commands = commands;
        }

        public async Task IntializeRegistrationsAsync()
        {
            await RegisterEvents();
            await RegisterModulesAsync();
            Console.WriteLine("registrations initialized!");
        }

        async Task RegisterModulesAsync()
        {
            Console.WriteLine("registering modules...");
            await commands.AddModulesAsync(
                Assembly.GetEntryAssembly(),
                services);
        }

        Task RegisterEvents()
        {
            Console.WriteLine("registering events...");
            services.GetRequiredService<EventService>();
            return Task.CompletedTask;
        }
    }
}
