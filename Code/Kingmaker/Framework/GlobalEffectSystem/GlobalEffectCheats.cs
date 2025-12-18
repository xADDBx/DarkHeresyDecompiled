using System;
using Core.Cheats;

namespace Kingmaker.Framework.GlobalEffectSystem;

public static class GlobalEffectCheats
{
	[Cheat(Name = "set_global_effect")]
	public static void SetGlobalEffect(BlueprintGlobalEffect effect, float weight)
	{
		GlobalEffectDirector.Shared.SetWeightFromCheat(effect, Math.Clamp(weight, 0f, 1f));
	}

	[Cheat(Name = "remove_global_effect")]
	public static void RemoveGlobalEffect(BlueprintGlobalEffect effect)
	{
		GlobalEffectDirector.Shared.RemoveWeightFromCheat(effect);
	}
}
