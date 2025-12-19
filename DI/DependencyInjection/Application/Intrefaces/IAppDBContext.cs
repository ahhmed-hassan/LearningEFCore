


using Microsoft.EntityFrameworkCore;

namespace DependencyInjection.Application.Intrefaces;

internal class IAppDBContext : DbContext
{
    public DbSet<Domain.User> Users { get; set; }


}
