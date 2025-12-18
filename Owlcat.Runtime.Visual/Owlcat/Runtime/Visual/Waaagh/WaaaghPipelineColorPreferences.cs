using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh;

internal class WaaaghPipelineColorPreferences
{
	internal static Func<Color> GetPreviewCameraBackgoundColor;

	internal static Color PreviewCameraBackgroundColor;

	static WaaaghPipelineColorPreferences()
	{
		PreviewCameraBackgroundColor = new Color(0.32156864f, 0.32156864f, 0.32156864f, 0f);
	}
}
