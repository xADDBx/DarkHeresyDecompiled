using System;
using UnityEngine;

namespace Owlcat.EntityBlackboard;

[Serializable]
public class BlackboardVariablesList
{
	[SerializeField]
	[SerializeReference]
	private VariableElement[] m_Variables;

	public int Count
	{
		get
		{
			VariableElement[] variables = m_Variables;
			if (variables == null)
			{
				return 0;
			}
			return variables.Length;
		}
	}

	public VariableElement this[int index] => m_Variables[index];
}
