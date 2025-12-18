using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationPartVM : ViewModel, IVendorAddToSellCargoHandler, ISubscriber, IGainFactionReputationHandler
{
	private readonly ReactiveProperty<int> m_VendorReputationLevel = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int?> m_VendorReputationProgressToNextLevel = new ReactiveProperty<int?>();

	private readonly ReactiveProperty<float> m_VendorCurrentReputationProgress = new ReactiveProperty<float>();

	public readonly VendorReputationForItemWindowVM VendorReputationForItemWindow;

	public readonly LensSelectorVM Selector;

	private readonly ReactiveProperty<int?> m_ReputationPoints = new ReactiveProperty<int?>(0);

	private readonly ReactiveProperty<int?> m_NextLevelReputationPoints = new ReactiveProperty<int?>(0);

	private readonly ReactiveProperty<int> m_Delta = new ReactiveProperty<int>();

	private readonly ReactiveProperty<float> m_Difference = new ReactiveProperty<float>();

	private readonly ReactiveProperty<int> m_ExchangeValue = new ReactiveProperty<int>();

	public readonly List<ItemsItemOrigin> AcceptItems = new List<ItemsItemOrigin>();

	private readonly ReactiveProperty<bool> m_CanSellCargo = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<List<ContextMenuCollectionEntity>> m_ContextMenu = new ReactiveProperty<List<ContextMenuCollectionEntity>>();

	private readonly ReactiveProperty<bool> m_IsMaxLevel = new ReactiveProperty<bool>();

	public readonly bool NeedHidePfAndReputation;

	private readonly ReactiveProperty<bool> m_HasItemsToSell = new ReactiveProperty<bool>();

	public readonly string VendorFractionName;

	public ReadOnlyReactiveProperty<int> VendorReputationLevel => m_VendorReputationLevel;

	public ReadOnlyReactiveProperty<int?> VendorReputationProgressToNextLevel => m_VendorReputationProgressToNextLevel;

	public ReadOnlyReactiveProperty<float> VendorCurrentReputationProgress => m_VendorCurrentReputationProgress;

	public ReadOnlyReactiveProperty<int?> ReputationPoints => m_ReputationPoints;

	public ReadOnlyReactiveProperty<int?> NextLevelReputationPoints => m_NextLevelReputationPoints;

	public ReadOnlyReactiveProperty<int> Delta => m_Delta;

	public ReadOnlyReactiveProperty<float> Difference => m_Difference;

	public ReadOnlyReactiveProperty<int> ExchangeValue => m_ExchangeValue;

	public ReadOnlyReactiveProperty<bool> CanSellCargo => m_CanSellCargo;

	public ReadOnlyReactiveProperty<List<ContextMenuCollectionEntity>> ContextMenu => m_ContextMenu;

	public ReadOnlyReactiveProperty<bool> IsMaxLevel => m_IsMaxLevel;

	public ReadOnlyReactiveProperty<bool> HasItemsToSell => m_HasItemsToSell;

	private TradeLogic Vendor => VendorHelper.TradeLogic;

	private BlueprintVendorFaction VendorFaction => Game.Instance.TradeLogic.VendorFaction;

	public VendorReputationPartVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		NeedHidePfAndReputation = Vendor.NeedHideCostAndReputation;
		m_VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(VendorFaction.FactionType);
		m_VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(VendorFaction.FactionType);
		m_VendorCurrentReputationProgress.Value = ReputationHelper.GetCurrentReputationPoints(VendorFaction.FactionType);
		m_ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.CurrentValue);
		m_NextLevelReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.CurrentValue + 1);
		m_Delta.Value = (NextLevelReputationPoints.CurrentValue - ReputationPoints.CurrentValue).Value;
		m_Difference.Value = (VendorCurrentReputationProgress.CurrentValue - (float?)ReputationPoints.CurrentValue).Value;
		VendorFractionName = VendorFaction.DisplayName.Text;
		VendorReputationForItemWindow = new VendorReputationForItemWindowVM(AcceptItems).AddTo(this);
		Selector = new LensSelectorVM().AddTo(this);
		m_IsMaxLevel.Value = ReputationHelper.IsMaxReputation(VendorFaction.FactionType);
		CheckItemsToSell();
	}

	protected override void OnDispose()
	{
		UnselectAll();
	}

	public void SellCargo()
	{
	}

	public void HandleSellChange()
	{
	}

	public void ChangeRepValue()
	{
	}

	public void HideUnrelevant()
	{
		UnselectAll();
	}

	public void CheckItemsToSell()
	{
	}

	public void SelectAll()
	{
	}

	public void UnselectAll()
	{
	}

	public void HandleGainFactionReputation(FactionType factionType, ReputationType reputationType, int count)
	{
		int currentReputationPoints = ReputationHelper.GetCurrentReputationPoints(VendorFaction.FactionType);
		if (!IsMaxLevel.CurrentValue)
		{
			m_VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(VendorFaction.FactionType);
			m_VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(VendorFaction.FactionType);
			m_ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.CurrentValue);
			m_NextLevelReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.CurrentValue + 1);
			m_Delta.Value = (NextLevelReputationPoints.CurrentValue - ReputationPoints.CurrentValue).Value;
			m_Difference.Value = (currentReputationPoints - ReputationPoints.CurrentValue).Value;
		}
		m_VendorCurrentReputationProgress.Value = currentReputationPoints;
		m_IsMaxLevel.Value = ReputationHelper.IsMaxReputation(VendorFaction.FactionType);
	}

	public void SetContextMenu(List<ContextMenuCollectionEntity> entities)
	{
		m_ContextMenu.Value = entities;
	}
}
