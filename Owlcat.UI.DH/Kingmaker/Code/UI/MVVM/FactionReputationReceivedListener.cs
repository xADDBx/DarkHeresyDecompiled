using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM;

public class FactionReputationReceivedListener : NotificationListenerBase, IGainFactionReputationHandler, ISubscriber
{
	private readonly List<FactionData> m_Factions = new List<FactionData>();

	public override bool HasData => m_Factions.Any((FactionData f) => f.Count > 0);

	public override int Order => 11;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.LostFactionReputation;

	public FactionReputationReceivedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleGainFactionReputation(FactionType factionType, ReputationType reputationType, int count)
	{
		FactionData factionData = m_Factions.FirstOrDefault((FactionData f) => f.FactionType == factionType && f.ReputationType == reputationType);
		if (factionData != null)
		{
			factionData.AddCount(count);
		}
		else
		{
			m_Factions.Add(new FactionData(factionType, reputationType, count));
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		List<(FactionType, ReputationType, int)> list = (from f in m_Factions
			where f.Count > 0
			select (FactionType: f.FactionType, ReputationType: f.ReputationType, Count: f.Count)).ToList();
		if (!list.Any())
		{
			return new List<DialogNotificationVM>();
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			(FactionType, ReputationType, int) data = list[i];
			FactionReputationAppend(data, stringBuilder);
		}
		string text = string.Format(UINotificationTexts.Instance.FactionReputationReceivedFormat, stringBuilder);
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(NotificationFormatter.FormatText(text, NotificationType.Positive))
		};
	}

	private void FactionReputationAppend((FactionType, ReputationType, int) data, StringBuilder stringBuilder)
	{
		(FactionType, ReputationType, int) tuple = data;
		FactionType item = tuple.Item1;
		ReputationType item2 = tuple.Item2;
		int item3 = tuple.Item3;
		string text = NotificationFormatter.GenerateLink($"{UtilityFaction.GetSpriteLabel(item2)}{Math.Abs(item3)}", $"{EntityLink.Type.Encyclopedia}:{UIUtilityFaction.GetEncyclopediaName(item2)}", null);
		string text2 = NotificationFormatter.GenerateLink(UIStrings.Instance.CharacterSheet.GetFactionLabel(item) ?? "", $"{EntityLink.Type.Encyclopedia}:{UIUtilityFaction.GetEncyclopediaName(item)}", null);
		stringBuilder.Append(text2 + " : " + text);
	}

	public override void Clear()
	{
		m_Factions.Clear();
	}
}
