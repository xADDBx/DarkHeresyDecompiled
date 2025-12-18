using System;
using Kingmaker.UI.Sound.Base;

namespace Kingmaker.Code.UI.MVVM;

public interface IBarkHandle
{
	VoiceOverStatus VoiceOverStatus { get; }

	bool IsPlayingBark();

	void InterruptBark();

	void AddCallback(Action callback);
}
