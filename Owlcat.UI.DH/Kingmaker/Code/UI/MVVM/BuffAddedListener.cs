using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.Code.UI.MVVM;

public class BuffAddedListener : NotificationListenerBase, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	private readonly List<EntityFact> m_BuffAdded = new List<EntityFact>();

	public override bool HasData => m_BuffAdded.Any();

	public override int Order => 13;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.BuffAdded;

	public BuffAddedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact.Owner is BaseUnitEntity { IsInPlayerParty: not false })
		{
			string value = ((fact.Owner is BaseUnitEntity baseUnitEntity2) ? baseUnitEntity2.CharacterName : string.Empty);
			if ((!(fact is Buff) || !(fact.Blueprint is BlueprintBuff blueprintBuff) || (!blueprintBuff.IsHiddenInUI && blueprintBuff.ShowInDialogue)) && !string.IsNullOrWhiteSpace(fact.Name) && !string.IsNullOrWhiteSpace(value) && fact is Buff)
			{
				m_BuffAdded.Add(fact);
			}
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		List<EntityFact> list = m_BuffAdded.ToList();
		if (list.Count == 0)
		{
			return new List<DialogNotificationVM>();
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append("<br>");
			}
			BuffAppend(list[i], stringBuilder);
		}
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(NotificationFormatter.FormatText(stringBuilder.ToString()))
		};
	}

	private void BuffAppend(EntityFact buff, StringBuilder stringBuilder)
	{
		if (buff is Buff buff2)
		{
			object obj = buff.Name;
			string arg = (buff.Owner as BaseUnitEntity)?.CharacterName;
			string arg3 = ((!buff2.IsPermanent) ? string.Format(arg1: (buff2.ExpirationInRounds == 1) ? ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round.Text : ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds.Text, format: "{0} {1}", arg0: buff2.ExpirationInRounds) : ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.CharacterSheet.Permanent.Text);
			if (obj == null)
			{
				obj = "";
			}
			string arg4 = NotificationFormatter.GenerateLink((string)obj, "f:" + buff.Blueprint.AssetGuid);
			stringBuilder.Append(string.Format(UINotificationTexts.Instance.BuffAddedFormat, arg4, arg, arg3));
		}
	}

	public override void Clear()
	{
		m_BuffAdded.Clear();
	}
}
