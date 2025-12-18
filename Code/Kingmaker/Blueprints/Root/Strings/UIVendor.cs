using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIVendor
{
	public LocalizedString BeforeClose;

	public LocalizedString Trade;

	public LocalizedString ProceedTransaction;

	public LocalizedString CantBuyItem;

	public LocalizedString NotEnoughReputation;

	[Obsolete]
	public LocalizedString NotEnoughProfitFactor;

	public LocalizedString NotEnoughMoney;

	public LocalizedString ChooseVendorForTrade;

	public LocalizedString Exchange;

	public LocalizedString HideUnrelevant;

	public LocalizedString SelectAllRelevant;

	public LocalizedString UnselectAllRelevant;

	public LocalizedString Vendor;

	public LocalizedString Reputation;

	public LocalizedString Deal;

	public LocalizedString SellAllTrash;

	public LocalizedString ReturnTrash;

	public LocalizedString Discount;

	public LocalizedString NoItemsVendor;

	public LocalizedString ReputationLockHint;
}
