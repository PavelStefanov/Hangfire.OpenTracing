using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OpenTracing;
using OpenTracing.Util;
using System;
using WebApi.Jobs;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "WebApi", Version = "v1"}); });

            services.AddTransient<ExampleJob>();

            var connectionString = Configuration.GetValue<string>("Mongo:ConnectionString");
            var dbName = Configuration.GetValue<string>("Mongo:DbName");
            var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

            services.AddHangfire(c =>
            {
                c.UseMongoStorage(mongoClientSettings, dbName, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    }
                });

                c.UseOpenTracingFilter();
            });
            services.AddHangfireServer();

            services.AddSingleton<ITracer>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var serviceName = sp.GetRequiredService<IWebHostEnvironment>().ApplicationName;

                var reporter = new RemoteReporter.Builder()
                    .WithLoggerFactory(loggerFactory)
                    .WithSender(new UdpSender(string.Empty, 6831, 0))
                    .Build();

                var tracer = new Tracer.Builder(serviceName)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(new ConstSampler(true))
                    .WithReporter(reporter)
                    .Build();

                GlobalTracer.Register(tracer);

                return tracer;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = app.ApplicationServices.GetRequiredService<ITracer>();

            app.UseHangfireServer();

            RecurringJob.AddOrUpdate<ExampleJob>(
                service => service.Run(),
                "*/10 * * * * *", TimeZoneInfo.Utc);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}