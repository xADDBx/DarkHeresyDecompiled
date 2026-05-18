using System;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Visual.Animation;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("b276889454351b14bb73a55d3f9936c7")]
public class SystemMechanicsRoot : BlueprintScriptableObject
{
	public bool UseLightweightUnit;

	public bool CompanionsAI;

	public int CustomCompanionBaseCost = 1;

	public int StandartPerceptionRadius = 5;

	public int AreaEffectAutoDestroySeconds = 30;

	public int MinSprintDistance = 10;

	public int MaxWalkDistance = 2;

	public int MinSprintDistanceInCombatCells = 10;

	public int MaxWalkDistanceInCombatCells = 2;

	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintFaction> m_PlayerFaction;

	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintUnit> m_CustomCompanion;

	[Obsolete("VS")]
	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintFeature> m_NavigatorOccupation;

	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintUnlockableFlag> m_KingFlag;

	public AnimationSet HumanAnimationSet;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_CommonMobFact;

	[ValidateNotNull]
	public BpRef<BlueprintBodyPart> FallbackBodyPart;

	[ValidateNoNullEntries]
	public BpRef<BlueprintBodyPart>[] DefaultHumanoidBodyParts;

	[SerializeField]
	private int _criticalEffectsCountOnHit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_CommonDestructibleEntityFact;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_CommonSpaceMarineFact;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_DefaultMapObjectBlueprint;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_ProneCommonBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_DisabledCommonBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_SummonedUnitBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_LeaderCommonBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_DefaultUnit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_EmptyMechanicEntity;

	public GameObject FadeOutFx;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_SummonedUnitAppearBuff;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFactionReference m_FactionNeutrals;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFactionReference m_FactionCutsceneNeutral;

	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintVendorRoot> m_VendorRoot;

	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintItem> m_Money;

	[Header("Assassin")]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintEntityPropertyReference m_AssassinLethalityProperty;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFeatureReference m_AssassinCareerPath;

	[Header("Reach hit FX")]
	public float ReachFXBaseRange = 1.5f;

	public GameObject ReachFXTargetPrefab;

	public GameObject ReachFXMovingPrefab;

	public string ReachFXLocatorName = "Locator_HitFX_00";

	public float ReachFXFlightTime = 0.3f;

	[Header("Tutorial")]
	public int TutorialCooldownSeconds = 60;

	public float TutorialDelaySeconds = 3f;

	public float TutorialDelaySecondsAfterLoading = 10f;

	public PropertyCalculatorBlueprint AssassinLethalityProperty => m_AssassinLethalityProperty;

	public BlueprintFeature AssassinCareerPath => m_AssassinCareerPath;

	public BlueprintUnitFact CommonMobFact => m_CommonMobFact;

	public BlueprintMechanicEntityFact CommonDestructibleEntityFact => m_CommonDestructibleEntityFact;

	public BlueprintUnitFact CommonSpaceMarineFact => m_CommonSpaceMarineFact;

	public BlueprintMechanicEntityFact DefaultMapObjectBlueprint => m_DefaultMapObjectBlueprint;

	public BlueprintBuff SummonedUnitBuff => m_SummonedUnitBuff?.Get();

	public BlueprintUnit DefaultUnit => m_DefaultUnit?.Get();

	public BlueprintMechanicEntityFact EmptyMechanicEntity => m_EmptyMechanicEntity;

	public BlueprintBuff SummonedUnitAppearBuff => m_SummonedUnitAppearBuff?.Get();

	public BlueprintFaction FactionNeutrals => m_FactionNeutrals?.Get();

	public BlueprintFaction FactionCutsceneNeutral => m_FactionCutsceneNeutral?.Get();

	public BlueprintVendorRoot VendorRoot => m_VendorRoot;

	public BlueprintFaction PlayerFaction => m_PlayerFaction;

	public BlueprintUnit CustomCompanion => m_CustomCompanion;

	[Obsolete("VS")]
	public BlueprintFeature NavigatorOccupation => m_NavigatorOccupation;

	public BlueprintUnlockableFlag KingFlag => m_KingFlag;

	public BlueprintBuff ProneCommonBuff => m_ProneCommonBuff;

	public BlueprintBuff DisabledCommonBuff => m_DisabledCommonBuff;

	public int CriticalEffectsCountOnHit => _criticalEffectsCountOnHit;

	public BlueprintItem Money => m_Money;
}
