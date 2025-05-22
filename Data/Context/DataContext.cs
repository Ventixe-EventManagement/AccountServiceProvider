using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Data.Entities;

namespace Data.Context;

public class DataContext(DbContextOptions<DataContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
}