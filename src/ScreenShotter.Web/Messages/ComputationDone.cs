namespace ScreenShotter.Web.Messages
{
	public interface ComputationDone : TheSagaOfAWebShot
	{
		/// <summary>No of ticks taken to process this screenshot.</summary>
		ulong TicksTaken { get; set; }
	}
}