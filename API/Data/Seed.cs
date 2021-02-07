using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        // ASPNET identity gives us a userManager
        public static async Task SeedUsers(UserManager<AppUser> userManager, 
            RoleManager<AppRole> roleManager)
        {
            // don't seed the db if it already has users
            if (await userManager.Users.AnyAsync()) return;
            
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            if (users == null) return;

            // prepare to apply roles to the seeded users.
            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
            };

            // create these roles and save them to db
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            };

            // note that this is just seed data for development purposes
            // now create the users and also assign them a role as member.
            foreach (var user in users)
            {
                // set the initial photo to approved for seed users
                user.Photos.First().IsApproved = true;
                user.UserName = user.UserName.ToLower();

                // tell entity framework to track this new user (refactored to include userManager from ASPNET identity which takes care of saving it to the db)
                // await context.Users.AddAsync(user);

                // EF tracks and saves to DB here under ASPNET identity
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }
            // now we async save this to the db. (refactored to include userManager from ASPNET identity which takes care of saving it to the db)
            // await context.SaveChangesAsync();

            // now, for dev purposes, seed an admin.
            var admin = new AppUser
            {
                UserName = "admin"
            };

            // save the admin to the db. Then given them the role of Admin and Moderator.
            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        }
    }
}