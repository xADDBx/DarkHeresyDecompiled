using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityAddedListener : NotificationListenerBase, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	private readonly List<EntityFact> m_AbilityAdded = new List<EntityFact>();

	public override bool HasData => m_AbilityAdded.Any();

	public override int Order => 12;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.AbilityAdded;

	public AbilityAddedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (!(fact.Owner is BaseUnitEntity { IsInPlayerParty: not false }))
		{
			return;
		}
		string value = ((fact.Owner is BaseUnitEntity baseUnitEntity2) ? baseUnitEntity2.CharacterName : string.Empty);
		if (!(fact is Ability))
		{
			if (fact is Feature && fact.Blueprint is BlueprintFeature { ShowInDialogue: false })
			{
				return;
			}
		}
		else if (fact.Blueprint is BlueprintAbility { ShowInDialogue: false })
		{
			return;
		}
		if (!string.IsNullOrWhiteSpace(fact.Name) && !string.IsNullOrWhiteSpace(value) && (fact is Ability || fact is Feature))
		{
			m_AbilityAdded.Add(fact);
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		List<EntityFact> list = m_AbilityAdded.ToList();
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
			AbilityAppend(list[i], stringBuilder);
		}
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(NotificationFormatter.FormatText(stringBuilder.ToString()))
		};
	}

	private void AbilityAppend(EntityFact ability, StringBuilder stringBuilder)
	{
		if (ability is Ability || ability is Feature)
		{
			object obj = ability.Name;
			string text = ((!(ability.Owner is BaseUnitEntity baseUnitEntity)) ? string.Empty : baseUnitEntity.CharacterName);
			string arg = text;
			if (obj == null)
			{
				obj = "";
			}
			string arg2 = NotificationFormatter.GenerateLink((string)obj, "f:" + ability.Blueprint.AssetGuid);
			stringBuilder.Append(string.Format(UINotificationTexts.Instance.AbilityAddedFormat, arg2, arg));
		}
	}

	public override void Clear()
	{
		m_AbilityAdded.Clear();
	}
}
