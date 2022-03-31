using GROW_CRM.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GROW_CRM.BackgroundTasks
{
    public class DeleteTempMembers : BackgroundService
    {
        private readonly GROWContext _context;
        private readonly ILogger<DeleteTempMembers> _logger;

        public DeleteTempMembers(IServiceProvider services, ILogger<DeleteTempMembers> logger, IServiceScopeFactory factory)
        {
            Services = services;
            _context = factory.CreateScope().ServiceProvider.GetRequiredService<GROWContext>();
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service running.");

            await DeleteTempMembersService(stoppingToken);
        }

        private async Task DeleteTempMembersService(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
            "Consume Scoped Service Hosted Service is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedDeleteEmptyMembers>();

                await scopedProcessingService.DeleteTempMembersService(stoppingToken, _context);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}