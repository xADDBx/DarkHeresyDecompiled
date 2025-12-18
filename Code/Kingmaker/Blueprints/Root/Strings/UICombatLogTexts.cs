using System;
using Kingmaker.Localization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICombatLogTexts
{
	[Space]
	public LocalizedString MoraleEventCombatStart;

	public LocalizedString MoraleEventTurnStart;

	public LocalizedString MoraleEventAllyDeath;

	public LocalizedString MoraleEventEnemyDeath;

	public LocalizedString MoraleEventKillingEnemy;

	public LocalizedString MoraleEventLeaderAllyDeath;

	public LocalizedString MoraleEventLeaderEnemyDeath;

	public LocalizedString MoraleEventRestoreToRegular;

	public LocalizedString MoraleEventBecomeHeroic;

	public LocalizedString MoraleEventBecomeBroken;

	public LocalizedString MoraleEventForcedChangeMorale;

	public LocalizedString MoraleEventForcedChangeMoralePhase;

	public LocalizedString MoralePhaseBroken;

	public LocalizedString MoralePhaseHeroic;

	public LocalizedString MoraleAutoregen;

	public LocalizedString MoraleTopLimit;

	public LocalizedString MoraleBottomLimit;

	[Space]
	public LocalizedString ScatterShotHits;

	public LocalizedString ScatterShotCoverHits;

	public LocalizedString ScatterShotMiss;

	[Space]
	public LocalizedString ShotDirectionDeviation;

	public LocalizedString CentralShotDirection;

	public LocalizedString SlightDeviationShotDirection;

	public LocalizedString StrongDeviationShotDirection;

	public LocalizedString DeviationDescription;

	public LocalizedString DeviationHeader;

	[Space]
	public LocalizedString ChangeSize;

	public LocalizedString ShowModePin;

	public LocalizedString ShowModeUnpin;

	public LocalizedString ShowUnit;

	[FormerlySerializedAs("MoraleWasChanged")]
	public LocalizedString MoraleSourcesHeader;
}
