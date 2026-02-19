using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotNetNuke.Web.MvcPipeline.ModelFactories;

namespace RocketDirectoryRazor
{
    /// <summary>
    /// Startup class to register the Rocket skin model factory decorator
    /// for the RocketDirectoryRazor module.
    /// </summary>
    public class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            // Replace the default ISkinModelFactory with our decorator
            // Our decorator wraps the original SkinModelFactory and adds Rocket skin logic
            services.Replace(ServiceDescriptor.Transient<ISkinModelFactory, RocketSkinModelFactory>());
        }
    }
}
