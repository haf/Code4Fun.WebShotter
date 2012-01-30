using System;
using FluentNHibernate.Mapping;

namespace ScreenShotter.Web
{
	[Serializable]
	public class WebShotSagaMap
		: ClassMap<WebShotSaga>
	{
		public WebShotSagaMap()
		{
			Id(x => x.CorrelationId);
		}
	}
}