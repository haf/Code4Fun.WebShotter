using System;
using System.Web;
using FubuMVC.Core.Continuations;
using Magnum;
using MassTransit;
using MassTransit.Util;
using ScreenShotter.Web.Messages;
using Magnum.Extensions;

namespace ScreenShotter.Web.Handlers.Home
{
	public class PostHandler
	{
		private readonly IEndpoint _sagaRunner;
		private readonly IServiceBus _bus;

		public PostHandler(IEndpoint sagaRunner, [NotNull] IServiceBus bus)
		{
			if (sagaRunner == null) 
				throw new ArgumentNullException("sagaRunner");
			if (bus == null) throw new ArgumentNullException("bus");

			_sagaRunner = sagaRunner;
			_bus = bus;
		}

		public FubuContinuation Execute(SpecModel spec)
		{
			var sagaId = CombGuid.Generate();
			var waitUrl = "/compute/{0}/{1}".FormatWith(sagaId, HttpUtility.UrlEncode(spec.TargetUrl));

			_bus.Publish<Compute>(new
				{
					CorrelationId = sagaId,
					spec.TargetUrl
				});

			var model = new WaitModel {NewUrl = waitUrl}; //?

			return FubuContinuation.RedirectTo<Snapshotting.GetHandler>(x => x.Execute(model));
		}
	}
}