using System.Collections.Generic;
using Kingmaker.Cheats;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter;

public class WaitForCombat : ArbiterTask
{
	public WaitForCombat(ArbiterTask parent)
		: base(parent)
	{
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		yield return null;
		if (Game.Instance.Controllers.TurnController.InCombat)
		{
			Game.Instance.Controllers.TurnController.OnStart();
			CheatsCombat.KillAll(null);
		}
		yield return null;
	}
}
