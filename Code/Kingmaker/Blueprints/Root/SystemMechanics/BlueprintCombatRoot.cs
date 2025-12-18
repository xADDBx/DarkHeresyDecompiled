using System;
using Code.Enums;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.SystemMechanics;

[Serializable]
[TypeId("6aacab4e5a45411d94ccd135850df824")]
public class BlueprintCombatRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintCombatRoot>
	{
	}

	[Serializable]
	public class DOTEntry
	{
		public DOT Type;

		[SerializeField]
		[ValidateNotNull]
		private BlueprintBuffReference m_Buff;

		public BlueprintBuff Buff => m_Buff;
	}

	public int BaseOverpenetrationChance = 50;

	public int BurstNextBulletDodgePenalty = 20;

	public int BaseActionPointsRegen = 4;

	public int DistanceInPreparationTurn = 7;

	[Header("Initiative")]
	public InitiativeDistribution[] InitiativeDistributions = new InitiativeDistribution[0];

	[Header("Precise Shot")]
	public int CoveredTargetPreciseHitChancePenalty = 50;

	[Header("Scatter Shot")]
	public int BallisticSkillBonus = 30;

	public int BallisticSkillPercentScaling = 100;

	public int MinEffectiveBallisticSkill = 30;

	public int MaxEffectiveBallisticSkill = 95;

	public int MinResultMainLineChance = 15;

	[Header("DOT Settings")]
	[Obsolete("VS")]
	public DOTEntry[] DOTSettings = new DOTEntry[1]
	{
		new DOTEntry
		{
			Type = DOT.Bleeding
		}
	};

	[Header("Righteous Fury")]
	public int BaseRighteousFury = 10;

	public int HitChanceOverkillBorder = 100;

	[Header("Two Weapon Fighting")]
	public BlueprintAbilityGroupReference PrimaryHandAbilityGroup;

	public BlueprintAbilityGroupReference SecondaryHandAbilityGroup;

	[Header("Misc")]
	public BlueprintAbilityGroupReference DamageOverTimeAbilityGroup;

	[Obsolete("VS")]
	public BlueprintBuffReference AssassinKeystoneBuff;

	[Obsolete("VS")]
	public BlueprintBuffReference AssassinKeystoneBuffOpening;

	public float ForbiddenNodesTraverseCost = 1000f;

	[Header("NavLinks & 2x2 Creatures")]
	public bool UseManhattanHeuristic;

	public bool UseIncreasedCostForLeaps = true;

	public bool UseIncreasedCostForLargeCreatures = true;

	public bool LowerPriorityForLinkNodes = true;

	[Header("Cohesion")]
	public int MinCohesionRange = 1;

	public int MaxCohesionRange = 5;

	public int DefaultCohesionRange = 2;

	public BpRef<BlueprintAreaEffect> CohesionAreaEffect;
}
