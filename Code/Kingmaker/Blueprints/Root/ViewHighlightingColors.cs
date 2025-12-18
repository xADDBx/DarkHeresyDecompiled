using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("7e6f7bee062b47ea95672daac684f68e")]
public class ViewHighlightingColors : BlueprintScriptableObject
{
	[Header("Units")]
	public ViewHighlightingColorStates UnitEnemy;

	public ViewHighlightingColorStates UnitAlly;

	public ViewHighlightingColorStates UnitNeutral;

	public ViewHighlightingColorStates UnitDefault;

	[Header("Units > Loot")]
	public ViewHighlightingColorStates UnitLoot;

	public ViewHighlightingColorStates UnitViewedLoot;

	[Header("Loot")]
	public ViewHighlightingColorStates DefualtLoot;

	public ViewHighlightingColorStates ViewedLoot;

	public ViewHighlightingColorStates PerceptedLoot;

	public ViewHighlightingColorStates TrapedLoot;

	[Header("Trap")]
	public ViewHighlightingColorStates Trap;

	[Header("Target")]
	public ViewHighlightingColorStates AdditionalCombatObjective;

	[Header("Interact")]
	public ViewHighlightingColorStates Default;

	public ViewHighlightingColorStates Interaction;

	public ViewHighlightingColorStates Detective;

	[Header("Destructable Entity")]
	public ViewHighlightingColorStates DestructableEntity;
}
