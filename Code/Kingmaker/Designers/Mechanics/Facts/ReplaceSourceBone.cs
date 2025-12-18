using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("1672a7efc00918248b38e878a9424794")]
public class ReplaceSourceBone : UnitFactComponentDelegate
{
	public string SourceBone;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<UnitPartVisualChanges>().AddReplacementBone(SourceBone);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartVisualChanges>()?.RemoveReplacementBone(SourceBone);
	}
}
