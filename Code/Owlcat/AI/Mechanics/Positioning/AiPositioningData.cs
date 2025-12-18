using Kingmaker.ElementsSystem.ContextData;
using Pathfinding;

namespace Owlcat.AI.Mechanics.Positioning;

public class AiPositioningData : ContextData<AiPositioningData>
{
	private GraphNode m_Node;

	private IntRect m_SizeRect;

	public static GraphNode CurrentNode => ContextData<AiPositioningData>.Current?.m_Node;

	public static IntRect CurrentSizeRect => ContextData<AiPositioningData>.Current?.m_SizeRect ?? default(IntRect);

	public AiPositioningData Setup(GraphNode node)
	{
		m_Node = node;
		return this;
	}

	public AiPositioningData Setup(IntRect sizeRect)
	{
		m_SizeRect = sizeRect;
		return this;
	}

	public AiPositioningData Setup(GraphNode node, IntRect sizeRect)
	{
		m_Node = node;
		m_SizeRect = sizeRect;
		return this;
	}

	protected override void Reset()
	{
	}
}
