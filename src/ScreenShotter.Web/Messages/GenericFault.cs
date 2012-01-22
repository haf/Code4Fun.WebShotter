using System;
using MassTransit;

namespace ScreenShotter.Web.Messages
{
	public interface GenericFault : TheSagaOfAWebShot
	{
		/// <summary>
		/// Gets the description of the fault. Human readable. Initially we
		/// may show this to the user.
		/// </summary>
		string Description { get; set; }
	}
}