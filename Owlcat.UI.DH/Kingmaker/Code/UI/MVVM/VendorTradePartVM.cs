using System.Collections.Generic;
using System.Linq;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTradePartVM : ViewModel, IVendorDealHandler, ISubscriber, IVendorTransferHandler
{
	private readonly ReactiveProperty<string> m_VendorName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<FactionType> m_VendorFaction = new ReactiveProperty<FactionType>();

	private readonly ReactiveProperty<Sprite> m_VendorSprite = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<int> m_VendorReputationLevel = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int?> m_VendorReputationProgressToNextLevel = new ReactiveProperty<int?>();

	private readonly ReactiveProperty<float> m_VendorCurrentReputationProgress = new ReactiveProperty<float>();

	private readonly ReactiveProperty<bool> m_IsPossibleDeal = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<float> m_DealPrice = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<int?> m_ReputationPoints = new ReactiveProperty<int?>(0);

	private readonly ReactiveProperty<int?> m_NextLevelReputationPoints = new ReactiveProperty<int?>(0);

	private readonly ReactiveProperty<bool> m_VendorHasItemsToSell = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_Delta = new ReactiveProperty<int>();

	private readonly ReactiveProperty<float> m_Difference = new ReactiveProperty<float>();

	private readonly ReactiveProperty<bool> m_IsMaxLevel = new ReactiveProperty<bool>();

	public readonly string VendorFactionName;

	public readonly PartyVM PartyVM;

	private readonly ReactiveProperty<VendorTransitionWindowVM> m_TransitionWindowVM = new ReactiveProperty<VendorTransitionWindowVM>();

	public List<VendorLevelItemsVM> EnableSlots = new List<VendorLevelItemsVM>();

	private readonly ReactiveCommand<Unit> m_OnSlotsUpdate = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<string> VendorName => m_VendorName;

	public ReadOnlyReactiveProperty<FactionType> VendorFaction => m_VendorFaction;

	public ReadOnlyReactiveProperty<Sprite> VendorSprite => m_VendorSprite;

	public ReadOnlyReactiveProperty<int> VendorReputationLevel => m_VendorReputationLevel;

	public ReadOnlyReactiveProperty<int?> VendorReputationProgressToNextLevel => m_VendorReputationProgressToNextLevel;

	public ReadOnlyReactiveProperty<float> VendorCurrentReputationProgress => m_VendorCurrentReputationProgress;

	public ReadOnlyReactiveProperty<bool> IsPossibleDeal => m_IsPossibleDeal;

	public ReadOnlyReactiveProperty<float> DealPrice => m_DealPrice;

	public ReadOnlyReactiveProperty<int?> ReputationPoints => m_ReputationPoints;

	public ReadOnlyReactiveProperty<int?> NextLevelReputationPoints => m_NextLevelReputationPoints;

	public ReadOnlyReactiveProperty<bool> VendorHasItemsToSell => m_VendorHasItemsToSell;

	public ReadOnlyReactiveProperty<int> Delta => m_Delta;

	public ReadOnlyReactiveProperty<float> Difference => m_Difference;

	public ReadOnlyReactiveProperty<bool> IsMaxLevel => m_IsMaxLevel;

	public ReadOnlyReactiveProperty<VendorTransitionWindowVM> TransitionWindowVM => m_TransitionWindowVM;

	public Observable<Unit> OnSlotsUpdate => m_OnSlotsUpdate;

	private TradeLogic Vendor => VendorHelper.TradeLogic;

	public bool HasDiscount => false;

	public int DiscountValue => 0;

	public VendorTradePartVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		SetupSlots();
		PartyVM = new PartyVM().AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			OnUpdateHandler();
		}).AddTo(this);
		VendorFactionName = Game.Instance.TradeLogic.VendorFaction.DisplayName.Text;
		m_VendorFaction.Value = Game.Instance.TradeLogic.VendorFaction.FactionType;
		ReactiveProperty<string> vendorName = m_VendorName;
		object obj = Game.Instance.TradeLogic.VendorName ?? string.Empty;
		if (obj == null)
		{
			obj = "";
		}
		vendorName.Value = (string)obj;
		m_VendorSprite.Value = Game.Instance.TradeLogic.VendorPortrait?.SmallPortrait;
		m_VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(Game.Instance.TradeLogic.VendorFaction.FactionType);
		m_VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(Game.Instance.TradeLogic.VendorFaction.FactionType);
		m_VendorCurrentReputationProgress.Value = ReputationHelper.GetCurrentReputationPoints(Game.Instance.TradeLogic.VendorFaction.FactionType);
		m_ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(Game.Instance.TradeLogic.VendorFaction.FactionType, VendorReputationLevel.CurrentValue);
		m_NextLevelReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(Game.Instance.TradeLogic.VendorFaction.FactionType, VendorReputationLevel.CurrentValue + 1);
		m_Delta.Value = (NextLevelReputationPoints.CurrentValue - ReputationPoints.CurrentValue).Value;
		m_Difference.Value = (VendorCurrentReputationProgress.CurrentValue - (float?)ReputationPoints.CurrentValue).Value;
		m_IsMaxLevel.Value = ReputationHelper.IsMaxReputation(VendorFaction.CurrentValue);
	}

	protected override void OnDispose()
	{
		EnableSlots.ForEach(delegate(VendorLevelItemsVM s)
		{
			s.Dispose();
		});
		EnableSlots.Clear();
	}

	private void SetupSlots()
	{
		EnableSlots.ForEach(delegate(VendorLevelItemsVM s)
		{
			s.Dispose();
		});
		EnableSlots.Clear();
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(Game.Instance.TradeLogic.VendorFactionType);
		List<ItemEntity> list = Vendor.StoreItems.OrderBy((ItemEntity item) => Game.Instance.TradeLogic.VendorInventory.GetReputationToUnlock(item)).ToList();
		int num = -2;
		int num2 = 0;
		VendorLevelItemsVM vendorLevelItemsVM = null;
		ItemEntity itemEntity = list.LastOrDefault();
		foreach (ItemEntity item in list)
		{
			if (item != null)
			{
				int reputationToUnlock = Game.Instance.TradeLogic.VendorInventory.GetReputationToUnlock(item);
				int reputationLevelByPoints = ReputationHelper.GetReputationLevelByPoints(Game.Instance.TradeLogic.VendorFactionType, reputationToUnlock);
				if (reputationLevelByPoints > num)
				{
					vendorLevelItemsVM = new VendorLevelItemsVM(reputationLevelByPoints, reputationLevelByPoints > currentReputationLevel, item == itemEntity);
					num = reputationLevelByPoints;
					EnableSlots.Add(vendorLevelItemsVM);
				}
				vendorLevelItemsVM?.AddItem(item, num2);
				num2++;
			}
		}
		m_VendorHasItemsToSell.Value = EnableSlots.Count > 0;
		m_OnSlotsUpdate?.Execute();
	}

	private void OnUpdateHandler()
	{
		m_IsPossibleDeal.Value = Vendor.IsDealPossible;
		m_DealPrice.Value = Vendor.DealPrice;
		if (!IsMaxLevel.CurrentValue)
		{
			m_VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(Game.Instance.TradeLogic.VendorFaction.FactionType);
			m_VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(Game.Instance.TradeLogic.VendorFaction.FactionType);
			m_NextLevelReputationPoints.Value = ReputationHelper.GetNextLevelReputationPoints(Game.Instance.TradeLogic.VendorFaction.FactionType);
			m_VendorCurrentReputationProgress.Value = ReputationHelper.GetCurrentReputationPoints(Game.Instance.TradeLogic.VendorFaction.FactionType);
			m_ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(Game.Instance.TradeLogic.VendorFaction.FactionType, VendorReputationLevel.CurrentValue);
			m_Delta.Value = (NextLevelReputationPoints.CurrentValue - ReputationPoints.CurrentValue).Value;
			m_Difference.Value = (VendorCurrentReputationProgress.CurrentValue - (float?)ReputationPoints.CurrentValue).Value;
		}
	}

	public void HandleTransitionWindow(ItemEntity itemEntity = null)
	{
		CloseTransitionWindow();
		m_TransitionWindowVM.Value = new VendorTransitionWindowVM(Vendor, itemEntity, CloseTransitionWindow);
	}

	private void CloseTransitionWindow()
	{
		TransitionWindowVM.CurrentValue?.Dispose();
		m_TransitionWindowVM.Value = null;
	}

	void IVendorDealHandler.HandleVendorDeal()
	{
		SetupSlots();
	}

	void IVendorDealHandler.HandleCancelVendorDeal()
	{
	}
}
