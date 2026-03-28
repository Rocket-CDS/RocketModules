using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotNetNuke.Web.MvcPipeline.ModelFactories;

namespace RocketContentRazor
{
    /// <summary>
    /// Startup class to register the Rocket skin model factory decorator
    /// for the RocketContentRazor module.
    /// </summary>
    public class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            // Replace the default ISkinModelFactory with our decorator
            // Our decorator wraps the original SkinModelFactory and adds Rocket skin logic
            // NOTE: This chnages the skin options for all of DNN.
            services.Replace(ServiceDescriptor.Transient<ISkinModelFactory, RocketSkinModelFactory>());
        }
    }
}
