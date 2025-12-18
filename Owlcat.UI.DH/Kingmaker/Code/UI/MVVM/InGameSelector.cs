using System;
using Kingmaker.Controllers;
using Kingmaker.Utility.ManualCoroutines;

namespace Kingmaker.Code.UI.MVVM;

public class InGameSelector : IDelayedSelector
{
	private CoroutineHandler m_DelayedApplySelection;

	public bool IsRunning => m_DelayedApplySelection.IsRunning;

	public void InvokeNextFrame(Action action)
	{
		m_DelayedApplySelection = Game.Instance.Controllers.CoroutinesController.InvokeInTicks(action, 1);
	}

	public void Stop()
	{
		m_DelayedApplySelection.Stop();
	}

	public void Clear()
	{
		Stop();
		m_DelayedApplySelection = default(CoroutineHandler);
	}
}
