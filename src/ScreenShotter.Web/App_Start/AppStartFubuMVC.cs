using System;
using System.Web.Routing;
using Bottles;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using MassTransit;
using MassTransit.BusConfigurators;
using MassTransit.Pipeline;
using MassTransit.Pipeline.Inspectors;
using MassTransit.Pipeline.Sinks;
using MassTransit.Util;
using StructureMap.Pipeline;
using log4net;

// You can remove the reference to WebActivator by calling the Start() method from your Global.asax Application_Start
[assembly: WebActivator.PreApplicationStartMethod(typeof(ScreenShotter.Web.App_Start.AppStartFubuMVC), "Start", callAfterGlobalAppStart: true)]

namespace ScreenShotter.Web.App_Start
{
	public static class AppStartFubuMVC
	{
		public static void Start()
		{
			// ConfigureFubuMVC is the main FubuRegistry
			// for this application.  FubuRegistry classes 
			// are used to register conventions, policies,
			// and various other parts of a FubuMVC application
			FubuApplication.For<ConfigureFubuMVC>()
				.StructureMap(Bootstrapper.CreateContainer())
				.Bootstrap();

			// Ensure that no errors occurred during bootstrapping
			PackageRegistry.AssertNoFailures();
		}



		public class OutboundMessageInterceptorConfiguratorScope :
			PipelineInspectorBase<MassTransit.Pipeline.Configuration.OutboundMessageInterceptorConfiguratorScope>
		{
			public Func<IPipelineSink<ISendContext>, IPipelineSink<ISendContext>> InsertAfter { get; private set; }

			[UsedImplicitly]
			protected bool Inspect(OutboundMessagePipeline pipeline)
			{
				InsertAfter = (sink => pipeline.ReplaceOutputSink(sink));

				return false;
			}
		}
		class TestInterceptor : IOutboundMessageInterceptor
		{
			private static readonly ILog logger = LogManager.GetLogger(typeof(TestInterceptor));

			public void PreDispatch(ISendContext context)
			{
				logger.Debug(string.Format("Pre-dispatch, {0}", context));
			}

			public void PostDispatch(ISendContext context)
			{
				logger.Debug(string.Format("Post-dispatch, {0}", context));
			}
		}
	}
}