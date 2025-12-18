using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("44bf98d9936c43b092cdfec07c997418")]
public class EtudeBracketForbidJumpToWarp : EtudeBracketTrigger
{
	public override bool RequireLinkedArea => true;
}
