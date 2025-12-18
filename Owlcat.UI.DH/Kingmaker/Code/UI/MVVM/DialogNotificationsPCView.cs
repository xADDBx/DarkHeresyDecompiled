using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Code.View.UI.MVVM.Dialog;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Items;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using WebSocketSharp;

namespace Kingmaker.Code.UI.MVVM;

public class DialogNotificationsPCView : View<DialogNotificationsVM>
{
	private enum NotificationType
	{
		Positive,
		Negative,
		Neutral
	}

	[Header("Clue notifications")]
	[SerializeField]
	private GameObject m_ClueNotificationBlock;

	[SerializeField]
	private DialogNotificationView m_ClueNotificationPrefab;

	[SerializeField]
	private WidgetList m_ClueNotificationWidgetList;

	[SerializeField]
	private TextMeshProUGUI m_NewItemText;

	[SerializeField]
	private TextMeshProUGUI m_HeaderText;

	[Header("Common notifications")]
	[SerializeField]
	private GameObject m_CommonNotificationBlock;

	[SerializeField]
	private DialogNotificationView m_CommonNotificationPrefab;

	[SerializeField]
	private WidgetList m_CommonNotificationWidgetList;

	[Header("Other settings")]
	[SerializeField]
	private DialogNotificationColors m_DialogNotificationColors;

	private readonly ObservableList<DialogNotificationVM> m_CommonNotifications = new ObservableList<DialogNotificationVM>();

	private readonly ObservableList<DialogNotificationVM> m_ClueNotifications = new ObservableList<DialogNotificationVM>();

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
		Clear();
	}

	protected override void OnBind()
	{
		base.ViewModel.HasNotifications.Subscribe(OnUpdateNotifications).AddTo(this);
		m_ClueNotifications.ObserveCountChanged().Subscribe(delegate
		{
			UpdateClueNotificationsView();
		}).AddTo(this);
		m_CommonNotifications.ObserveCountChanged().Subscribe(delegate
		{
			UpdateCommonNotificationsView();
		}).AddTo(this);
		m_NewItemText.text = UIStrings.Instance.Dialog.NewItemLabel;
	}

	public void UpdateClueNotificationsView()
	{
		m_ClueNotificationWidgetList.DrawEntries(m_ClueNotifications, m_ClueNotificationPrefab);
	}

	public void UpdateCommonNotificationsView()
	{
		m_CommonNotificationWidgetList.DrawEntries(m_CommonNotifications, m_CommonNotificationPrefab);
	}

	private void Clear()
	{
		m_ClueNotificationWidgetList.Clear();
		m_ClueNotifications.Clear();
		m_CommonNotificationWidgetList.Clear();
		m_CommonNotifications.Clear();
	}

	private void OnUpdateNotifications(bool show)
	{
		base.gameObject.SetActive(show);
		if (show)
		{
			UISounds.Instance.Sounds.Notifications.NewInformation.Play();
			Clear();
			m_HeaderText.gameObject.SetActive(value: false);
			TryAddToCommonNotification(GetItemReceivedNotification());
			TryAddToCommonNotification(GetItemLostNotification());
			TryAddToCommonNotification(GetCustomNotifications());
			TryAddToCommonNotification(GetDamageDealt());
			TryAddToCommonNotification(GetConvictionShift());
			TryAddToCommonNotification(GetLostFactionReputation());
			TryAddToCommonNotification(GetReceivedFactionReputation());
			TryAddToCommonNotification(GetAbilityAdded());
			TryAddToCommonNotification(GetBuffAdded());
			TryAddToClueNotifications(GetOpenedCases());
			TryAddToClueNotifications(GetClosedCases());
			TryAddToClueNotifications(GetCluesReceived());
			TryAddToClueNotifications(GetAddendumsReceived());
			TryAddToClueNotifications(GetConclusionsConstructed());
			m_ClueNotificationBlock.SetActive(m_ClueNotifications.Any());
			m_CommonNotificationBlock.SetActive(m_CommonNotifications.Any());
		}
	}

	private void TryAddToClueNotifications(List<DialogNotificationVM> notifications)
	{
		if (notifications == null)
		{
			return;
		}
		foreach (DialogNotificationVM notification in notifications)
		{
			if (notification.Label.IsNullOrEmpty())
			{
				break;
			}
			m_ClueNotifications.Add(notification);
		}
	}

	private void TryAddToCommonNotification(DialogNotificationVM notification)
	{
		if (!notification.Label.IsNullOrEmpty())
		{
			m_CommonNotifications.Add(notification);
		}
	}

	private DialogNotificationVM GetItemReceivedNotification()
	{
		List<KeyValuePair<ItemEntity, int>> list = base.ViewModel.ItemsReceived.Where((KeyValuePair<ItemEntity, int> k) => k.Value > 0).ToList();
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				SmartAppend(new KeyValuePair<string, int>(GenerateLink(list[i].Key.Name, "i:" + list[i].Key.UniqueId), list[i].Value), stringBuilder);
			}
			m_HeaderText.text = UIStrings.Instance.Dialog.NewItemReceived;
			m_HeaderText.gameObject.SetActive(value: true);
			string text = string.Format(UINotificationTexts.Instance.ItemsRecievedFormat, stringBuilder);
			return new DialogNotificationVM(FormatTextWithType(text, NotificationType.Positive, showNewItemText: true));
		}
		return new DialogNotificationVM(string.Empty);
	}

	private DialogNotificationVM GetItemLostNotification()
	{
		List<KeyValuePair<ItemEntity, int>> list = base.ViewModel.ItemsLost.Where((KeyValuePair<ItemEntity, int> k) => k.Value > 0).ToList();
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				SmartAppend(new KeyValuePair<string, int>(GenerateLink(list[i].Key.Name, "ib:" + list[i].Key.Blueprint.AssetGuid), list[i].Value), stringBuilder);
			}
			string text = string.Format(UINotificationTexts.Instance.ItemsLostFormat, stringBuilder);
			return new DialogNotificationVM(FormatTextWithType(text, NotificationType.Negative));
		}
		return new DialogNotificationVM(string.Empty);
	}

	private DialogNotificationVM GetDamageDealt()
	{
		List<KeyValuePair<string, int>> list = base.ViewModel.DamageDealt.Where((KeyValuePair<string, int> k) => k.Value > 0).ToList();
		if (list.Count <= 0)
		{
			return new DialogNotificationVM(string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			DamageDealtAppend(list[i], stringBuilder);
		}
		string format = UINotificationTexts.Instance.DamageDealtFormat;
		return new DialogNotificationVM(FormatTextWithType(string.Format(format, stringBuilder), NotificationType.Negative));
	}

	private DialogNotificationVM GetLostFactionReputation()
	{
		List<(FactionType, ReputationType, int)> lostData = base.ViewModel.FactionReputationChanged.GetLostData();
		if (!lostData.Any())
		{
			return new DialogNotificationVM(string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < lostData.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			(FactionType, ReputationType, int) data = lostData[i];
			FactionReputationAppend(data, stringBuilder);
		}
		string text = string.Format(UINotificationTexts.Instance.FactionReputationLostFormat, stringBuilder);
		return new DialogNotificationVM(FormatTextWithType(text, NotificationType.Negative));
	}

	private DialogNotificationVM GetReceivedFactionReputation()
	{
		List<(FactionType, ReputationType, int)> receivedData = base.ViewModel.FactionReputationChanged.GetReceivedData();
		if (!receivedData.Any())
		{
			return new DialogNotificationVM(string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < receivedData.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			(FactionType, ReputationType, int) data = receivedData[i];
			FactionReputationAppend(data, stringBuilder);
		}
		string text = string.Format(UINotificationTexts.Instance.FactionReputationReceivedFormat, stringBuilder);
		return new DialogNotificationVM(FormatTextWithType(text, NotificationType.Positive));
	}

	private DialogNotificationVM GetAbilityAdded()
	{
		List<EntityFact> list = base.ViewModel.AbilityAdded.ToList();
		if (list.Count <= 0)
		{
			return new DialogNotificationVM(string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append("<br>");
			}
			AbilityAppend(list[i], stringBuilder, _: true);
		}
		return new DialogNotificationVM(FormatTextWithType(stringBuilder.ToString()));
	}

	private DialogNotificationVM GetBuffAdded()
	{
		List<EntityFact> list = base.ViewModel.BuffAdded.ToList();
		if (list.Count <= 0)
		{
			return new DialogNotificationVM(string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append("<br>");
			}
			BuffAppend(list[i], stringBuilder, _: true);
		}
		return new DialogNotificationVM(FormatTextWithType(stringBuilder.ToString()));
	}

	private List<DialogNotificationVM> GetOpenedCases()
	{
		IOrderedEnumerable<BlueprintCase> orderedEnumerable = base.ViewModel.CasesOpened.OrderBy((BlueprintCase c) => c.AssetGuid);
		if (!orderedEnumerable.Any())
		{
			return null;
		}
		List<DialogNotificationVM> list = new List<DialogNotificationVM>();
		foreach (BlueprintCase item2 in orderedEnumerable)
		{
			string text = string.Format(UINotificationTexts.Instance.CasesOpenedFormat, GenerateLink(item2.Name.Text, "Detective:" + item2.AssetGuid));
			DialogNotificationVM item = new DialogNotificationVM(FormatTextWithType(text, NotificationType.Positive, showNewItemText: true), item2.Icon, new TooltipTemplateDetective(item2));
			list.Add(item);
		}
		return list;
	}

	private List<DialogNotificationVM> GetClosedCases()
	{
		IOrderedEnumerable<BlueprintCase> orderedEnumerable = base.ViewModel.CasesClosed.OrderBy((BlueprintCase c) => c.AssetGuid);
		if (!orderedEnumerable.Any())
		{
			return null;
		}
		List<DialogNotificationVM> list = new List<DialogNotificationVM>();
		foreach (BlueprintCase item in orderedEnumerable)
		{
			string text = string.Format(UINotificationTexts.Instance.CasesClosedFormat, GenerateLink(item.Name.Text, "Detective:" + item.AssetGuid));
			list.Add(new DialogNotificationVM(FormatTextWithType(text, NotificationType.Positive), item.Icon));
		}
		return list;
	}

	private List<DialogNotificationVM> GetCluesReceived()
	{
		if (!base.ViewModel.CluesReceived.Any())
		{
			return null;
		}
		return (from clue in base.ViewModel.CluesReceived
			group clue by clue.ParentCase.Blueprint.AssetGuid into @group
			select CreateClueNotification(UINotificationTexts.Instance.CluesAddedFormat, @group.ToList())).ToList();
	}

	private DialogNotificationVM CreateClueNotification(string prefix, List<BlueprintClue> clues)
	{
		BlueprintClue blueprintClue = clues.FirstOrDefault();
		if (blueprintClue == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < clues.Count; i++)
		{
			BlueprintClue clue = clues[i];
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			IEnumerable<BlueprintClueAddendum> enumerable2;
			if (blueprintClue.ParentCase.Blueprint.IsUnknown())
			{
				IEnumerable<BlueprintClueAddendum> enumerable = new List<BlueprintClueAddendum>();
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = base.ViewModel.AddendumsReceived.Where((BlueprintClueAddendum a) => a.ParentClue == clue);
			}
			IEnumerable<BlueprintClueAddendum> addendums = enumerable2;
			string additionalInfo = DetectiveInfoEncryption.EncryptAddendums(clue, addendums);
			ClueAppend(clue, stringBuilder, additionalInfo);
		}
		m_HeaderText.text = UIStrings.Instance.Dialog.NewClueReceived;
		m_HeaderText.gameObject.SetActive(value: true);
		string text = string.Format(prefix, stringBuilder);
		text = FormatTextWithType(text, NotificationType.Positive, showNewItemText: true);
		bool num = Game.Instance.DetectiveSystem.GetCaseStatus(blueprintClue.ParentCase) == CaseStatus.None;
		TooltipTemplateDetective iconTooltip = (num ? new TooltipTemplateDetective(null) : new TooltipTemplateDetective(blueprintClue.ParentCase.Blueprint));
		Sprite icon = (num ? UIConfig.Instance.DetectiveConfig.UnknownCluesIcon : blueprintClue.ParentCase.Blueprint.Icon);
		return new DialogNotificationVM(text, icon, iconTooltip);
	}

	private DialogNotificationVM CreateConclusionNotification(List<BlueprintConclusion> conclusions)
	{
		if (!conclusions.Any())
		{
			return null;
		}
		BlueprintCase blueprint = conclusions.First().ParentCase.Blueprint;
		string text = GenerateLink(UIStrings.Instance.Dialog.NewConclusionConstructed, "Detective:" + blueprint.AssetGuid + ":" + DetectiveInfoEncryption.EncryptConclusions(conclusions));
		text = FormatTextWithType(text, NotificationType.Positive, showNewItemText: true);
		bool num = Game.Instance.DetectiveSystem.GetCaseStatus(blueprint) == CaseStatus.None;
		TooltipTemplateDetective iconTooltip = (num ? new TooltipTemplateDetective(null) : new TooltipTemplateDetective(blueprint));
		Sprite icon = (num ? UIConfig.Instance.DetectiveConfig.UnknownCluesIcon : blueprint.Icon);
		return new DialogNotificationVM(text, icon, iconTooltip);
	}

	private List<DialogNotificationVM> GetAddendumsReceived()
	{
		List<DialogNotificationVM> list = (from clue in (from a in base.ViewModel.AddendumsReceived
				where !base.ViewModel.CluesReceived.Contains(a.ParentClue)
				select a.ParentClue.Blueprint).Distinct()
			group clue by clue.ParentCase.Blueprint.AssetGuid into g
			select CreateClueNotification(UINotificationTexts.Instance.AddendumsAddedFormat, g.ToList())).ToList();
		if (list.Count > 0)
		{
			m_HeaderText.text = UIStrings.Instance.Dialog.NewAddendumReceived;
			m_HeaderText.gameObject.SetActive(value: true);
		}
		return list;
	}

	private List<DialogNotificationVM> GetConclusionsConstructed()
	{
		return (from clue in base.ViewModel.ConclusionsConstructed
			group clue by clue.ParentCase.Blueprint.AssetGuid into g
			select CreateConclusionNotification(g.ToList())).ToList();
	}

	private DialogNotificationVM GetConvictionShift()
	{
		if (base.ViewModel.AlignmentShifts.Count <= 0)
		{
			return new DialogNotificationVM(string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < base.ViewModel.AlignmentShifts.Count; i++)
		{
			var (alignmentAxis2, num2) = (KeyValuePair<AlignmentAxis, int>)(ref base.ViewModel.AlignmentShifts.ElementAt(i));
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			string arg = GenerateLink(UIUtilityText.GetSoulMarkDirectionText(alignmentAxis2).Text, $"SoulMarkShiftDirection:{alignmentAxis2}");
			stringBuilder.Append(string.Format(UINotificationTexts.Instance.SoulMarksShiftFormat, arg, num2));
		}
		return new DialogNotificationVM(FormatTextWithType(stringBuilder.ToString()));
	}

	private DialogNotificationVM GetCustomNotifications()
	{
		if (base.ViewModel.CustomNotifications.Count == 0)
		{
			return new DialogNotificationVM(string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string customNotification in base.ViewModel.CustomNotifications)
		{
			stringBuilder.Append(customNotification);
			stringBuilder.Append("\n");
		}
		return new DialogNotificationVM(FormatTextWithType(stringBuilder.ToString()));
	}

	private void FactionReputationAppend((FactionType, ReputationType, int) data, StringBuilder stringBuilder)
	{
		(FactionType, ReputationType, int) tuple = data;
		FactionType item = tuple.Item1;
		ReputationType item2 = tuple.Item2;
		int item3 = tuple.Item3;
		string text = GenerateLink($"{UtilityFaction.GetSpriteLabel(item2)}{Math.Abs(item3)}", "Encyclopedia:" + UIUtilityFaction.GetEncyclopediaName(item2), null);
		string text2 = GenerateLink(UIStrings.Instance.CharacterSheet.GetFactionLabel(item) ?? "", "Encyclopedia:" + UIUtilityFaction.GetEncyclopediaName(item), null);
		stringBuilder.Append(text2 + " : " + text);
	}

	private static void SmartAppend(KeyValuePair<string, int> pair, StringBuilder stringBuilder)
	{
		KeyValuePair<string, int> keyValuePair = pair;
		var (text2, num2) = (KeyValuePair<string, int>)(ref keyValuePair);
		stringBuilder.Append((num2 > 1) ? $"{text2} x{num2}" : text2);
	}

	private void DamageDealtAppend(KeyValuePair<string, int> pair, StringBuilder stringBuilder)
	{
		KeyValuePair<string, int> keyValuePair = pair;
		keyValuePair.Deconstruct(out var key, out var value);
		string text = key;
		int value2 = value;
		string text2 = GenerateLink($"{Math.Abs(value2)}", "Encyclopedia:Damage");
		stringBuilder.Append(text + " (" + text2 + ")");
	}

	private void AbilityAppend(EntityFact ability, StringBuilder stringBuilder, bool _)
	{
		if (ability is Ability || ability is Feature)
		{
			string text = ability.Name;
			string text2 = ((!(ability.Owner is BaseUnitEntity baseUnitEntity)) ? string.Empty : baseUnitEntity.CharacterName);
			string arg = text2;
			string arg2 = GenerateLink(text ?? "", "f:" + ability.Blueprint.AssetGuid);
			stringBuilder.Append(string.Format(UINotificationTexts.Instance.AbilityAddedFormat, arg2, arg));
		}
	}

	private void ClueAppend(BlueprintClue clue, StringBuilder stringBuilder, string additionalInfo = null)
	{
		string text = "<b><color=#" + m_DialogNotificationColors.LinkColor.HTML() + ">" + clue.GetUIData().Name.Text + "</color></b>";
		string value = GenerateLink(text, "Detective:" + clue.AssetGuid + ":" + additionalInfo);
		stringBuilder.Append(value);
	}

	private void BuffAppend(EntityFact buff, StringBuilder stringBuilder, bool _)
	{
		if (buff is Buff buff2)
		{
			string text = buff.Name;
			string arg = (buff.Owner as BaseUnitEntity)?.CharacterName;
			string arg2;
			if (buff2.IsPermanent)
			{
				arg2 = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.CharacterSheet.Permanent.Text;
			}
			else
			{
				string arg3 = ((buff2.ExpirationInRounds == 1) ? ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round.Text : ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds.Text);
				arg2 = $"{buff2.ExpirationInRounds} {arg3}";
			}
			string arg4 = GenerateLink(text ?? "", "f:" + buff.Blueprint.AssetGuid);
			stringBuilder.Append(string.Format(UINotificationTexts.Instance.BuffAddedFormat, arg4, arg, arg2));
		}
	}

	private string GenerateLink(string text, string link)
	{
		return GenerateLink(text, link, m_DialogNotificationColors.LinkColor);
	}

	private string GenerateLink(string text, string link, Color? color)
	{
		string text2 = "<b><link=\"" + link + "\">" + text + "</link></b>";
		if (color.HasValue)
		{
			text2 = "<color=#" + color.Value.HTML() + ">" + text2 + "</color>";
		}
		return text2;
	}

	private string FormatTextWithType(string text, NotificationType type = NotificationType.Neutral, bool showNewItemText = false)
	{
		if (text.IsNullOrEmpty())
		{
			return string.Empty;
		}
		string text2 = type switch
		{
			NotificationType.Positive => m_DialogNotificationColors.NotificationPositive.HTML(), 
			NotificationType.Negative => m_DialogNotificationColors.NotificationNegative.HTML(), 
			NotificationType.Neutral => m_DialogNotificationColors.NotificationNeutral.HTML(), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
		m_NewItemText.gameObject.SetActive(showNewItemText);
		return "<color=#" + text2 + ">" + text + "</color>";
	}
}
