using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using tracktor.app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.ServiceModel;
using Microsoft.IdentityModel.Protocols;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using tracktor.app.Helpers;

namespace tracktor.app
{
    public class Startup
    {
        private IHostingEnvironment _env;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthentication().AddCookie(o =>
            {
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        return SecurityStampValidator.ValidatePrincipalAsync(context);
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.FromResult(0);
                    },
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
                o.Lockout.MaxFailedAccessAttempts = 3;
                o.Lockout.AllowedForNewUsers = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
                options.Filters.AddService(typeof(AngularAntiforgeryCookieResultFilter));
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.Configure<SecurityStampValidatorOptions>(o =>
            {
                o.ValidationInterval = TimeSpan.Zero;
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-Token");

            services.AddTransient<IEmailSender, EmailSender>(sp => new EmailSender(
                Configuration["Tracktor:SmtpHost"],
                Int32.Parse(Configuration["Tracktor:SmtpPort"]),
                Boolean.Parse(Configuration["Tracktor:SmtpSsl"]),
                Configuration["Tracktor:SmtpSender"],
                Configuration["Tracktor:SmtpUsername"],
                Configuration["Tracktor:SmtpPassword"]));

            services.AddSingleton(CreateServiceClient());

            services.AddSingleton(Configuration);

            services.AddTransient<AngularAntiforgeryCookieResultFilter>();
        }

        private ITracktorService CreateServiceClient()
        {
            var uri = new Uri(Configuration["Tracktor:ServiceUrl"]);
            HttpBindingBase httpBinding;

            if (uri.Scheme == "http")
            {
                var binding = new BasicHttpBinding();
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                httpBinding = binding;
            }
            else
            {
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                httpBinding = binding;
            }
            var identity = new DnsEndpointIdentity("");
            var address = new EndpointAddress(uri, identity, new AddressHeader[0]);
            var factory = new ChannelFactory<ITracktorService>(httpBinding, address);
            ClientCredentials loginCredentials = new ClientCredentials();
            loginCredentials.UserName.UserName = "tracktor";
            loginCredentials.UserName.Password = Configuration["Tracktor:ServicePassword"];
            var defaultCredentials = factory.Endpoint.EndpointBehaviors.OfType<ClientCredentials>().First();
            factory.Endpoint.EndpointBehaviors.Remove(defaultCredentials);
            factory.Endpoint.EndpointBehaviors.Add(loginCredentials);
            return factory.CreateChannel();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _env = env;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseMiddleware(typeof(ExceptionHandling));
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });

            Initialize(app.ApplicationServices).Wait();
        }

        protected async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                // drop test db
                if (_env.IsEnvironment("Test"))
                {
                    await dbContext.Database.EnsureDeletedAsync();
                }

                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    await dbContext.Database.MigrateAsync();

                    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    foreach (var role in new string[] { "User" })
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                }
            }
        }
    }
}
