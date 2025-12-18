using System;

namespace Owlcat.UI;

public interface IConsoleHint : IDisposable
{
	void SetLabel(string label);

	ConsoleHint GetRealHint();
}
