using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(App_Mundial_Miles.Startup))]
namespace App_Mundial_Miles
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
