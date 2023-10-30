using Microsoft.EntityFrameworkCore;
using GoDisneyBlog.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace GoDisneyBlog.Data
{
    public class GoDisneyContext : IdentityDbContext<StoreUser>
    {
        public GoDisneyContext(DbContextOptions<GoDisneyContext> options) : base(options)
        { }

        public DbSet<Card>? Cards { get; set; }
        public DbSet<CardList>? CardLists { get; set; }
        public DbSet<RememberMe>? DecryptionKeys { get; set; }
        public DbSet<ContactForm>? ContactForms { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    builder.Entity<IdentityUserRole<string>>()
        //        .HasOne(u => u.RoleId)
        //        .WithMany()
        //        .HasForeignKey(usrRole => usrRole.UserId)
        //        .IsRequired()
        //        .OnDelete(DeleteBehavior.Cascade);
        //}
    }


}
