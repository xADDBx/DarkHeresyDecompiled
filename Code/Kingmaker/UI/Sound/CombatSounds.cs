using System;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class CombatSounds
{
	[Serializable]
	public class UISoundCombat
	{
		[field: SerializeField]
		public UISound NotInCombatSetWaypointClick { get; private set; }

		[field: SerializeField]
		public UISound ActionBarSlotClick { get; private set; }

		[field: SerializeField]
		public UISound ActionBarCanNotSlotClick { get; private set; }

		[field: SerializeField]
		public UISound MomentumHeroicActReached { get; private set; }

		[field: SerializeField]
		public UISound MomentumDesperateMeasuresReached { get; private set; }

		[field: SerializeField]
		public UISound MomentumHighlightOn { get; private set; }

		[field: SerializeField]
		public UISound MomentumHighlightOff { get; private set; }

		[field: SerializeField]
		public UISound CombatStart { get; private set; }

		[field: SerializeField]
		public UISound CombatEnd { get; private set; }

		[field: SerializeField]
		public UISound EndTurn { get; private set; }

		[field: SerializeField]
		public UISound NewRound { get; private set; }

		[field: SerializeField]
		public UISound CombatGridHover { get; private set; }

		[field: SerializeField]
		public UISound CombatGridSetWaypointClick { get; private set; }

		[field: SerializeField]
		public UISound CombatGridClearWaypoint { get; private set; }

		[field: SerializeField]
		public UISound CombatGridConfirmActionClick { get; private set; }

		[field: SerializeField]
		public UISound CombatGridCantPerformActionClick { get; private set; }

		[field: SerializeField]
		public UISound UnitDeath { get; private set; }

		[field: SerializeField]
		public UISound ExitBattlePopupShow { get; private set; }

		[field: SerializeField]
		public UISound ExitBattlePopupExperienceGrowStart { get; private set; }

		[field: SerializeField]
		public UISound ExitBattlePopupExperienceGrowStop { get; private set; }

		[field: SerializeField]
		public UISound PrecisionShotOn { get; private set; }

		[field: SerializeField]
		public UISound PrecisionShotOff { get; private set; }

		[field: SerializeField]
		public UISound PrecisionShotTargetChange { get; private set; }

		[field: SerializeField]
		public UISound PreciseAttackConfirm { get; private set; }

		[field: SerializeField]
		public UISound PreparationTurnDeployUnit { get; private set; }

		[field: SerializeField]
		public UISound CursorNotificationMessage { get; private set; }
	}

	[Serializable]
	public class UISoundActionBar
	{
		[field: SerializeField]
		public UISound Show { get; private set; }

		[field: SerializeField]
		public UISound Hide { get; private set; }

		[field: SerializeField]
		public UISound ActionBarSwitch { get; private set; }

		[field: SerializeField]
		public UISound WeaponListOpen { get; private set; }

		[field: SerializeField]
		public UISound WeaponListClose { get; private set; }

		[field: SerializeField]
		public UISound DPadShow { get; private set; }

		[field: SerializeField]
		public UISound DPadHide { get; private set; }
	}

	[Serializable]
	public class UISoundCombatLog
	{
		[field: SerializeField]
		public UISound Open { get; private set; }

		[field: SerializeField]
		public UISound Close { get; private set; }

		[field: SerializeField]
		public UISound FiltersOpen { get; private set; }

		[field: SerializeField]
		public UISound FiltersClose { get; private set; }

		[field: SerializeField]
		public UISound SizeChanged { get; private set; }
	}

	[Serializable]
	public class UISoundInitiativeTracker
	{
		[field: SerializeField]
		public UISound Show { get; private set; }

		[field: SerializeField]
		public UISound Hide { get; private set; }

		[field: SerializeField]
		public UISound RoundCount { get; private set; }
	}

	[Serializable]
	public class UIQuickSlotReplenishSounds
	{
		[field: SerializeField]
		public UISound Success { get; private set; }

		[field: SerializeField]
		public UISound Failure { get; private set; }
	}

	public static CombatSounds Instance => Services.GetInstance<UISounds>().Sounds.CombatSounds;

	[field: SerializeField]
	public UISoundCombat Combat { get; private set; }

	[field: SerializeField]
	public UISoundActionBar ActionBar { get; private set; }

	[field: SerializeField]
	public UISoundCombatLog CombatLog { get; private set; }

	[field: SerializeField]
	public UISoundInitiativeTracker InitiativeTracker { get; private set; }

	[field: SerializeField]
	public UIQuickSlotReplenishSounds QuickSlotsReplenishSounds { get; private set; }
}
