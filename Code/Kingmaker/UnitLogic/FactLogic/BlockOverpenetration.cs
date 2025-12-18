using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("5907f3b54e2e4718a1f00356e189a937")]
public class BlockOverpenetration : UnitFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.BlockOverpenetration.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.BlockOverpenetration.Release();
	}
}
