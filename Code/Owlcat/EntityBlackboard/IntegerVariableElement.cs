using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.EntityBlackboard;

[Serializable]
[TypeId("2e536d49bb3847b093f09e0cbb11d22b")]
public class IntegerVariableElement : VariableElement
{
	[SerializeField]
	private int m_Value;

	public int Value => m_Value;

	public override string GetValueAsText()
	{
		return m_Value.ToString();
	}
}
