using System;

namespace ScreenShotter.Web.Messages
{
	public interface Computing : TheSagaOfAWebShot
	{
	}

	[Serializable]
	public class ComputingImpl : Computing
	{
		public Guid CorrelationId { get; set; }
	}
}