using System.Linq;
using Core.Cheats;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.Framework.CutsceneSystem;

namespace Kingmaker.Cheats;

internal static class CheatsCutscenes
{
	[Cheat(Name = "stop_cutscene", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StopCutscenes(BlueprintCutscene cutscene)
	{
		foreach (CutscenePlayerData item in Game.Instance.EntityPools.Cutscenes.Where((CutscenePlayerData p) => p.Cutscene == cutscene))
		{
			item.Stop();
		}
	}

	[Cheat(Name = "play_cutscene", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void PlayCutscene(BlueprintCutscene cutscene)
	{
		CutscenePlayerView.Play(cutscene, null);
	}
}
