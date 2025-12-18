using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4bde1b0763e647c8bc60af834c284536")]
public class IsTargetReachable : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + FormulaTargetScope.Current + " can reach " + Target.Colorized();
	}

	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		if (!(this.GetTargetByType(Target) is BaseUnitEntity baseUnitEntity2))
		{
			return false;
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(baseUnitEntity.MovementAgent, baseUnitEntity2, limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, baseUnitEntity2))
		{
			if (warhammerPathPlayer == null || warhammerPathPlayer.path.Count == 0)
			{
				return false;
			}
			GraphNode endNode = warhammerPathPlayer.path.Last();
			if (endNode == null)
			{
				return false;
			}
			return baseUnitEntity2.GetOccupiedNodes().Any((GridNodeBase n) => n.ContainsOutgoingConnection(endNode) && endNode.ContainsOutgoingConnection(n));
		}
	}
}
