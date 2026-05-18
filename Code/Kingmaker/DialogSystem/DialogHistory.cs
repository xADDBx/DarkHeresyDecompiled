using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.DialogSystem;

public class DialogHistory
{
	[NotNull]
	public readonly List<IDialogShowData> History = new List<IDialogShowData>();

	private readonly DialogContext m_Context;

	private readonly string m_PlayerCharacterName;

	private readonly BlueprintUnit m_PlayerCharacterBlueprint;

	private IDialogShowData m_PendingCueHistoryEntry;

	public DialogHistory(DialogContext context, BaseUnitEntity playerCharacter)
	{
		m_Context = context;
		m_PlayerCharacterName = playerCharacter?.CharacterName;
		m_PlayerCharacterBlueprint = playerCharacter?.Blueprint;
	}

	public void AddCue(BlueprintCue cue)
	{
		m_PendingCueHistoryEntry = ((!string.IsNullOrEmpty(m_Context.CurrentSpeakerName)) ? new CueShowData(cue, m_Context.CurrentSpeakerName, m_Context.CurrentSpeakerBlueprint) : new CueShowData(cue));
	}

	public void AddAnswer(BlueprintAnswer answer)
	{
		AddPendingCueEntry();
		AddAnswerEntry(answer);
	}

	private void AddAnswerEntry([NotNull] BlueprintAnswer answer)
	{
		if (answer.AddToHistory)
		{
			AddEntry(new AnswerShowData(answer, m_PlayerCharacterName, m_PlayerCharacterBlueprint));
		}
	}

	private void AddPendingCueEntry()
	{
		if (m_PendingCueHistoryEntry != null)
		{
			AddEntry(m_PendingCueHistoryEntry);
			m_PendingCueHistoryEntry = null;
		}
	}

	private void AddEntry(IDialogShowData entry)
	{
		History.Add(entry);
		EventBus.RaiseEvent(delegate(IDialogHistoryHandler h)
		{
			h.HandleOnDialogHistory(entry);
		});
	}
}
