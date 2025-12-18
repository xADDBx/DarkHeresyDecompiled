using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.EntityBlackboard;

[Serializable]
[TypeId("1d37bcd71b8844f18cc2b2c5ecca92ea")]
public class BooleanVariableElement : VariableElement
{
	[SerializeField]
	private bool m_Value;

	public bool Value => m_Value;

	public override string GetValueAsText()
	{
		return m_Value.ToString();
	}
}
