using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM;

public class DamageDealtListener : NotificationListenerBase, IDamageHandler, ISubscriber
{
	private readonly Dictionary<string, int> m_DamageDealt = new Dictionary<string, int>();

	public override bool HasData => m_DamageDealt.Any((KeyValuePair<string, int> k) => k.Value > 0);

	public override int Order => 8;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.DamageDealt;

	public DamageDealtListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (m_DamageDealt.TryGetValue(dealDamage.ConcreteTarget.Name ?? string.Empty, out var _))
		{
			m_DamageDealt[dealDamage.ConcreteTarget.Name ?? string.Empty] += dealDamage.ResultValue;
		}
		else
		{
			m_DamageDealt.Add(dealDamage.ConcreteTarget.Name ?? string.Empty, dealDamage.ResultValue);
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		List<KeyValuePair<string, int>> list = m_DamageDealt.Where((KeyValuePair<string, int> k) => k.Value > 0).ToList();
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
			DamageDealtAppend(list[i], stringBuilder);
		}
		string label = NotificationFormatter.FormatText(string.Format(UINotificationTexts.Instance.DamageDealtFormat, stringBuilder), NotificationType.Negative);
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(label)
		};
	}

	private void DamageDealtAppend(KeyValuePair<string, int> pair, StringBuilder stringBuilder)
	{
		KeyValuePair<string, int> keyValuePair = pair;
		keyValuePair.Deconstruct(out var key, out var value);
		string text = key;
		int value2 = value;
		string text2 = NotificationFormatter.GenerateLink($"{Math.Abs(value2)}", $"{EntityLink.Type.Encyclopedia}:Damage");
		stringBuilder.Append(text + " (" + text2 + ")");
	}

	public override void Clear()
	{
		m_DamageDealt.Clear();
	}
}
