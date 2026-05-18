using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ConclusionsConstructedListener : NotificationListenerBase, IConclusionStatusChanged, ISubscriber
{
	private readonly List<BlueprintConclusion> m_ConclusionsConstructed = new List<BlueprintConclusion>();

	public override bool HasData => m_ConclusionsConstructed.Any();

	public override int Order => 4;

	public override NotificationCategory Category => NotificationCategory.Detective;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.ConclusionConstructed;

	public override bool HasNewItems => true;

	public ConclusionsConstructedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleConclusionStatusChanged(BlueprintConclusion blueprint)
	{
		if (!blueprint.ParentCase.Blueprint.IsClosed())
		{
			m_ConclusionsConstructed.Add(blueprint);
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		IEnumerable<IGrouping<string, BlueprintConclusion>> source = from clue in m_ConclusionsConstructed
			group clue by clue.ParentCase.Blueprint.AssetGuid;
		if (!source.Any())
		{
			return new List<DialogNotificationVM>();
		}
		return source.Select((IGrouping<string, BlueprintConclusion> c) => CreateConclusionNotification(c.ToList())).ToList();
	}

	private DialogNotificationVM CreateConclusionNotification(List<BlueprintConclusion> conclusions)
	{
		if (!conclusions.Any())
		{
			return DialogNotificationVM.Empty;
		}
		BlueprintCase blueprint = conclusions.First().ParentCase.Blueprint;
		string label = NotificationFormatter.FormatText(NotificationFormatter.GenerateLink(UIStrings.Instance.Dialog.NewConclusionConstructed, $"{EntityLink.Type.Detective}:{blueprint.AssetGuid}:{DetectiveInfoEncryption.EncryptConclusions(conclusions)}"), NotificationType.Positive);
		bool num = Game.Instance.DetectiveSystem.GetCaseStatus(blueprint) == CaseStatus.None;
		TooltipTemplateDetective iconTooltip = (num ? new TooltipTemplateDetective(null) : new TooltipTemplateDetective(blueprint));
		Sprite icon = (num ? UIConfig.Instance.DetectiveConfig.UnknownCluesIcon : blueprint.Icon);
		return new DialogNotificationVM(label, icon, iconTooltip);
	}

	public override void Clear()
	{
		m_ConclusionsConstructed.Clear();
	}
}
