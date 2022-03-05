using GROW_CRM.Data;
using GROW_CRM.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GROW_CRM.BackgroundTasks
{
    internal interface IScopedDeleteEmptyMembers
    {
        Task DeleteTempMembersService(CancellationToken stoppingToken, GROWContext _context);
    }

    internal class ScopedDeleteEmptyMembers : IScopedDeleteEmptyMembers
    {        
        private readonly ILogger _logger;

        public ScopedDeleteEmptyMembers(ILogger<ScopedDeleteEmptyMembers> logger)
        {
            _logger = logger;
        }

        public async Task DeleteTempMembersService(CancellationToken stoppingToken, GROWContext _context)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var members = from m in _context.Members
                              where m.FirstName == "" && m.LastName == ""
                              select m;

                foreach(Member m in members)
                {
                    _context.Remove(m);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Scoped Processing Service is working");

                await Task.Delay(86400000, stoppingToken);
            }
        }
    }
}
