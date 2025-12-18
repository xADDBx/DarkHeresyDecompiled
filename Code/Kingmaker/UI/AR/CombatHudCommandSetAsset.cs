using System;
using UnityEngine;

namespace Kingmaker.UI.AR;

[CreateAssetMenu(menuName = "ScriptableObjects/CombatHudCommandSetAsset")]
public sealed class CombatHudCommandSetAsset : ScriptableObject
{
	public CombatHudCommand[] Commands = Array.Empty<CombatHudCommand>();

	public CombatHudAreas GetUsedAreas()
	{
		CombatHudAreas combatHudAreas = (CombatHudAreas)0;
		if (Commands != null)
		{
			CombatHudCommand[] commands = Commands;
			foreach (CombatHudCommand combatHudCommand in commands)
			{
				combatHudAreas |= combatHudCommand.GetUsedAreas();
			}
		}
		return combatHudAreas;
	}
}
