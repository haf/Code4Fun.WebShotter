using FubuMVC.Core;
using FubuMVC.Spark;
using ScreenShotter.Web.Handlers;
using ScreenShotter.Web.Handlers.Home;

namespace ScreenShotter.Web
{
	public class ConfigureFubuMVC : FubuRegistry
	{
		public ConfigureFubuMVC()
		{
			// This line turns on the basic diagnostics and request tracing
			IncludeDiagnostics(true);

			ApplyHandlerConventions<HandlersMarker>();

			// Policies
			Routes.HomeIs<GetHandler>(x => x.Execute());

			this.UseSpark();

			// Match views to action methods by matching
			// on model type, view name, and namespace
			Views.TryToAttachWithDefaultConventions();
		}
	}
}