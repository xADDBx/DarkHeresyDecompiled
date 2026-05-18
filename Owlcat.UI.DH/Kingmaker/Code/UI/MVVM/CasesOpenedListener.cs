using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM;

public class CasesOpenedListener : NotificationListenerBase, ICaseStatusChanged, ISubscriber
{
	private readonly List<BlueprintCase> m_CasesOpened = new List<BlueprintCase>();

	public override bool HasData => m_CasesOpened.Any();

	public override int Order => 0;

	public override NotificationCategory Category => NotificationCategory.Detective;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.OpenedCase;

	public override bool HasNewItems => true;

	public CasesOpenedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleCaseStatusChanged(BlueprintCase blueprint)
	{
		if (Game.Instance.DetectiveSystem.GetCaseStatus(blueprint) == CaseStatus.Opened)
		{
			m_CasesOpened.Add(blueprint);
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		IOrderedEnumerable<BlueprintCase> source = m_CasesOpened.OrderBy((BlueprintCase c) => c.AssetGuid);
		if (!source.Any())
		{
			return new List<DialogNotificationVM>();
		}
		return source.Select(delegate(BlueprintCase c)
		{
			string arg = NotificationFormatter.GenerateLink(c.Name.Text, $"{EntityLink.Type.Detective}:{c.AssetGuid}");
			return new DialogNotificationVM(NotificationFormatter.FormatText(string.Format(UINotificationTexts.Instance.CasesOpenedFormat, arg), NotificationType.Positive), c.Icon, new TooltipTemplateDetective(c));
		}).ToList();
	}

	public override void Clear()
	{
		m_CasesOpened.Clear();
	}
}
