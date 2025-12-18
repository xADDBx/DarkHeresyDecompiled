using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITextAlignment
{
	[Header("Alignments")]
	public LocalizedString Xenophilia;

	public LocalizedString Xanthite;

	public LocalizedString Monodominance;

	public LocalizedString Torian;

	[Header("Alignment Ranks")]
	public LocalizedString SoulMarkRankTierNone;

	public LocalizedString AlignmentMarkRankTier1;

	public LocalizedString AlignmentMarkRankTier2;

	public LocalizedString AlignmentMarkRankTier3;

	public LocalizedString AlignmentMarkRankTier4;

	public LocalizedString AlignmentMarkRankTier5;

	[Header("Conviction")]
	public LocalizedString RadicalTitle;

	public LocalizedString RadicalDescription;

	public LocalizedString PuritanTitle;

	public LocalizedString PuritanDescription;

	public LocalizedString CurrentConvictionTitle;

	public LocalizedString CurrentConvictionDescription;
}
