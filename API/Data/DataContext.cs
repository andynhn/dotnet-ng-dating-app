using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    /*
        Because we want to access the different user roles and because we've initially given our entities a key of int
        (instead of ASPNET identity's default), we need to provide type parameters for IdentityDbContext<..>)
        Because we want to specify AppUserRole here, we'll need to specify many others as well
        We cast <int> here because that's what we set the id as initially before refactoring to include ASPNET Identity.
    */ 
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, 
        IdentityUserClaim<int>, AppUserRole, 
        IdentityUserLogin<int>, IdentityRoleClaim<int>, 
        IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        // note that we do not have a DbSet for User anymore. That's provided by IdentityDbContext
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        
        // properties for Signal R group hub message implementation
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }

        // configure Entities for the UserLike feature. Need this or we may get errors when we add a migration.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // relationship between app user and user roles: an app user can have many user roles. 
            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            // relatinoship bewteen app role and user roles: an app role can have many users.
            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            // configure the primary key ourselves instead of auto increment. combo of source user and liked user.
            builder.Entity<UserLike>()
                .HasKey(k => new {k.SourceUserId, k.LikedUserId});

            // the source user.
            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);  // if you are using SQL server, you need to set the DeleteBehavior to DeleteBehavior.NoAction

            // the liked user
            builder.Entity<UserLike>()
                .HasOne(s => s.LikedUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // configure for the Message feature. Recipient side
            // a message has one recipient with many messages that can be received.
            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict); // only remove if other party deleted it as well
            
            // configure message feature. sender side.
            // a message has one sender with many messages that can be sent.
            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict); // only remove if other party deleted it as well.
            
            
        }

    }
}