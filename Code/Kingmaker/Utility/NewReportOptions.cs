using System;
using JetBrains.Annotations;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Utility;

public class NewReportOptions : IDisposable
{
	private bool _isDisposed;

	[CanBeNull]
	public TooltipData Tooltip { get; private set; }

	[CanBeNull]
	public Texture2D Screenshot { get; private set; }

	public string OtherUiFeatureName { get; }

	public bool WithSave { get; private set; }

	public bool WithCrashDump { get; private set; }

	internal NewReportOptions([CanBeNull] TooltipData tooltip, [CanBeNull] Texture2D screenshot, string otherUiFeatureName, bool withSave, bool withCrashDump)
	{
		Tooltip = tooltip;
		Screenshot = screenshot;
		OtherUiFeatureName = otherUiFeatureName;
		WithSave = withSave;
		WithCrashDump = withCrashDump;
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			if (Screenshot != null)
			{
				UnityEngine.Object.Destroy(Screenshot);
			}
			_isDisposed = true;
		}
	}
}
