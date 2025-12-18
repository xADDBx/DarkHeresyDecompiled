using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.AR;

[CreateAssetMenu(menuName = "ScriptableObjects/CombatHudSurfaceRendererAsset")]
public sealed class CombatHudSurfaceRendererAsset : ScriptableObject
{
	public OutlineSettings outlineSettings = OutlineSettings.Default;

	public FillSettings fillSettings = FillSettings.Default;

	public CombatHudCommand[] deploymentCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] movementCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] abilityRangeCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] abilityPatternRangeCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] abilityPatternCommands = Array.Empty<CombatHudCommand>();

	[FormerlySerializedAs("allyStratagemLoopCommands")]
	public CombatHudCommand[] allyDebugCommands = Array.Empty<CombatHudCommand>();

	[FormerlySerializedAs("hostileStratagemLoopCommands")]
	public CombatHudCommand[] hostileDebugCommands = Array.Empty<CombatHudCommand>();

	[UsedImplicitly]
	private void OnValidate()
	{
		CombatHudCommand[] array = movementCommands;
		foreach (CombatHudCommand combatHudCommand in array)
		{
			combatHudCommand.OnValidate();
		}
		array = abilityRangeCommands;
		foreach (CombatHudCommand combatHudCommand2 in array)
		{
			combatHudCommand2.OnValidate();
		}
		array = abilityPatternRangeCommands;
		foreach (CombatHudCommand combatHudCommand3 in array)
		{
			combatHudCommand3.OnValidate();
		}
		array = abilityPatternCommands;
		foreach (CombatHudCommand combatHudCommand4 in array)
		{
			combatHudCommand4.OnValidate();
		}
		array = allyDebugCommands;
		foreach (CombatHudCommand combatHudCommand5 in array)
		{
			combatHudCommand5.OnValidate();
		}
		array = hostileDebugCommands;
		foreach (CombatHudCommand combatHudCommand6 in array)
		{
			combatHudCommand6.OnValidate();
		}
	}
}
