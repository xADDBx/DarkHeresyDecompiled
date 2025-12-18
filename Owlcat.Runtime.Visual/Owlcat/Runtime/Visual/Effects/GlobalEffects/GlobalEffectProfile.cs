using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects;

public class GlobalEffectProfile : ScriptableObject
{
	public List<ComponentBase> Components = new List<ComponentBase>();

	[NonSerialized]
	public bool IsDirty = true;

	internal int GetComponentListHashCode()
	{
		int num = 17;
		for (int i = 0; i < Components.Count; i++)
		{
			num = num * 23 + Components[i].GetType().GetHashCode();
		}
		return num;
	}

	public bool Has(Type type)
	{
		foreach (ComponentBase component in Components)
		{
			if (component.GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	internal void Sanitize()
	{
		for (int num = Components.Count - 1; num >= 0; num--)
		{
			if (Components[num] == null)
			{
				Components.RemoveAt(num);
			}
		}
	}
}
