using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Gameplay.Parts;

namespace Code.View.UI.UIUtils;

public static class UIUtilityCombat
{
	public static bool NeedShowMiniInspect(MechanicEntityUIWrapper? wrapper)
	{
		if (UIConfig.Instance.CombatConfig.DebugFlags.HasFlag(CombatDebugFlags.ShowInspectAlwaysOnHover))
		{
			return wrapper?.AdditionalCombatObjective != null;
		}
		return IsNewAdditionalCombatObj(wrapper);
	}

	public static bool IsNewAdditionalCombatObj(MechanicEntityUIWrapper? wrapper)
	{
		if (wrapper?.AdditionalCombatObjective != null)
		{
			PartAdditionalCombatObjectiveUnit additionalCombatObjective = wrapper.Value.AdditionalCombatObjective;
			if (additionalCombatObjective == null)
			{
				return false;
			}
			return !additionalCombatObjective.ObjectIsViewed;
		}
		return false;
	}
}
