using Microsoft.EntityFrameworkCore;
using PaymentDispatcher.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentDispatcher.Database.Helpers
{
    public static class ConfigurationHelper
    {
        private static ModelBuilder ConfigureRequests(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DispatcherDBRequest>(ent => ent.ToTable("DispatcherDBRequests"));

            modelBuilder.Entity<DispatcherDBRequest>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<DispatcherDBRequest>()
                .Property(x => x.UniqueRequestToken)
                .IsRequired();

            modelBuilder.Entity<DispatcherDBRequest>()
                .Property(x => x.UniqueAggregatorToken)
                .HasMaxLength(512)
                .IsRequired();

            return modelBuilder;
        }
        private static ModelBuilder ConfigureTokenMaps(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AggregatorTokenMap>(ent => ent.ToTable("AggregatorTokenMaps"));

            modelBuilder.Entity<AggregatorTokenMap>()
               .HasKey(x => x.Id);

            modelBuilder.Entity<AggregatorTokenMap>()
                .Property(x => x.AggregatorAddress)
                .HasMaxLength(256);

            modelBuilder.Entity<AggregatorTokenMap>()
                .Property(x => x.AggregatorToken)
                .HasMaxLength(256);

            modelBuilder.Entity<AggregatorTokenMap>()
                .HasIndex(x => new { x.AggregatorAddress, x.AggregatorToken })
                .IsUnique();

            return modelBuilder;
        }

        public static void ConfigureDB(this ModelBuilder modelBuilder)
        {
            modelBuilder
                .ConfigureRequests()
                .ConfigureTokenMaps();
        }
    }
}
