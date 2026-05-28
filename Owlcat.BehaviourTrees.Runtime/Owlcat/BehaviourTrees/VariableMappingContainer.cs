using System;
using System.Collections.Generic;
using System.Linq;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class VariableMappingContainer
{
	public List<VariableMapping> VariableMappings = new List<VariableMapping>();

	public bool TryGet(string variableId, out VariableMapping mapping)
	{
		mapping = VariableMappings.FirstOrDefault((VariableMapping m) => m.SubTreeVariableId == variableId);
		return mapping != null;
	}

	public void Update(BehaviourTreeSerializableData data)
	{
		if (data == null)
		{
			VariableMappings.Clear();
			return;
		}
		foreach (BehaviourTreeVariableElement variable in data.Variables)
		{
			if (variable.IsPublic && VariableMappings.FirstOrDefault((VariableMapping v) => v.SubTreeVariableId == variable.Id) == null)
			{
				VariableMappings.Add(new VariableMapping
				{
					SubTreeVariableId = variable.Id
				});
			}
		}
		for (int num = VariableMappings.Count - 1; num >= 0; num--)
		{
			VariableMapping mapping = VariableMappings[num];
			BehaviourTreeVariableElement behaviourTreeVariableElement = data.Variables.FirstOrDefault((BehaviourTreeVariableElement v) => v.Id == mapping.SubTreeVariableId);
			if (behaviourTreeVariableElement == null || !behaviourTreeVariableElement.IsPublic)
			{
				VariableMappings.RemoveAt(num);
			}
		}
	}
}
