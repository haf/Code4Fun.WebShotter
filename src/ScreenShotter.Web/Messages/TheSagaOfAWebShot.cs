using System;
using MassTransit;

namespace ScreenShotter.Web.Messages
{
	public interface TheSagaOfAWebShot : CorrelatedBy<Guid>
	{
	}
}