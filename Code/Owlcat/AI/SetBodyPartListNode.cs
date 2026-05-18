using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.AI.Mechanics.BodyParts;
using Owlcat.BehaviourTrees;
using Owlcat.Fmw.Blueprints;

namespace Owlcat.AI;

public class SetBodyPartListNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly BodyPartListVariable m_BodyPartsList;

	private readonly EntityVariable m_Target;

	private readonly PropertyCalculator m_Filter;

	public SetBodyPartListNode(EntityVariable agent, BodyPartListVariable bodyPartsList, EntityVariable target, PropertyCalculator filter)
	{
		m_Agent = agent;
		m_BodyPartsList = bodyPartsList;
		m_Target = target;
		m_Filter = filter;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_BodyPartsList.Value = GetFilteredBodyParts(m_Agent.Value, (BaseUnitEntity)m_Target.Value, m_Filter);
		if (m_BodyPartsList.Value.Count <= 0)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static List<BpRef<BlueprintBodyPart>> GetFilteredBodyParts(MechanicEntity agent, BaseUnitEntity target, PropertyCalculator filter)
	{
		IEnumerable<BlueprintBodyPart> enumerable = target?.BodyParts;
		List<BpRef<BlueprintBodyPart>> result = new List<BpRef<BlueprintBodyPart>>();
		if (enumerable == null || !enumerable.Any())
		{
			return result;
		}
		if (filter == null || filter.Empty)
		{
			enumerable.ForEach(delegate(BlueprintBodyPart bp)
			{
				result.Add(bp.Reference());
			});
			return result;
		}
		foreach (BlueprintBodyPart item in enumerable)
		{
			using (ContextData<AiBodyPartsContextData>.Request().Setup(item))
			{
				if (filter.GetBoolValue(agent, null, target))
				{
					result.Add(item.Reference());
				}
			}
		}
		return result;
	}
}
