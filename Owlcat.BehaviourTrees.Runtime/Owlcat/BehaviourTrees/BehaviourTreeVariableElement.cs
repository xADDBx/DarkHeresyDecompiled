using System;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[Serializable]
[TypeId("489938ab623f44c789bd9b08f7e3abfb")]
public abstract class BehaviourTreeVariableElement : BehaviourTreeElement
{
	public string Key;

	public bool IsPublic;

	public new string Id => name;

	public string DisplayKey
	{
		get
		{
			if (!string.IsNullOrEmpty(Key))
			{
				return Key;
			}
			return "<None>";
		}
	}

	public bool IsSettable { get; set; } = true;


	public abstract BlackboardVariable CreateVariable();

	public override string GetCaption()
	{
		return "Variable: " + DisplayKey;
	}
}
[TypeId("489938ab623f44c789bd9b08f7e3abfb")]
public abstract class BehaviourTreeVariableElement<TValue> : BehaviourTreeVariableElement
{
	public TValue Value;
}
