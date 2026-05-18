using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITextAlignment
{
	[Header("Alignment Ranks")]
	public LocalizedString AlignmentRankTierNone;

	public LocalizedString AlignmentRankTier1;

	public LocalizedString AlignmentRankTier2;

	public LocalizedString AlignmentRankTier3;

	public LocalizedString AlignmentRankTier4;

	public LocalizedString AlignmentRankTier5;

	[Header("Tooltip")]
	public LocalizedString AlignmentRankTitle;

	public LocalizedString AlignmentMayBeLocked;

	public LocalizedString AlignmentIsLockedOpposite;

	public LocalizedString AlignmentIsLockedMainChoosed;
}
