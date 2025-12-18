using System.Collections.Generic;
using Kingmaker.GameModes;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter;

public class WaitForDialogTask : ArbiterTask
{
	public WaitForDialogTask(ArbiterTask parent)
		: base(parent)
	{
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		yield return null;
		if (Game.Instance.CurrentModeType == GameModeType.Dialog)
		{
			Game.Instance.Controllers.DialogController.StopDialog();
		}
		yield return null;
	}
}
