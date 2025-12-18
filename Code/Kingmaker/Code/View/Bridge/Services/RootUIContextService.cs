using System;
using Kingmaker.Code.View.Bridge.Interfaces;

namespace Kingmaker.Code.View.Bridge.Services;

public static class RootUIContextService
{
	public static Func<IRootUIContext> RootUIContextFactory { get; set; }

	public static IRootUIContext CreateRootUIContext()
	{
		if (RootUIContextFactory == null)
		{
			throw new InvalidOperationException("RootUIContextFactory is not initialized.");
		}
		return RootUIContextFactory();
	}
}
