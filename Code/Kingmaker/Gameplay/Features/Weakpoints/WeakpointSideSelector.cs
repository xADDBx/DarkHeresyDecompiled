using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Weakpoints;

[Serializable]
public sealed class WeakpointSideSelector
{
	public enum SelectionType
	{
		Closest,
		Opposite,
		Random,
		Specific
	}

	public SelectionType Type;

	[ShowIf("IsSpecificSide")]
	public WeakpointSide SpecificSide;

	private bool IsSpecificSide => Type == SelectionType.Specific;

	public WeakpointSide Select([NotNull] MechanicEntity caster, [NotNull] MechanicEntity target)
	{
		return Type switch
		{
			SelectionType.Random => (WeakpointSide)PFStatefulRandom.UnitRandom.Range(0, 4), 
			SelectionType.Closest => GetClosestSide(caster, target), 
			SelectionType.Opposite => GetClosestSide(caster, target).Opposite(), 
			SelectionType.Specific => SpecificSide, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static WeakpointSide GetClosestSide([NotNull] MechanicEntity caster, [NotNull] MechanicEntity target)
	{
		Vector3 forward = target.Forward;
		Vector3 normalized = (target.Center - caster.Center).normalized;
		return GraphHelper.GetWarhammerAttackSide(forward, normalized, target.Size);
	}

	public override string ToString()
	{
		if (!IsSpecificSide)
		{
			return $"{Type} side";
		}
		return $"{SpecificSide} side";
	}
}
