using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.Debug;

public static class PixelsBrightnessAnalyzerSettings
{
	private const float DefaultDarkThreshold = 0.05f;

	private const float DefaultBrightThreshold = 0.95f;

	public static bool Enabled;

	public static PixelsBrightnessMode Mode = PixelsBrightnessMode.Luminosity;

	public static bool ShowHighlight = true;

	public static bool BeforePostProcessing = true;

	public static float DarkThreshold = 0.05f;

	public static Color DarkColor = new Color(1f, 0.5f, 0.5f, 1f);

	public static float DarkPixelPercentage;

	public static float BrightThreshold = 0.95f;

	public static Color BrightColor = new Color(0.5f, 0.5f, 1f, 1f);

	public static float BrightPixelPercentage;

	public static bool ShowHistogram;

	private static bool s_PanelRegistered;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void RegisterDebugPanel()
	{
		if (!s_PanelRegistered)
		{
			s_PanelRegistered = true;
			DebugUI.Panel panel = DebugManager.instance.GetPanel("Pixels Brightness Analyzer", createIfNull: true);
			panel.children.Add(new DebugUI.BoolField
			{
				displayName = "Enable",
				tooltip = "Enable the brightness analyzer. Runs a compute pass each frame to analyze pixel brightness",
				getter = () => Enabled,
				setter = delegate(bool v)
				{
					Enabled = v;
				}
			});
			panel.children.Add(new DebugUI.EnumField
			{
				displayName = "Brightness Mode",
				tooltip = "Luminosity: Rec.709 perceptual brightness (matches Photoshop's Luminosity histogram). MaxChannel: max(R,G,B) — equals Photoshop's HSB Brightness, useful for per-channel clipping detection.",
				autoEnum = typeof(PixelsBrightnessMode),
				getter = () => (int)Mode,
				setter = delegate
				{
				},
				getIndex = () => (int)Mode,
				setIndex = delegate(int v)
				{
					Mode = (PixelsBrightnessMode)v;
				}
			});
			panel.children.Add(new DebugUI.BoolField
			{
				displayName = "Highlight Pixels",
				tooltip = "Highlight dark/bright pixels with overlay colors",
				getter = () => ShowHighlight,
				setter = delegate(bool v)
				{
					ShowHighlight = v;
				}
			});
			panel.children.Add(new DebugUI.BoolField
			{
				displayName = "Before Post Processing",
				tooltip = "Analyze HDR linear buffer before tonemapping (true) or final LDR buffer after post processing (false)",
				getter = () => BeforePostProcessing,
				setter = delegate(bool v)
				{
					BeforePostProcessing = v;
				}
			});
			panel.children.Add(new DebugUI.FloatField
			{
				displayName = "Dark Threshold",
				tooltip = "Pixels with luminance at or below this value are highlighted (0 = black, 0.5 = mid-gray, 1 = white)",
				getter = () => DarkThreshold,
				setter = delegate(float v)
				{
					DarkThreshold = v;
				},
				min = () => 0f,
				max = () => 1f
			});
			panel.children.Add(new DebugUI.ColorField
			{
				displayName = "Dark Color",
				tooltip = "Overlay color for pixels at or below the dark threshold",
				getter = () => DarkColor,
				setter = delegate(Color v)
				{
					DarkColor = v;
				},
				hdr = false,
				showAlpha = false
			});
			panel.children.Add(new DebugUI.Value
			{
				displayName = "Dark Pixel Percentage",
				tooltip = "Percentage of screen pixels at or below the dark threshold",
				getter = () => $"{DarkPixelPercentage:F2}%"
			});
			panel.children.Add(new DebugUI.FloatField
			{
				displayName = "Bright Threshold",
				tooltip = "Pixels with luminance at or above this value are highlighted (0 = black, 0.5 = mid-gray, 1 = white)",
				getter = () => BrightThreshold,
				setter = delegate(float v)
				{
					BrightThreshold = v;
				},
				min = () => 0f,
				max = () => 1f
			});
			panel.children.Add(new DebugUI.ColorField
			{
				displayName = "Bright Color",
				tooltip = "Overlay color for pixels at or above the bright threshold",
				getter = () => BrightColor,
				setter = delegate(Color v)
				{
					BrightColor = v;
				},
				hdr = false,
				showAlpha = false
			});
			panel.children.Add(new DebugUI.Value
			{
				displayName = "Bright Pixel Percentage",
				tooltip = "Percentage of screen pixels at or above the bright threshold",
				getter = () => $"{BrightPixelPercentage:F2}%"
			});
			panel.children.Add(new DebugUI.BoolField
			{
				displayName = "Show Histogram",
				tooltip = "Display brightness histogram overlay in the corner of the screen",
				getter = () => ShowHistogram,
				setter = delegate(bool v)
				{
					ShowHistogram = v;
				}
			});
			panel.children.Add(new DebugUI.Value
			{
				displayName = "Status",
				getter = () => (!(DarkThreshold >= BrightThreshold)) ? "" : "!!! ERROR: DARK >= BRIGHT !!!"
			});
		}
	}
}
