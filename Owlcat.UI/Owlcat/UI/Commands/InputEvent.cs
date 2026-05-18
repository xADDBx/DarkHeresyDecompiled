using UnityEngine;

namespace Owlcat.UI.Commands;

public abstract class InputEvent
{
	public virtual bool Consumed { get; internal set; }

	public virtual bool Bubbling { get; internal set; }

	public virtual float Progress { get; internal set; } = 1f;


	public virtual float GetAxis()
	{
		return 0f;
	}

	public virtual Vector2 GetAxis2D()
	{
		return Vector2.zero;
	}

	public virtual bool IsTrigger(string binding)
	{
		return false;
	}

	protected static bool Contains(string binding, string bindings)
	{
		int num = bindings.IndexOf(binding);
		if (num == -1)
		{
			return false;
		}
		int num2 = num + binding.Length - 1;
		if (num2 >= bindings.Length - 1)
		{
			return true;
		}
		return bindings[num2 + 1] == ';';
	}
}
