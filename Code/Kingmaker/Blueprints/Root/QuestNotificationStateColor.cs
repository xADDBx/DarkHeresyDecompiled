using System;
using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class QuestNotificationStateColor
{
	public Color Failed;

	public Color Completed;

	public Color New;

	public Color Updated;

	public Color Postponed;

	public Color GetQuestStateColor(QuestNotificationState state)
	{
		return state switch
		{
			QuestNotificationState.Failed => Failed, 
			QuestNotificationState.Completed => Completed, 
			QuestNotificationState.Updated => Updated, 
			QuestNotificationState.Postponed => Postponed, 
			_ => New, 
		};
	}
}
