using Microsoft.EntityFrameworkCore;
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
        public DbSet<DispatcherDBRequest> DispatcherPaymentRequests { get; set; }
        public DbSet<AggregatorTokenMap> AggregatorTokenMaps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"server=localhost\IBORZENKO;Integrated Security=SSPI;MultipleActiveResultSets=true;Initial Catalog=PaymentDispatcher");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureDB();
        }
    }
}
