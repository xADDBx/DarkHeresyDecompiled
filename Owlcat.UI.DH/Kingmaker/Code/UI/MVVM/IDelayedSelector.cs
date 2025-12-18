using System;

namespace Kingmaker.Code.UI.MVVM;

public interface IDelayedSelector
{
	bool IsRunning { get; }

	void InvokeNextFrame(Action action);

	void Stop();

	void Clear();
}
