using System;
using System.Web;
using System.Web.Routing;
using Bottles;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using MassTransit;
using MassTransit.NHibernateIntegration.Saga;
using MassTransit.Pipeline.Inspectors;
using MassTransit.Saga;
using NHibernate;
using ScreenShotter.Web.Handlers.Home;
using StructureMap;
using StructureMap.Pipeline;

// You can remove the reference to WebActivator by calling the Start() method from your Global.asax Application_Start
[assembly: WebActivator.PreApplicationStartMethod(typeof(ScreenShotter.Web.App_Start.AppStartFubuMVC), "Start", callAfterGlobalAppStart: true)]

namespace ScreenShotter.Web.App_Start
{
	public static class AppStartFubuMVC
	{
		public static void Start()
		{
			var sagaDb = HttpContext.Current.Server.MapPath("~/App_Data/saga_state.db");
			var endpointUri = "loopback://localhost/ScreenShotter.Web";

			// FubuApplication "guides" the bootstrapping of the FubuMVC
			// application
			var container = new Container(x =>
				{
					x.For<ISessionFactory>()
						.Singleton()
						.Add(ctx =>
							Fluently.Configure()
								.Database(SQLiteConfiguration.Standard.UsingFile(sagaDb))
								.Mappings(m =>
									m.FluentMappings.AddFromAssemblyOf<ScreenShotterApp>())
								.BuildSessionFactory());

					x.For<ISagaRepository<WebShotSaga>>()
						.Singleton()
						.Use<NHibernateSagaRepository<WebShotSaga>>();

					x.ForConcreteType<WebShotSaga>();

					x.ForConcreteType<PostHandler>().Configure.Ctor<IEndpoint>()
						.Is(ctx => ctx.GetInstance<IServiceBus>().GetEndpoint(new Uri(endpointUri)));

					x.For<IServiceBus>().Singleton();
				});

			var serviceBus = ServiceBusFactory.New(sbc =>
				{
					sbc.UseJsonSerializer();
					sbc.ReceiveFrom(endpointUri);
					sbc.UseControlBus();
					sbc.Subscribe(x => x.LoadFrom(container));
				});

			PipelineViewer.Trace(serviceBus.InboundPipeline);

			PipelineViewer.Trace(serviceBus.OutboundPipeline);

			container.Inject(serviceBus);

			// ConfigureFubuMVC is the main FubuRegistry
			// for this application.  FubuRegistry classes 
			// are used to register conventions, policies,
			// and various other parts of a FubuMVC application
			FubuApplication.For<ConfigureFubuMVC>()
				.StructureMap(container)
				.Bootstrap();

			// Ensure that no errors occurred during bootstrapping
			PackageRegistry.AssertNoFailures();
		}
	}
}