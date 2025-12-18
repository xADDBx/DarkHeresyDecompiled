using System;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Alignments;

[Serializable]
public class AlignmentShift
{
	[FormerlySerializedAs("Direction")]
	public AlignmentAxis Axis;

	[HideIf("NoShift")]
	public int Value;

	[HideIf("NoShift")]
	public LocalizedString Description;

	public bool NoShift => Axis == AlignmentAxis.None;
}
