using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CluesReceivedListener : NotificationListenerBase, IClueStatusChanged, ISubscriber, IClueAddendumStatusChanged
{
	private readonly List<BlueprintClue> m_CluesReceived = new List<BlueprintClue>();

	private readonly List<BlueprintClueAddendum> m_AddendumsReceived = new List<BlueprintClueAddendum>();

	public override bool HasData => m_CluesReceived.Any();

	public override int Order => 2;

	public override NotificationCategory Category => NotificationCategory.Detective;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.ClueReceived;

	public override LocalizedString HeaderText => UIStrings.Instance.Dialog.NewClueReceived;

	public override bool HasNewItems => true;

	public CluesReceivedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleClueStatusChanged(BlueprintClue blueprint)
	{
		if (!blueprint.ParentCase.Blueprint.IsClosed() && Game.Instance.DetectiveSystem.HasClueExcludingHidden(blueprint) && !m_CluesReceived.Any((BlueprintClue c) => c.HasOverride(blueprint)))
		{
			m_CluesReceived.Add(blueprint);
			m_AddendumsReceived.RemoveAll((BlueprintClueAddendum a) => a.ParentClue.Blueprint == blueprint);
		}
	}

	public void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint)
	{
		if (blueprint.ParentCase.Blueprint.IsOpen() && Game.Instance.DetectiveSystem.HasClueExcludingHidden(blueprint.ParentClue))
		{
			m_AddendumsReceived.Add(blueprint);
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		if (!m_CluesReceived.Any())
		{
			return new List<DialogNotificationVM>();
		}
		return (from clue in m_CluesReceived
			group clue by clue.ParentCase.Blueprint.AssetGuid into @group
			select CreateClueNotification(UINotificationTexts.Instance.CluesAddedFormat, @group.ToList())).ToList();
	}

	private DialogNotificationVM CreateClueNotification(string prefix, List<BlueprintClue> clues)
	{
		BlueprintClue blueprintClue = clues.FirstOrDefault();
		if (blueprintClue == null)
		{
			return DialogNotificationVM.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < clues.Count; i++)
		{
			BlueprintClue clue = clues[i];
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			IEnumerable<BlueprintClueAddendum> addendums = ((!blueprintClue.ParentCase.Blueprint.IsUnknown()) ? m_AddendumsReceived.Where((BlueprintClueAddendum a) => a.ParentClue == clue) : Enumerable.Empty<BlueprintClueAddendum>());
			string additionalInfo = DetectiveInfoEncryption.EncryptAddendums(clue, addendums);
			NotificationFormatter.AppendClue(clue, stringBuilder, m_DialogUIType, additionalInfo);
		}
		string label = NotificationFormatter.FormatText(string.Format(prefix, stringBuilder), NotificationType.Positive);
		bool num = Game.Instance.DetectiveSystem.GetCaseStatus(blueprintClue.ParentCase) == CaseStatus.None;
		TooltipTemplateDetective iconTooltip = (num ? new TooltipTemplateDetective(null) : new TooltipTemplateDetective(blueprintClue.ParentCase.Blueprint));
		Sprite icon = (num ? UIConfig.Instance.DetectiveConfig.UnknownCluesIcon : Game.Instance.DetectiveSystem.GetCaseDisplay(blueprintClue.ParentCase).Icon);
		return new DialogNotificationVM(label, icon, iconTooltip);
	}

	public override void Clear()
	{
		m_CluesReceived.Clear();
		m_AddendumsReceived.Clear();
	}
}
