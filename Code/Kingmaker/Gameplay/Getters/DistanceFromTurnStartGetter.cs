using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("79c147a78832471aa440a319d87aaa36")]
public sealed class DistanceFromTurnStartGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		Vector3? vector = base.CurrentEntity.GetCombatStateOptional()?.TurnStartPosition;
		if (vector.HasValue)
		{
			Vector3 valueOrDefault = vector.GetValueOrDefault();
			return base.CurrentEntity.DistanceToInCells(valueOrDefault);
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Distance in cells from position where unit start turn";
	}
}
