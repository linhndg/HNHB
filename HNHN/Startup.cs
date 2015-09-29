using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HNHB.Startup))]
namespace HNHB
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
