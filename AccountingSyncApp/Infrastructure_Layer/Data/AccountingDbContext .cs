﻿using Domain_Layer.Models;
using Domain_Layer.Models; // Your entities like Customer, XeroToken, etc.
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Infrastructure_Layer.Data
{
    public class AccountingDbContext : DbContext
    {
        public AccountingDbContext(DbContextOptions<AccountingDbContext> options)
            : base(options) { }
        //parameterless constructor for design-time tools
        public AccountingDbContext()
        {
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<XeroTokenResponse> XeroTokenResponse { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer PK
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.Id);

            // XeroToken PK
            modelBuilder.Entity<XeroTokenResponse>()
                .HasKey(t => t.Id);

            // Invoice PK
            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.Id);

            // Quote PK
            modelBuilder.Entity<Quote>()
                .HasKey(q => q.Id);

            modelBuilder.Entity<Invoice>()
            .HasOne<Customer>()       // Invoice belongs to Customer
            .WithMany()               // Customer can have many invoices
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quote>()
            .HasOne<Customer>()       // Quote belongs to Customer
            .WithMany()               // Customer can have many quotes
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
