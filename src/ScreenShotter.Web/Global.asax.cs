using System;
using System.Web;
using log4net;
using log4net.Config;

namespace ScreenShotter.Web
{
	public class ScreenShotterApp : HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			BasicConfigurator.Configure();
		}

		protected void Application_End(object sender, EventArgs e)
		{
			LogManager.Shutdown();
		}
	}
}