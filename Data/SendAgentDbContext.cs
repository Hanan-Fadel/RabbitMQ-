using AirlineSendAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineSendAgent.Data
{
    public class SendAgentDbContext : DbContext
    {
      public SendAgentDbContext(DbContextOptions<SendAgentDbContext> options) : base(options)
      {
        
      }
      public DbSet<WebhookSubscription> webhookSubscriptions { get; set; }
    }
}