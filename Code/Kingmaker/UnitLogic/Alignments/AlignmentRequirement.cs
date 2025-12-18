using System;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.UnitLogic.Alignments;

[Serializable]
public class AlignmentRequirement
{
	public AlignmentAxis Direction;

	[InfoBox("Value in `Ranks`, Value >= 0, 0 means no alignment shift or lets you check by mark")]
	[HideIf("m_checkByValueUnavailable")]
	public int Rank;

	[HideIf("m_checkByRankUnavailable")]
	public bool CheckByMark;

	[InfoBox("Value in `Mark`, Value >= 1 and <= 5, 0 means no mark in alignment")]
	[ShowIf("CheckByMark")]
	[HideIf("m_checkByRankUnavailable")]
	public int Mark;

	[HideIf("NoShift")]
	public LocalizedString Description;

	private bool NoShift => Direction == AlignmentAxis.None;

	private bool m_checkByValueUnavailable
	{
		get
		{
			if (!NoShift)
			{
				return CheckByMark;
			}
			return true;
		}
	}

	private bool m_checkByRankUnavailable
	{
		get
		{
			if (!NoShift)
			{
				if (CheckByRank)
				{
					return !CheckByMark;
				}
				return false;
			}
			return true;
		}
	}

	public bool CheckByRank => Rank > 0;

	public bool Empty
	{
		get
		{
			if ((CheckByRank || CheckByMark) && (!CheckByMark || Mark != 0))
			{
				return NoShift;
			}
			return true;
		}
	}
}
