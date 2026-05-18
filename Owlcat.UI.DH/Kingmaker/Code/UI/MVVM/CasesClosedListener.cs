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

public class CasesClosedListener : NotificationListenerBase, ICaseStatusChanged, ISubscriber
{
	private readonly List<BlueprintCase> m_CasesClosed = new List<BlueprintCase>();

	public override bool HasData => m_CasesClosed.Any();

	public override int Order => 1;

	public override NotificationCategory Category => NotificationCategory.Detective;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.ClosedCase;

	public CasesClosedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleCaseStatusChanged(BlueprintCase blueprint)
	{
		if (Game.Instance.DetectiveSystem.GetCaseStatus(blueprint) == CaseStatus.Closed)
		{
			m_CasesClosed.Add(blueprint);
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		IOrderedEnumerable<BlueprintCase> source = m_CasesClosed.OrderBy((BlueprintCase c) => c.AssetGuid);
		if (!source.Any())
		{
			return new List<DialogNotificationVM>();
		}
		return source.Select(delegate(BlueprintCase c)
		{
			string arg = NotificationFormatter.GenerateLink(c.Name.Text, $"{EntityLink.Type.Detective}:{c.AssetGuid}");
			return new DialogNotificationVM(NotificationFormatter.FormatText(string.Format(UINotificationTexts.Instance.CasesClosedFormat, arg), NotificationType.Positive), c.Icon, new TooltipTemplateDetective(c));
		}).ToList();
	}

	public override void Clear()
	{
		m_CasesClosed.Clear();
	}
}
