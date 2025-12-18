using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[Obsolete]
[TypeId("965e937484aeea541a6b32e1d76d6e7f")]
public class SavesFixerRecalculate : UnitFactComponentDelegate
{
	protected override void OnActivate()
	{
	}
}
