using Microsoft.EntityFrameworkCore;

namespace DatabaseConsumer.Services.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    
}