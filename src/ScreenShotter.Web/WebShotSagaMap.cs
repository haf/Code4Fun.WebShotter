using System;
using FluentNHibernate.Mapping;
using MassTransit.NHibernateIntegration;

namespace ScreenShotter.Web
{
	[Serializable]
	public class WebShotSagaMap
		: ClassMap<WebShotSaga>
	{
		public WebShotSagaMap()
		{
			Not.LazyLoad();
			Id(x => x.CorrelationId);
			Map(x => x.CurrentState)
				.Access.ReadOnlyPropertyThroughCamelCaseField(Prefix.Underscore)
				.CustomType<StateMachineUserType>();
		}
	}
}