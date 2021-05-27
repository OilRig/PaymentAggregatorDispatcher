using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaymentDispatcher.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PaymentDispatcher.Database.Helpers.ConfigurationHelper;

namespace PaymentDispatcher.Database.Context
{
    public class DatabaseContext : DbContext
    {
        private IConfiguration _configuration { get; }
        public DbSet<DispatcherDBRequest> DispatcherPaymentRequests { get; set; }
        public DbSet<AggregatorTokenMap> AggregatorTokenMaps { get; set; }

        public DatabaseContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureDB();
        }
    }
}
