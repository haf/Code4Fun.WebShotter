using System;
using System.Web;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MassTransit;
using MassTransit.NHibernateIntegration.Saga;
using MassTransit.Pipeline.Inspectors;
using MassTransit.Saga;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using ScreenShotter.Web.Handlers.Home;
using StructureMap;

namespace ScreenShotter.Web.App_Start
{
	public class Bootstrapper
	{
		public static IContainer CreateContainer(string sagaDb = null)
		{
			sagaDb = sagaDb ?? HttpContext.Current.Server.MapPath("~/App_Data/saga_state.db");
			if (sagaDb == null) throw new ArgumentNullException("sagaDb");

			var endpointUri = "loopback://localhost/ScreenShotter.Web/";

			// FubuApplication "guides" the bootstrapping of the FubuMVC
			// application
			var container = new Container(x =>
				{
					x.For<ISessionFactory>()
						.Singleton()
						.Add(ctx =>
							{
								var config = Fluently.Configure()
									.Database(SQLiteConfiguration.Standard.UsingFile(sagaDb))
									.Mappings(m => m.FluentMappings.AddFromAssemblyOf<WebShotSagaMap>())
									.BuildConfiguration();
									
								new SchemaUpdate(config).Execute(true, true);

								return config.BuildSessionFactory();
							});

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
					sbc.Subscribe(x => x.LoadFrom(container));
				});

			PipelineViewer.Trace(serviceBus.InboundPipeline);

			//PipelineViewer.Trace(serviceBus.OutboundPipeline);

			container.Inject(serviceBus);
			return container;
		}
	}
}