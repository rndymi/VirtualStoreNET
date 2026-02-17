using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VirtualStore.Startup))]
namespace VirtualStore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
