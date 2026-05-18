using System;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class ServiceWindowsSounds
{
	[Serializable]
	public class UISoundInventory : ServiceWindowUISound
	{
		[field: SerializeField]
		public UISound ErrorEquip { get; private set; }

		[field: SerializeField]
		public UISound SlotsShow { get; private set; }

		[field: SerializeField]
		public UISound VisualSettingsShow { get; private set; }

		[field: SerializeField]
		public UISound VisualSettingsHide { get; private set; }

		[field: SerializeField]
		public UISound RotateDollStart { get; private set; }

		[field: SerializeField]
		public UISound RotateDollLoopStart { get; private set; }

		[field: SerializeField]
		public UISound RotateDollLoopStop { get; private set; }

		[field: SerializeField]
		public UISound RotateDollStop { get; private set; }

		[field: SerializeField]
		public UISound ResetDollPosition { get; private set; }
	}

	[Serializable]
	public class UISoundCharacter : ServiceWindowUISound
	{
		[field: SerializeField]
		public UISound NewLevelNotification { get; private set; }

		[field: SerializeField]
		public UISound LevelUpgradedNotification { get; private set; }

		[field: SerializeField]
		public UISound Select { get; private set; }

		[field: SerializeField]
		public UISound SelectAll { get; private set; }

		[field: SerializeField]
		public UISound StatsShow { get; private set; }

		[field: SerializeField]
		public UISound StatsHide { get; private set; }

		[field: SerializeField]
		public UISound InfoShow { get; private set; }

		[field: SerializeField]
		public UISound InfoHide { get; private set; }

		[field: SerializeField]
		public UISound DollAnimationShow { get; private set; }

		[field: SerializeField]
		public UISound PaperTabChanged { get; private set; }
	}

	[Serializable]
	public class UISoundJournal : ServiceWindowUISound
	{
	}

	[Serializable]
	public class UISoundLocalMap : ServiceWindowUISound
	{
		[field: SerializeField]
		public UISound ShowHideLocalMapLegend { get; private set; }
	}

	[Serializable]
	public class UISoundEncyclopedia : ServiceWindowUISound
	{
	}

	[Serializable]
	public class UISoundDetectiveJournal : ServiceWindowUISound
	{
		[field: SerializeField]
		public UISound SignalDevice { get; set; }

		[field: SerializeField]
		public UISound SignalDeviceShow { get; set; }

		[field: SerializeField]
		public UISound SignalDeviceHide { get; set; }

		[field: SerializeField]
		public UISound ConclusionSelectionSelectConclusion { get; set; }

		[field: SerializeField]
		public UISound ConclusionSelectionApplyConclusion { get; set; }

		[field: SerializeField]
		public UISound SetLineTargetDot { get; set; }

		[field: SerializeField]
		public UISound ClueResearch { get; set; }

		[field: SerializeField]
		public UISound ClueResearchComplete { get; set; }

		[field: SerializeField]
		public UISound CaseItemDragStart { get; set; }

		[field: SerializeField]
		public UISound CaseItemDragStop { get; set; }

		[field: SerializeField]
		public UISound RemoveConclusion { get; set; }

		[field: SerializeField]
		public UISound StampStart { get; set; }

		[field: SerializeField]
		public UISound StampLanded { get; set; }
	}

	[Serializable]
	public class UISoundFactions : ServiceWindowUISound
	{
	}

	public static ServiceWindowsSounds Instance => Services.GetInstance<UISounds>().Sounds.ServiceWindowsSounds;

	[field: SerializeField]
	public UISoundInventory Inventory { get; private set; }

	[field: SerializeField]
	public UISoundCharacter Character { get; private set; }

	[field: SerializeField]
	public UISoundJournal Journal { get; private set; }

	[field: SerializeField]
	public UISoundLocalMap LocalMap { get; private set; }

	[field: SerializeField]
	public UISoundEncyclopedia Encyclopedia { get; private set; }

	[field: SerializeField]
	public UISoundDetectiveJournal DetectiveJournal { get; private set; }

	[field: SerializeField]
	public UISoundFactions Factions { get; private set; }

	public void PlayOpenSound(ServiceWindowsType type)
	{
		GetServiceWindow(type)?.Open.Play();
	}

	public void PlayCloseSound(ServiceWindowsType type)
	{
		GetServiceWindow(type)?.Close.Play();
	}

	public void PlaySwitchSound(ServiceWindowsType type)
	{
		GetServiceWindow(type)?.SwitchTo.Play();
	}

	private ServiceWindowUISound GetServiceWindow(ServiceWindowsType type)
	{
		return type switch
		{
			ServiceWindowsType.None => null, 
			ServiceWindowsType.Inventory => Inventory, 
			ServiceWindowsType.CharacterInfo => Character, 
			ServiceWindowsType.Journal => Journal, 
			ServiceWindowsType.LocalMap => LocalMap, 
			ServiceWindowsType.Encyclopedia => Encyclopedia, 
			ServiceWindowsType.DetectiveJournal => DetectiveJournal, 
			ServiceWindowsType.Reputation => Factions, 
			_ => null, 
		};
	}
}
