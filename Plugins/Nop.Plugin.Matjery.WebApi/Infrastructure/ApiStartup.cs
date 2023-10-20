using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Nop.Core.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Services;
using Nop.Services.Authentication.External;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.Matjery.WebApi.Infrastructure
{
    public class ApiStartup : INopStartup
    {
        public int Order => 1;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nop API", Version = "v1" });
            });

            services.AddSwaggerGenNewtonsoftSupport();

            services.AddScoped<ICategoryPluginService, CategoryPluginService>();
            services.AddScoped<IProductPluginService, ProductPluginService>();
            services.AddScoped<IUAEPassPluginService, UAEPassPluginService>();
            services.AddScoped<ICustomerPluginService, CustomerPluginService>();
            services.AddScoped<IAddressPluginService, AddressPluginService>();
            services.AddScoped<IRegionPluginService, RegionPluginService>();
            services.AddScoped<IBlackpointPluginService, BlackPointPluginService>();
            services.AddScoped<ICartPluginService, CartPluginService>();
            services.AddScoped<IOrderPluginService, OrderPluginService>();
            services.AddScoped<IWishListPluginService, WishListPluginService>();
            services.AddScoped<IProductAttributePluginService, ProductAttributePluginService>();
            services.AddScoped<ICheckOutPluginService, CheckOutPluginService>();
            services.AddScoped<ICampaignPluginService, CampaignPluginService>();
            services.AddScoped<IPushNotificationPluginService, PushNotificationPluginService>();
            services.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();
            services.AddScoped<IReviewPluginService, ReviewPluginService>();
            services.AddScoped<ISearchPluginService, SearchPluginService>();
            services.AddScoped<IVendorPluginService, VendorPluginService>();
            services.AddScoped<IFollowersPluginService, FollowersPluginService>();
            services.AddScoped<ITopicsPluginService, TopicsPluginService>();
            services.AddScoped<IQueuedPushNotificationService, QueuedPushNotificationService>();
            services.AddScoped<ISyncStatusService, SyncStatusService>();
            services.AddScoped<ILogger,DefaultLogger>();
            services.AddScoped<ITradeLicensePluginService, TradeLicensePluginService>();
            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ILookupPluginService, LookupPluginService>();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(x => x
                             .AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader());

            app.MapWhen(context => context.Request.Path.StartsWithSegments(new PathString("/api")),
                a =>
                {
               

                    a.Use(async (context, next) =>
                    {
                        // API Call
                        context.Request.EnableBuffering();
                        await next();
                    });

                    a.UseRouting();
                    a.UseAuthentication();
                    a.UseAuthorization();
                    a.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });

                    //swagger configuration
                    {
                        a.UseSwagger(options => options.RouteTemplate = "api/swagger/{documentName}/swagger.json");
                        a.UseSwaggerUI(c =>
                        {
                            c.RoutePrefix = "";
                            c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Nop.Plugin.Api v4.40");
                            //c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                        });
                    }
                }
            );

        }
    }
}
