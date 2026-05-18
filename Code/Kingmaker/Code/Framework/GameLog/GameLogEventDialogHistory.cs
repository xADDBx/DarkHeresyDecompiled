using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;
using WebSocketSharp;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventDialogHistory : GameLogEvent<GameLogEventDialogHistory>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IDialogHistoryHandler, ISubscriber
	{
		public void HandleOnDialogHistory(IDialogShowData showData)
		{
			if (Game.Instance.Controllers.DialogController.Dialog.Type != DialogType.Book)
			{
				AddEvent(new GameLogEventDialogHistory(showData));
			}
		}
	}

	private readonly IDialogShowData m_ShowData;

	private string m_SpeakerNameFormatedText;

	private string m_SkillcheckFormatedText;

	private string m_SoulmarkFormatedText;

	private string m_CueFormatedText;

	public GameLogEventDialogHistory(IDialogShowData showData)
	{
		m_ShowData = showData;
	}

	public string GetText(DialogCueColors dialogCueColors)
	{
		Color color = dialogCueColors.GetSpeakerColor(m_ShowData.SpeakerName, m_ShowData.IsOverrideSpeakerColor, m_ShowData.SpeakerColor);
		m_SpeakerNameFormatedText = ((!m_ShowData.SpeakerName.IsNullOrEmpty()) ? ("<smallcaps><b><color=#" + ColorUtility.ToHtmlStringRGB(color * dialogCueColors.NameColorMultiplayer) + ">" + m_ShowData.SpeakerName + "</color></b></smallcaps>: ") : string.Empty);
		m_CueFormatedText = (m_ShowData.SpeakerName.IsNullOrEmpty() ? ("<i><color=#" + ColorUtility.ToHtmlStringRGB((Color)dialogCueColors.Narrator) + ">" + m_ShowData.Text + "</color><i>") : m_ShowData.Text);
		m_CueFormatedText = UtilityText.StringIDToColor(m_CueFormatedText, DialogCueColors.NarratorColorStringID, dialogCueColors.Narrator);
		return m_SpeakerNameFormatedText + m_SkillcheckFormatedText + m_SoulmarkFormatedText + m_CueFormatedText;
	}
}
