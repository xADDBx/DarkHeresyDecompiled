using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using UnityEngine;

namespace Kingmaker.Code.Framework.GameLog;

public class DialogHistoryLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventDialogHistory>
{
	public void HandleEvent(GameLogEventDialogHistory evt)
	{
		DialogCueColors defaultDialogCueColors = ConfigRoot.Instance.UIConfig.m_DefaultDialogCueColors;
		AddMessage(new CombatLogMessage(evt.GetText(defaultDialogCueColors), default(Color), PrefixIcon.None, null, hasTooltip: false));
	}
}
