using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Owlcat.BehaviourTrees;

public class Blackboard
{
	private readonly Dictionary<string, BlackboardVariableWrapper> m_Map = new Dictionary<string, BlackboardVariableWrapper>();

	public ReadOnlyCollection<BlackboardVariableWrapper> VariableWrappers => new ReadOnlyCollection<BlackboardVariableWrapper>(m_Map.Values.ToList());

	public ReadOnlyCollection<BlackboardVariable> Variables => new ReadOnlyCollection<BlackboardVariable>(m_Map.Values.Select((BlackboardVariableWrapper w) => w.Variable).ToList());

	public void AddVariable(string id, BlackboardVariable variable, string customKey = null)
	{
		if (!m_Map.TryAdd(id, new BlackboardVariableWrapper(customKey ?? variable.Key, variable)))
		{
			throw new Exception("Variable with id '" + id + "' already exists");
		}
	}

	public bool TryGetVariable<T>(string id, out T variable) where T : BlackboardVariable
	{
		if (m_Map.TryGetValue(id, out var value))
		{
			variable = value.Variable as T;
			return true;
		}
		variable = null;
		return false;
	}

	public T GetVariable<T>(string id) where T : BlackboardVariable
	{
		if (TryGetVariable<T>(id, out var variable))
		{
			if (variable == null)
			{
				throw new Exception("Variable with id '" + id + "' is not of type '" + typeof(T).Name + "'");
			}
			return variable;
		}
		throw new Exception("Variable with id '" + id + "' not found");
	}

	public BlackboardVariable GetVariable(string id)
	{
		return GetVariable<BlackboardVariable>(id);
	}

	public BlackboardVariableWrapper GetVariableWrapper(string id)
	{
		if (m_Map.TryGetValue(id, out var value))
		{
			return value;
		}
		throw new Exception("Variable with id '" + id + "' not found");
	}
}
