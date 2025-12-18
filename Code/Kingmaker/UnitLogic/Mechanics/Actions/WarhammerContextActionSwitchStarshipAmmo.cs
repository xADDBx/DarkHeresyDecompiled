using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("18f04b6b29193e84faec69aae8e87c65")]
public class WarhammerContextActionSwitchStarshipAmmo : ContextAction
{
	[SerializeField]
	private BlueprintStarshipWeapon.Reference m_Weapon;

	[SerializeField]
	private BlueprintStarshipAmmo.Reference m_Ammo;

	[SerializeField]
	private bool m_ReloadInstantly;

	public override string GetCaption()
	{
		return "Switch " + m_Weapon?.Get()?.name + " ammo to " + m_Ammo?.Get()?.name;
	}

	protected override void RunAction()
	{
	}
}
