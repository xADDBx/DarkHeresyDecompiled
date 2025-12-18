using Kingmaker.UI.Sound;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilitySound
{
	public static void PlaySelectorSound()
	{
		UISounds.Instance.Sounds.Selector.SelectorStart.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStart.Play();
	}

	public static void StopSelectorSound()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
	}
}
