using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("e99c9aa536f64e948d7408aeff47aced")]
public class BuildPointsReplacement : BlueprintComponent
{
	public int Cost = 1;
}
