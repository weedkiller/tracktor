using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(tracktor.Startup))]
namespace tracktor
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
