using System;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class VariableMapping
{
	public string SubTreeVariableId;

	public string SuperTreeVariableId;

	public static bool operator ==(VariableMapping vm1, VariableMapping vm2)
	{
		if ((object)vm1 == vm2)
		{
			return true;
		}
		if ((object)vm1 == null || (object)vm2 == null)
		{
			return false;
		}
		if (vm1.SubTreeVariableId == vm2.SubTreeVariableId)
		{
			return vm1.SuperTreeVariableId == vm2.SuperTreeVariableId;
		}
		return false;
	}

	public static bool operator !=(VariableMapping vm1, VariableMapping vm2)
	{
		return !(vm1 == vm2);
	}
}
