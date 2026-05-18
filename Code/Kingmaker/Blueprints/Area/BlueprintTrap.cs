using System;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[TypeId("6b7601a27a6eb6240b34483588f51f3d")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintTrap : BlueprintMapObject
{
	public class ElementsData : ContextData<ElementsData>
	{
		public BaseUnitEntity TriggeringUnit { get; private set; }

		public TrapObjectData TrapObject { get; private set; }

		public ElementsData Setup(BaseUnitEntity unit, TrapObjectData obj)
		{
			TriggeringUnit = unit;
			TrapObject = obj;
			return this;
		}

		protected override void Reset()
		{
			TriggeringUnit = null;
			TrapObject = null;
		}
	}

	public SkillCheckDifficulty AwarenessDifficulty;

	[SerializeField]
	[HideIf("AwarenessDifficultyIsCustom")]
	public int AwarenessDC = 15;

	public float AwarenessRadius = 7f;

	public int DisableDC = 25;

	public int DisableTriggerMargin = 5;

	public bool IsHiddenWhenInactive = true;

	public bool AllowedForRandomEncounters;

	public UnitAnimationInteractionType DisarmAnimation = UnitAnimationInteractionType.DisarmTrap;

	public ConditionsChecker TriggerConditions;

	public ConditionsChecker DisableConditions;

	public ActionList TrapActions;

	public ActionList DisableActions;

	public StatType DisarmSkill = StatType.SkillDemolition;

	private bool AwarenessDifficultyIsCustom => AwarenessDifficulty == SkillCheckDifficulty.Custom;

	protected override Type GetFactType()
	{
		return null;
	}
}
