using System;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[Obsolete]
[TypeId("0e1f492ac13c4af9add252666e481277")]
public class AbilityAreaEffectMovement : AreaEffectLogic
{
	public Feet DistancePerRound;

	public float SpeedMps => DistancePerRound.Meters / 5f;

	protected override void OnTick(IEvalContext context, AreaEffectEntity areaEffect)
	{
		Vector3 normalized = (Quaternion.Euler(0f, context.ClickedTarget.Orientation, 0f) * Vector3.forward).normalized;
		Vector3 vector = areaEffect.Position + normalized * SpeedMps * Game.Instance.Controllers.TimeController.DeltaTime;
		if (Physics.Raycast(new Ray(vector + new Vector3(0f, 10f, 0f), Vector3.down), out var hitInfo, 20f, 2359553))
		{
			vector = hitInfo.point;
		}
		if (!LineOfSightGeometry.Instance.HasObstacle(areaEffect.Position, vector))
		{
			areaEffect.Position = vector;
		}
	}
}
