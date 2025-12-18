using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UINotificationTexts
{
	public LocalizedString ItemsLostFormat;

	public LocalizedString ItemsRecievedFormat;

	public LocalizedString XPGainedFormat;

	public LocalizedString DamageDealtFormat;

	public LocalizedString NavigatorResourceAddedFormat;

	public LocalizedString NavigatorResourceLostFormat;

	public LocalizedString SoulMarksShiftFormat;

	public LocalizedString GainedProfitFactor;

	public LocalizedString LostProfitFactor;

	public LocalizedString FactionReputationLostFormat;

	public LocalizedString FactionReputationReceivedFormat;

	public LocalizedString FactionVendorDiscountLostFormat;

	public LocalizedString FactionVendorDiscountReceivedFormat;

	public LocalizedString AbilityAddedFormat;

	public LocalizedString BuffAddedFormat;

	public LocalizedString CasesOpenedFormat;

	public LocalizedString CasesClosedFormat;

	public LocalizedString CluesAddedFormat;

	public LocalizedString AddendumsAddedFormat;

	public LocalizedString ConclusionConstructedFormat;

	public LocalizedString AttackOfOpportunityTrigger;

	[Header("Mock")]
	public LocalizedString ReputationTitle;

	public LocalizedString ReputationChanged;

	public LocalizedString ReputationDescription;

	public static UINotificationTexts Instance => UIStrings.Instance.NotificationTexts;
}
