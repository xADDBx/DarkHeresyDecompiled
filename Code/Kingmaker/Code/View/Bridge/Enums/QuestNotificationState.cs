using System;

namespace Kingmaker.Code.View.Bridge.Enums;

[Flags]
public enum QuestNotificationState
{
	Nothing = 0,
	New = 1,
	Completed = 2,
	Failed = 4,
	Updated = 8,
	Postponed = 0x10
}
