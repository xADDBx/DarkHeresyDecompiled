using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[ComponentName("Passive/Override vision range")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("67915f4130ad1714a9ba6b2fd210e28f")]
public class OverrideVisionRange : BlueprintComponent
{
	[Range(0f, 22f)]
	public int VisionRangeInMeters = 8;

	[InfoBox(Text = "Очень опасная галка, может ломать вход/выход из боя")]
	public bool AlsoInCombat;
}
