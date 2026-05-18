using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;

namespace Code.View.UI.UIUtils;

public static class UIUtilityCombat
{
	public static bool IsCombatLockActive()
	{
		return Game.Instance.Controllers.TurnController.IsCombatLockActive;
	}

	public static bool NeedShowMiniInspect(MechanicEntityUIWrapper? wrapper)
	{
		if (UIConfig.Instance.CombatConfig.DebugFlags.HasFlag(CombatDebugFlags.ShowInspectAlwaysOnHover))
		{
			return wrapper?.AdditionalCombatObjective != null;
		}
		return IsNewAdditionalCombatObj(wrapper);
	}

	private static bool IsNewAdditionalCombatObj(MechanicEntityUIWrapper? wrapper)
	{
		if (wrapper?.AdditionalCombatObjective == null || wrapper.Value.AdditionalCombatObjective.ObjectIsViewed)
		{
			return false;
		}
		return !string.IsNullOrWhiteSpace(wrapper.Value.AdditionalCombatObjective.GetDescription()?.Text);
	}
}
