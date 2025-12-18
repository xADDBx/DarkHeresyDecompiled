using System;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CoverHighlightConfig
{
	[Serializable]
	public class CoverHighlightColorEntry
	{
		public DestructionStage Stage;

		public Color StageColor;
	}

	public Color DefaultColor = Color.white;

	public CoverHighlightColorEntry[] Colors;

	public Color GetHighlightColor(DestructionStage stage)
	{
		return Colors.FirstOrDefault((CoverHighlightColorEntry i) => i.Stage == stage)?.StageColor ?? DefaultColor;
	}
}
