using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Extensions;
using API.Interfaces;
using API.Middleware;
using API.Services;
using API.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationServices(_config);   // These extension methods (e.g. AddIdentityServices and AddApplicationServices within the Extensions folder) save us from typing repetitive code. We can just use the methods from these extensions throughout our app where we need them. Try to keep Startup class as clean as possible.
            services.AddControllers();
            services.AddCors();     //implement cors
            services.AddIdentityServices(_config);
            // need to add signal R to services here
            services.AddSignalR();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            // exception handling comes first

            // commented out for now to show how to develop your own exception handling middleware (above)
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            //     app.UseSwagger();
            //     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            // }

            app.UseHttpsRedirection();

            app.UseRouting();

            // order matters. UseCors, then UseAuthentication, then UseAuthorization
            app.UseCors(x => x.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins("https://localhost:4200"));     //implement cors

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseDefaultFiles(); // if there is an index.html inside there, it will use that (our angular app uses it);
            app.UseStaticFiles(); // need this as well to serve the angular static files.

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // need to add Signal R hub here. Specify the hubs that you use.
                endpoints.MapHub<PresenceHub>("hubs/presence");
                // for our message hub
                endpoints.MapHub<MessageHub>("hubs/message");
                // configure endpoint to hit our fallback controller, to help serve Angular app from index.html in wwwroot folder in production
                // Index is the name of the action (only 1 method in the fallback controller), then the name of the controller.
                endpoints.MapFallbackToController("Index", "Fallback");
            });
        }
    }
}
