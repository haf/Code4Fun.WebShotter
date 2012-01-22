using System;
using System.Web;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MassTransit;
using MassTransit.NHibernateIntegration.Saga;
using NHibernate;
using log4net;
using log4net.Config;

namespace ScreenShotter.Web
{
	public class ScreenShotterApp : HttpApplication
	{
		private static ISessionFactory _nhFac;

		protected void Application_Start(object sender, EventArgs e)
		{
			ConfigureLog4Net();
			_nhFac = ConfigureNH();
			ConfigureMT();
		}

		private void ConfigureLog4Net()
		{
			BasicConfigurator.Configure();
		}

		private void ConfigureMT()
		{
			Bus.Initialize(sbc =>
				{
					sbc.UseJsonSerializer();
					sbc.Subscribe(s => s.Saga(new NHibernateSagaRepository<WebShotSaga>(_nhFac)));
					sbc.ReceiveFrom("loopback://ScreenShotter.Web");
					sbc.UseControlBus();
				});
		}

		private ISessionFactory ConfigureNH()
		{
			return Fluently.Configure()
				.Database(
					SQLiteConfiguration.Standard
						.UsingFile(Server.MapPath("~/App_Data/saga_state.db"))
				)
				.Mappings(m =>
				          m.FluentMappings.AddFromAssemblyOf<ScreenShotterApp>())
				.BuildSessionFactory();
		}

		protected void Session_Start(object sender, EventArgs e)
		{
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected void Application_Error(object sender, EventArgs e)
		{
		}

		protected void Session_End(object sender, EventArgs e)
		{
		}

		protected void Application_End(object sender, EventArgs e)
		{
			StopMT();
			StopNH();
			StopLog4Net();
		}

		private void StopNH()
		{
			_nhFac.Dispose();
		}

		private void StopLog4Net()
		{
			LogManager.Shutdown();
		}

		private void StopMT()
		{
			Bus.Shutdown();
		}
	}
}