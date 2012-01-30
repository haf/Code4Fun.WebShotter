using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Magnum.StateMachine;
using MassTransit;
using MassTransit.Saga;
using ScreenShotter.Web.Messages;
using ScreenShotter.Web.WebShotSagaSupport;

// You can remove the reference to WebActivator by calling the Start() method from your Global.asax Application_Start

namespace ScreenShotter.Web
{
	[Serializable]
	public class WebShotSaga
		: SagaStateMachine<WebShotSaga>,
		  ISaga
	{
		static WebShotSaga()
		{
			Define(() =>
				{
					Initially(
						When(Compute)
							.Then((saga, message) => saga.LaunchProcessing(message.TargetUrl))
						);

					During(Computing,
					       When(ComputationDone)
					       	.Then((s, m) => { })
					       	.Complete(),
					       When(Faulted)
					       	.Then((saga, message) => { })
					       	.Complete());
				});
		}

		private void LaunchProcessing(string url)
		{
			var sw = Stopwatch.StartNew();
			var tres = Task.Factory.StartNew(() =>
				{
					var w = new CutyCaptWrapper();
					// its code is so bad - I have to hack it to make this invocation work properly + error handling:
					var res = w.GetScreenShot(url);
					return res;
				});
			Bus.Publish<Computing>(new{});

			tres.ContinueWith(tsaved =>
				{
					sw.Stop();
					Bus.Publish<ComputationDone>(new
						{
							TicksTaken = (ulong) sw.ElapsedTicks,
							CorrelationId
						});
				});
		}

		public WebShotSaga(Guid correlationId)
		{
			CorrelationId = correlationId;
		}

		[Obsolete("For nhib")]
		protected WebShotSaga()
		{
		}

		public static State Initial { get; set; }
		public static State Computing { get; set; }
		public static State Completed { get; set; }

		public static Event<Compute> Compute { get; set; }
		public static Event<ComputationDone> ComputationDone { get; set; }
		public static Event<GenericFault> Faulted { get; set; }

		public virtual Guid CorrelationId { get; set; }
		public virtual IServiceBus Bus { get; set; }
	}
}