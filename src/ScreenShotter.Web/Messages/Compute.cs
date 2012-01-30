namespace ScreenShotter.Web.Messages
{
	public interface Compute : TheSagaOfAWebShot
	{
		/// <summary>The url to fetch and create an image off of.</summary>
		string TargetUrl { get; set; }
	}
}