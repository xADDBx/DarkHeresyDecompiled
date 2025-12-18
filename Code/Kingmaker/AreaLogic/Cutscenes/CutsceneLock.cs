using System.Linq;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;

namespace Kingmaker.AreaLogic.Cutscenes;

public static class CutsceneLock
{
	public static bool Active
	{
		get
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.Cutscene))
			{
				return Game.Instance.CurrentModeType == GameModeType.CutsceneGlobalMap;
			}
			return true;
		}
	}

	public static void CheckRequest()
	{
		if (!Active && Game.Instance.EntityPools.Cutscenes.Any((CutscenePlayerData v) => v.RequireLockControl))
		{
			Game.Instance.GameCommandQueue.ScheduleSwitchCutsceneLock(@lock: true);
		}
	}

	public static void CheckRelease()
	{
		if (Active && Game.Instance.EntityPools.Cutscenes.All((CutscenePlayerData v) => !v.HasActiveLockControl))
		{
			Game.Instance.GameCommandQueue.ScheduleSwitchCutsceneLock(@lock: false);
		}
	}
}
