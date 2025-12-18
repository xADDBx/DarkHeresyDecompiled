using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItem))]
[TypeId("d6f0c3aef90114545b6ae50863a93075")]
public class MoneyReplacement : BlueprintComponent
{
	public long Cost = 1L;
}
