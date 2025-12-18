using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[TypeId("76a0aedc87e64d9e95518853c442388e")]
public abstract class Requirement : BlueprintComponent
{
	public bool HideInUI;
}
