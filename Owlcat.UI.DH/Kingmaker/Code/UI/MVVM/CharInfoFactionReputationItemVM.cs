using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFactionReputationItemVM : ViewModel, IFullScreenUIHandler, ISubscriber
{
	public readonly FactionType FactionType;

	private readonly ReactiveProperty<float> m_CurrentReputation = new ReactiveProperty<float>();

	private readonly ReactiveProperty<int?> m_NextLevelReputation = new ReactiveProperty<int?>();

	private readonly ReactiveProperty<int> m_ReputationLevel = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsMaxLevel = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_Delta = new ReactiveProperty<int>();

	private readonly ReactiveProperty<float> m_Difference = new ReactiveProperty<float>();

	public readonly string Label;

	public readonly string Description;

	public readonly TooltipBaseTemplate Tooltip;

	public readonly List<FactionVendorInformationVM> Vendors = new List<FactionVendorInformationVM>();

	public bool CanTrade;

	public ReadOnlyReactiveProperty<float> CurrentReputation => m_CurrentReputation;

	public ReadOnlyReactiveProperty<int?> NextLevelReputation => m_NextLevelReputation;

	public ReadOnlyReactiveProperty<int> ReputationLevel => m_ReputationLevel;

	public ReadOnlyReactiveProperty<bool> IsMaxLevel => m_IsMaxLevel;

	public ReadOnlyReactiveProperty<int> Delta => m_Delta;

	public ReadOnlyReactiveProperty<float> Difference => m_Difference;

	public CharInfoFactionReputationItemVM(FactionType factionType, bool canTrade = false, List<MechanicEntity> vendors = null)
	{
		EventBus.Subscribe(this).AddTo(this);
		FactionType = factionType;
		CanTrade = canTrade;
		m_CurrentReputation.Value = ReputationHelper.GetCurrentReputationPoints(FactionType);
		m_NextLevelReputation.Value = ReputationHelper.GetNextLevelReputationPoints(FactionType);
		m_ReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(FactionType);
		m_IsMaxLevel.Value = ReputationHelper.IsMaxReputation(FactionType);
		Label = UIStrings.Instance.CharacterSheet.GetFactionLabel(FactionType);
		Description = UIStrings.Instance.CharacterSheet.GetFactionDescription(FactionType);
		Tooltip = new TooltipTemplateSimple(Label, Description);
		AddVendorsInfo(vendors);
	}

	private void AddVendorsInfo(IEnumerable<MechanicEntity> vendors = null)
	{
		Vendors.Clear();
		List<DetectedVendorData> list = ((vendors != null) ? (from vendor in vendors
			where vendor?.GetOptional<PartVendor>()?.Faction?.FactionType == FactionType
			let location = vendor.GetOptional<PartLastDetectedLocation>()
			select new DetectedVendorData(vendor, location?.Area, location?.AreaPart, location?.Chapter ?? 0)).ToList() : Game.Instance.VendorsManager.DetectedVendors.Where((DetectedVendorData x) => x.Faction?.FactionType == FactionType).ToList());
		if (CanTrade)
		{
			foreach (DetectedVendorData item in list)
			{
				Vendors.Add(new FactionVendorInformationVM(item.Area?.Get()?.AreaName, item.VendorName, item.Entity));
			}
			return;
		}
		foreach (DetectedVendorData item2 in list)
		{
			Vendors.Add(new FactionVendorInformationVM(item2.Area?.Get()?.AreaName, item2.VendorName));
		}
	}

	public string GetCurrentAndNextLevelProgress()
	{
		m_IsMaxLevel.Value = ReputationHelper.IsMaxReputation(FactionType);
		m_NextLevelReputation.Value = ReputationHelper.GetNextLevelReputationPoints(FactionType);
		if (!IsMaxLevel.CurrentValue)
		{
			return string.Format(CultureInfo.InvariantCulture, (Mathf.RoundToInt(CurrentReputation.CurrentValue) < 10) ? "{0:0}" : "{0:0,0}", Mathf.RoundToInt(CurrentReputation.CurrentValue)) + " / " + string.Format(CultureInfo.InvariantCulture, "{0:0,0}", NextLevelReputation.CurrentValue);
		}
		return UIStrings.Instance.CharacterSheet.MaxReputationLevel;
	}

	public int GetCurrentReputationPoints()
	{
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(FactionType);
		int reputationPointsByLevel = ReputationHelper.GetReputationPointsByLevel(FactionType, currentReputationLevel);
		int reputationPointsByLevel2 = ReputationHelper.GetReputationPointsByLevel(FactionType, currentReputationLevel + 1);
		m_Delta.Value = reputationPointsByLevel2 - reputationPointsByLevel;
		return Delta.CurrentValue;
	}

	public float GetNextLevelReputationPoints()
	{
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(FactionType);
		int currentReputationPoints = ReputationHelper.GetCurrentReputationPoints(FactionType);
		int reputationPointsByLevel = ReputationHelper.GetReputationPointsByLevel(FactionType, currentReputationLevel);
		m_Difference.Value = currentReputationPoints - reputationPointsByLevel;
		return Difference.CurrentValue;
	}

	public float GetProgressToNextLevel()
	{
		return ReputationHelper.GetProgressToNextLevel(FactionType);
	}

	public float GetProgressPercent()
	{
		int? nextLevelReputationPoints = ReputationHelper.GetNextLevelReputationPoints(FactionType);
		if (nextLevelReputationPoints.HasValue)
		{
			return CurrentReputation.CurrentValue / (float)nextLevelReputationPoints.Value;
		}
		return 0f;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.Vendor && !state)
		{
			m_CurrentReputation.Value = ReputationHelper.GetCurrentReputationPoints(FactionType);
			m_NextLevelReputation.Value = ReputationHelper.GetNextLevelReputationPoints(FactionType);
			m_ReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(FactionType);
			m_IsMaxLevel.Value = ReputationHelper.IsMaxReputation(FactionType);
		}
	}
}
