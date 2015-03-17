using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using Autofac;
using Autofac.Integration.Owin;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using System.Web.Mvc;
using System.Web.Http;
using tracktor.service;
using System.Reflection;


[assembly: OwinStartup(typeof(tracktor.web.Startup))]

namespace tracktor.web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // autofac
            var builder = new ContainerBuilder();            
            builder.RegisterControllers(typeof(WebApiApplication).Assembly);
            builder.RegisterType<TracktorService>().As<ITracktorService>().InstancePerRequest();
            var config = GlobalConfiguration.Configuration;
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(config);
            TracktorStartup.ConfigureAutofac(builder);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();

            ConfigureAuth(app);
        }
    }
}
