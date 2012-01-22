using System;
using System.Web;
using Magnum;
using MassTransit;
using ScreenShotter.Web.Messages;
using Magnum.Extensions;

namespace ScreenShotter.Web
{
	public class ImgLauncherController
	{
		private readonly IEndpoint _sagaRunner;

		public ImgLauncherController(IEndpoint sagaRunner)
		{
			if (sagaRunner == null) throw new ArgumentNullException("sagaRunner");
			_sagaRunner = sagaRunner;
		}

		public WebShotModel Post_ImgHtml(WebShotSpecification spec)
		{
			var sagaId = CombGuid.Generate();
			var waitUrl = ComputeUrl(spec, sagaId);
			var startMsg = ComputeStartMsg(spec, sagaId);

			_sagaRunner.Send<Compute>(startMsg);

			return new WebShotModel { NewUrl = waitUrl };
		}

		private TheSagaOfAWebShot ComputeStartMsg(WebShotSpecification spec, Guid sagaId)
		{
			return new ComputeImpl
				{
					CorrelationId = sagaId,
					TargetUrl = spec.TargetUrl
				};
		}

		private string ComputeUrl(WebShotSpecification spec, Guid sagaId)
		{
			return "/compute/{0}/{1}".FormatWith(sagaId, HttpUtility.UrlEncode(spec.TargetUrl));
		}
	}

	public class WebShotModel
	{
		public string NewUrl { get; set; }
	}

	public class WebShotSpecification
	{
		public string TargetUrl { get; set; }
	}
}