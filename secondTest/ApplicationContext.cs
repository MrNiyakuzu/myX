using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Subscribe> Subscribes { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    //public ApplicationContext()
    //{
    //    Database.EnsureDeleted();   // удаляем бд со старой схемой
    //    Database.EnsureCreated();   // создаем бд с новой схемой
    //}
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        //Database.EnsureDeleted();   // удаляем бд со старой схемой
        Database.EnsureCreated();   // создаем базу данных при первом обращении
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<User>().HasData(
        //        new User { Id = Guid.NewGuid().ToString(), Name = "Tom2", Age = 37 },
        //        new User { Id = Guid.NewGuid().ToString(), Name = "Bob2", Age = 41 },
        //        new User { Id = Guid.NewGuid().ToString(), Name = "Sam2", Age = 24 }
        //);
        //modelBuilder.Entity<User>().HasData(
        //        new User { Id = 1, Name = "Tom2", Age = 37 },
        //        new User { Id = 2, Name = "Bob2", Age = 41 },
        //        new User { Id = 3, Name = "Sam2", Age = 24 }
        //);
        modelBuilder.Entity<Subscribe>().HasKey(s => new { s.AuthorId, s.SubscriberId });
        Console.WriteLine("Модель создалась");
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=helloapp.db");
    }

}