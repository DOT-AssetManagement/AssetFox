using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using BridgeCare.Models;
using BridgeCare.Security;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using Unity;

[assembly: OwinStartup(typeof(BridgeCare.Startup))]

namespace BridgeCare
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // Configs to run hangfire dashboard. Right now, it has been configured only for localhost
            var container = UnityConfig.Container;

            Hangfire.GlobalConfiguration.Configuration.UseActivator(new ContainerJobActivator(container));
            var dashboardPath = "/hangfire";

            app.UseHangfireAspNet(GetHangfireServers);

            app.UseHangfireDashboard(dashboardPath);

            //app.UseHangfireDashboard(dashboardPath, new DashboardOptions
            //{
            //    Authorization = new[] { new MyAuthorizationFilter() }
            //});
        }

        private IEnumerable<IDisposable> GetHangfireServers()
        {
            Hangfire.GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage("BridgeCareContext");

            yield return new BackgroundJobServer();
        }
        protected class ContainerJobActivator : JobActivator
        {
            private IUnityContainer _container;

            public ContainerJobActivator(IUnityContainer container)
            {
                _container = container;
            }

            public override object ActivateJob(Type type)
            {
                return _container.Resolve(type);
            }
        }

        protected class MyAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                //if (HttpContext.Current.User.IsInRole(Role.ADMINISTRATOR))
                //{
                //    return true;
                //}

                //return false;
                var owinContext = new OwinContext(context.GetOwinEnvironment());
                //return HttpContext.Current.User.Identity.IsAuthenticated;
                return owinContext.Authentication.User.Identity.IsAuthenticated;
            }
        }
    }
}
