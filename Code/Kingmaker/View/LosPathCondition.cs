using System;
using Kingmaker.Pathfinding;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public class LosPathCondition : ABPathEndingCondition
{
	private readonly bool m_CheckLos;

	private readonly float m_ApproachRadiusMeters;

	private readonly ICustomDistanceCheck m_CustomDistanceCheck;

	private GraphNode m_EndNode;

	public LosPathCondition(ABPath path, float approachRadiusMeters, bool checkLos, ICustomDistanceCheck customDistanceCheck)
		: base(path)
	{
		m_CheckLos = checkLos;
		m_ApproachRadiusMeters = approachRadiusMeters;
		m_EndNode = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(path.originalEndPoint);
		m_CustomDistanceCheck = customDistanceCheck;
	}

	public override bool TargetFound(GraphNode node, uint H, uint G)
	{
		if (!(node is GridNodeBase))
		{
			return false;
		}
		if (node == m_EndNode)
		{
			return true;
		}
		Vector3 vector = (Vector3)node.position;
		if (m_CustomDistanceCheck != null)
		{
			if (!m_CustomDistanceCheck.IsCloseEnough(node))
			{
				return false;
			}
		}
		else if (GeometryUtils.SqrMechanicsDistance(vector, abPath.originalEndPoint) > m_ApproachRadiusMeters * m_ApproachRadiusMeters)
		{
			return false;
		}
		try
		{
			if (m_CheckLos && LosCalculations.GetDirectLos(vector, abPath.originalEndPoint))
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
		return true;
	}
}
