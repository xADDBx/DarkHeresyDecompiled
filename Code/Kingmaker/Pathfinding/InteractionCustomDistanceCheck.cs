using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.Covers;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class InteractionCustomDistanceCheck : ICustomDistanceCheck
{
	private Vector3 m_InteractionPosition;

	private int m_InteractionRadius;

	public InteractionCustomDistanceCheck(BaseUnitEntity interactor, AbstractInteractionPart part)
	{
		m_InteractionPosition = part.Owner.Position;
		m_InteractionRadius = part.ApproachRadius;
	}

	public bool IsCloseEnough(GraphNode node)
	{
		Vector3 vector = (Vector3)node.position;
		Vector3 eyePosition = vector + LosCalculations.EyeShift;
		return AbstractInteractionPart.IsEnoughCloseForInteraction(vector, eyePosition, m_InteractionPosition, m_InteractionRadius);
	}
}
