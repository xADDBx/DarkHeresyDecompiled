using System;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class AlignmentAxisStaticInfo
{
	public AlignmentAxis Axis;

	public LocalizedString Name;

	public LocalizedString Description;

	public Sprite Icon;
}
