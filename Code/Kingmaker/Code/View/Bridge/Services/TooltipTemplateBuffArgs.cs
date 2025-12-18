using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Services;

public readonly struct TooltipTemplateBuffArgs
{
	public Buff Buff { get; }

	public IEntity OverrideCaster { get; }

	public bool IsConcentration { get; }

	public Sprite OverrideIcon { get; }

	public string OverrideName { get; }

	public TooltipTemplateBuffArgs(Buff buff, IEntity overrideCaster = null, bool isConcentration = false, Sprite overrideIcon = null, string overrideName = null)
	{
		Buff = buff;
		OverrideCaster = overrideCaster;
		IsConcentration = isConcentration;
		OverrideIcon = overrideIcon;
		OverrideName = overrideName;
	}
}
