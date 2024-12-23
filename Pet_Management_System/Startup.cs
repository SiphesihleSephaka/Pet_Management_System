using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Pet_Management_System.Startup))]
namespace Pet_Management_System
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
