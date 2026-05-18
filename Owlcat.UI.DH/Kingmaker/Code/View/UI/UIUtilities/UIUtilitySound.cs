using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilitySound
{
	public static void PlaySelectorSound()
	{
		SystemSounds.Instance.Selector.Start.Play();
		SystemSounds.Instance.Selector.LoopStart.Play();
	}

	public static void StopSelectorSound()
	{
		SystemSounds.Instance.Selector.Stop.Play();
		UISounds.Instance.Play(SystemSounds.Instance.Selector.LoopStop, isButton: false, playAnyway: true);
	}

	public static void SetHoverSound(this OwlcatMultiSelectable selectable, ButtonSoundsEnum soundType)
	{
		UISounds.Instance.SetHoverSound(selectable, soundType);
	}

	public static void SetClickSound(this OwlcatSelectable selectable, ButtonSoundsEnum soundType)
	{
		UISounds.Instance.SetClickSound(selectable, soundType);
	}

	public static void SetClickAndHoverSound(this OwlcatSelectable selectable, ButtonSoundsEnum soundType)
	{
		UISounds.Instance.SetClickAndHoverSound(selectable, soundType);
	}
}
