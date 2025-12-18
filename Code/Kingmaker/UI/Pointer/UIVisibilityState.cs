using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using R3;

namespace Kingmaker.UI.Pointer;

public static class UIVisibilityState
{
	public const int PresetHideAllIndex = 0;

	public const int PresetShawAllIndex = 9;

	private static readonly UIVisibilityFlags[] s_Presets = new UIVisibilityFlags[10]
	{
		UIVisibilityFlags.None,
		UIVisibilityFlags.StaticPart,
		UIVisibilityFlags.DynamicPart,
		UIVisibilityFlags.CommonPart,
		UIVisibilityFlags.Pointer,
		UIVisibilityFlags.BugReport,
		UIVisibilityFlags.StaticPart | UIVisibilityFlags.DynamicPart,
		UIVisibilityFlags.StaticPart | UIVisibilityFlags.DynamicPart | UIVisibilityFlags.CommonPart,
		UIVisibilityFlags.StaticPart | UIVisibilityFlags.DynamicPart | UIVisibilityFlags.CommonPart | UIVisibilityFlags.Pointer,
		UIVisibilityFlags.StaticPart | UIVisibilityFlags.DynamicPart | UIVisibilityFlags.CommonPart | UIVisibilityFlags.Pointer | UIVisibilityFlags.BugReport
	};

	private static readonly ReactiveProperty<UIVisibilityFlags> s_VisibilityPreset = new ReactiveProperty<UIVisibilityFlags>(s_Presets[9]);

	private static int s_VisibilityPresetIndex = 9;

	public static ReadOnlyReactiveProperty<UIVisibilityFlags> VisibilityPreset => s_VisibilityPreset;

	public static int VisibilityPresetIndex => s_VisibilityPresetIndex;

	public static void HideAllUI()
	{
		SetVisibilityPresetIndex(0);
	}

	public static void ShowAllUI()
	{
		SetVisibilityPresetIndex(9);
	}

	private static void SetVisibilityPresetIndex(int value)
	{
		if (s_VisibilityPresetIndex != value)
		{
			if (value >= s_Presets.Length)
			{
				value = 0;
			}
			s_VisibilityPresetIndex = value;
			s_VisibilityPreset.Value = s_Presets[value];
			EventBus.RaiseEvent(delegate(IUIVisibilityHandler h)
			{
				h.HandleUIVisibilityChange(s_VisibilityPreset.Value);
			});
		}
	}

	public static void SwitchVisibility()
	{
		SetVisibilityPresetIndex(s_VisibilityPresetIndex + 1);
	}
}
