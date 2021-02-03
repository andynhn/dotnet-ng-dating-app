using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            // Need to AddIdentityCore to configure the app to use ASPNET Identity.
            // services.AddIdentity if you have an MVC app where client side is served by dotnet via Razor pages. Gives pages, cookie based auth, user would manitain connection with server
            // since this is a SPA using angular with token-based auth, we want addIdentityCore, while adding extras that we need.
            services.AddIdentityCore<AppUser>(opt => 
            {
                opt.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>() // Role Manager and the Type that it's going to use
                .AddSignInManager<SignInManager<AppUser>>() // Sign in Manager and the Type that it's going to use
                .AddRoleValidator<RoleValidator<AppRole>>() // Role Validator and the type that it needs to use
                .AddEntityFrameworkStores<DataContext>();   // DataContext so that it sets up the DB with all the tables that we need for the dotnet identity tables.

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => 
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            // need to configure these services to add our custom admin roles under ASPNET Identity.
            services.AddAuthorization(opt => 
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }

    }
}