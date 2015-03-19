// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

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
using Autofac.Integration.Wcf;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Configuration;
[assembly: OwinStartup(typeof(tracktor.web.Startup))]

namespace tracktor.web
{
    public class TracktorServiceClient
    {
        public static ITracktorService Create()
        {
            var httpBinding = new BasicHttpsBinding("BasicHttpsBinding_ITracktorService");
            var identity = new DnsEndpointIdentity("");
            var address = new EndpointAddress(new Uri(ConfigurationManager.AppSettings["ServiceUrl"]), identity, new AddressHeaderCollection());
            var factory = new ChannelFactory<ITracktorService>(httpBinding, address);
            ClientCredentials loginCredentials = new ClientCredentials();
            loginCredentials.UserName.UserName = "tracktor";
            loginCredentials.UserName.Password = ConfigurationManager.AppSettings["ServicePassword"];
            var defaultCredentials = factory.Endpoint.Behaviors.Find<ClientCredentials>();
            factory.Endpoint.Behaviors.Remove(defaultCredentials);
            factory.Endpoint.Behaviors.Add(loginCredentials);
            return factory.CreateChannel();
        }
    }

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // autofac
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(WebApiApplication).Assembly);
            builder.Register<ITracktorService>((c, p) => TracktorServiceClient.Create()).InstancePerRequest();
            var config = GlobalConfiguration.Configuration;
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(config);
            builder.RegisterType<TracktorService>();
            TracktorStartup.ConfigureAutofac(builder);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            AutofacHostFactory.Container = container;
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();

            ConfigureAuth(app);
        }
    }
}
