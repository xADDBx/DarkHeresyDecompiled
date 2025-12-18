using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using R3;

namespace Owlcat.UI;

public class InputEnabledScope : IDisposable
{
	private static readonly List<InputEnabledScope> s_InputEnabledScopes = new List<InputEnabledScope>();

	private static ReadOnlyReactiveProperty<bool> s_InputEnabledProperty;

	private ReadOnlyReactiveProperty<bool> m_InputEnabled;

	[CanBeNull]
	public static ReadOnlyReactiveProperty<bool> GetInputEnabledProperty([CanBeNull] ReadOnlyReactiveProperty<bool> source = null)
	{
		if (s_InputEnabledProperty != null)
		{
			if (source == null)
			{
				return s_InputEnabledProperty;
			}
			return source.And(s_InputEnabledProperty).ToReadOnlyReactiveProperty(s_InputEnabledProperty.CurrentValue);
		}
		return source;
	}

	private static void AddScope(InputEnabledScope scope)
	{
		s_InputEnabledScopes.Add(scope);
		UpdateProperty();
	}

	private static void RemoveScope(InputEnabledScope scope)
	{
		s_InputEnabledScopes.Remove(scope);
		UpdateProperty();
	}

	private static void UpdateProperty()
	{
		if (s_InputEnabledScopes.Count == 0)
		{
			s_InputEnabledProperty = null;
			return;
		}
		Observable<bool> observable = s_InputEnabledScopes[0].m_InputEnabled;
		for (int i = 1; i < s_InputEnabledScopes.Count; i++)
		{
			observable = observable.And(s_InputEnabledScopes[i].m_InputEnabled);
		}
		s_InputEnabledProperty = observable.ToReadOnlyReactiveProperty(initialValue: false);
	}

	public InputEnabledScope(ReadOnlyReactiveProperty<bool> inputEnabled)
	{
		m_InputEnabled = inputEnabled;
		AddScope(this);
	}

	public void Dispose()
	{
		RemoveScope(this);
	}
}
