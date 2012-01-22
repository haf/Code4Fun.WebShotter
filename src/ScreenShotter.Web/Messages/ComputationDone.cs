using System;

namespace ScreenShotter.Web.Messages
{
	public interface ComputationDone : TheSagaOfAWebShot
	{
		/// <summary>No of ticks taken to process this screenshot.</summary>
		ulong TicksTaken { get; set; }
	}

	[Serializable]
	public class ComputationDoneImpl : ComputationDone
	{
		public ulong TicksTaken { get; set; }
		public Guid CorrelationId { get; set; }
	}
}