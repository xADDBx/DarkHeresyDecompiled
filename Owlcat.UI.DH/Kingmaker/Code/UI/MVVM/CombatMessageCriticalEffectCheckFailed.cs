using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageCriticalEffectCheckFailed : CombatMessageBase
{
	public string EffectName;

	public override string GetText()
	{
		return "Failed to apply " + EffectName;
	}

	public override Sprite GetSprite()
	{
		return ConfigRoot.Instance.UIConfig.UIIcons.Fail;
	}

	public override bool GetAttention()
	{
		return true;
	}
}
