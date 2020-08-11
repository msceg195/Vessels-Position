using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Vessels_Position.Startup))]

namespace Vessels_Position
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
