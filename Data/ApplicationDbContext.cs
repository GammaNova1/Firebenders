using Microsoft.EntityFrameworkCore;
using Firebenders.Models;
using System.Collections.Generic;

namespace Firebenders.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Records> Records { get; set; }
    }
}
