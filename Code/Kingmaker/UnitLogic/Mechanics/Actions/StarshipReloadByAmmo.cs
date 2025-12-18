using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("044aea76c9fc23843b947396486cb518")]
public class StarshipReloadByAmmo : ContextAction
{
	public StarshipWeaponType WeaponType;

	[SerializeField]
	private BlueprintStarshipAmmo.Reference m_AmmoReference;

	public int reloadChancesForMatching;

	public int reloadChancesOtherwise;

	public ActionList ReloadActions;

	public BlueprintStarshipAmmo AmmoReference => m_AmmoReference?.Get();

	public override string GetCaption()
	{
		return "Run actions if starship weapon requires reloading";
	}

	protected override void RunAction()
	{
	}
}
