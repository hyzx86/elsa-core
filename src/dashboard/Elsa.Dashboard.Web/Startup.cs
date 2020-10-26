using Elsa.Activities.Email.Extensions;
using Elsa.Activities.Http.Extensions;
using Elsa.Activities.Timers.Extensions;
using Elsa.Dashboard.Extensions;
using Elsa.Persistence.MongoDb.Extensions;
using Elsa.Persistence.YesSql.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data;
using YesSql;
using YesSql.Provider.SqlServer;

namespace Elsa.Dashboard.Web
{
    public class Startup
    {
        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var elsaSection = Configuration.GetSection("Elsa");
            
            services
                // Add workflow services.
                .AddElsa(x =>x.AddYesSqlStores(  options =>
                        options.UseSqlServer(@"server=(localdb)\MSSQLLocalDB;Integrated Security=True;Database=Elsa;", IsolationLevel.ReadUncommitted)
                            .UseDefaultIdGenerator()
                            .SetTablePrefix("elsa_")
                        )
                    )
                //x.AddMongoDbStores(Configuration, "Elsa", "MongoDb"))                
                // Add activities we'd like to use.
                // Configuring the activities as is done here is only required if we want to be able to actually run workflows form this application.
                // Otherwise it's only necessary to register activities for the workflow designer to discover.
                .AddHttpActivities(options => options.Bind(elsaSection.GetSection("Http")))
                .AddEmailActivities(options => options.Bind(elsaSection.GetSection("Smtp")))
                .AddTimerActivities(options => options.Bind(elsaSection.GetSection("Timers")))
                
                // Add Dashboard services.
                .AddElsaDashboard();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) 
                app.UseDeveloperExceptionPage();

            app
                // This is only necessary if we want to be able to run workflows containing HTTP activities from this application. 
                .UseHttpActivities()
                
                .UseStaticFiles()
                .UseRouting()
                .UseEndpoints(endpoints => endpoints.MapControllers())
                .UseWelcomePage();
        }
    }
}