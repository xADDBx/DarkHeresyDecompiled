using JetBrains.Annotations;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.CharactersRigidbody;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Entities;

public static class UnitEntityViewHelper
{
	public static IKController GetIKControllerOptional([NotNull] this AbstractUnitEntityView view)
	{
		return (view as UnitEntityView)?.IkController;
	}

	public static RigidbodyCreatureController GetRigidbodyControllerOptional([NotNull] this AbstractUnitEntityView view)
	{
		return (view as UnitEntityView)?.RigidbodyController;
	}

	public static Vector3 GetViewPosition([NotNull] this IAbstractUnitEntityView view)
	{
		return view.GetViewPosition(view.Data.Position);
	}

	public static Vector3 GetViewPosition([NotNull] this IAbstractUnitEntityView view, Vector3 mechanicsPosition)
	{
		if (view.MovementAgent.NodeLinkTraverser.IsTraverseNow)
		{
			return SizePathfindingHelper.FromMechanicsToViewPosition((MechanicEntity)view.Data, mechanicsPosition);
		}
		return view.AsAbstractUnitEntityView().GetViewPositionOnGround(mechanicsPosition);
	}
}
