using System;

namespace Owlcat.EntityBlackboard;

[Serializable]
public abstract class VariableElement
{
	public string Key;

	public virtual string GetValueAsText()
	{
		return string.Empty;
	}
}
