using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("0926042f5249455a8b09cf6a241c5b2b")]
public class ScrapReplacement : BlueprintComponent
{
	public int Cost = 1;
}
