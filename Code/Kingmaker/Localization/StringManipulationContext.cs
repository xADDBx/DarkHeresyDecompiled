using System;

namespace Kingmaker.Localization;

public class StringManipulationContext : IDisposable
{
	public string CurrentStringKind;

	public void Dispose()
	{
		CurrentStringKind = string.Empty;
	}
}
