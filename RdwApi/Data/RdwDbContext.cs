using Microsoft.EntityFrameworkCore;
using RdwApi.Models;

namespace RdwApi.Data;

public class RdwDbContext(DbContextOptions<RdwDbContext> options) : DbContext(options)
{
    public DbSet<ModelReliability> ModelReliabilities => Set<ModelReliability>();
    public DbSet<AgeReliability> AgeReliabilities => Set<AgeReliability>();
}
