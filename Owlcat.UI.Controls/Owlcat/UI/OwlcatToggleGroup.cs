using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace Owlcat.UI;

[AddComponentMenu("UI/Owlcat/Owlcat Toggle Group", 31)]
[DisallowMultipleComponent]
public class OwlcatToggleGroup : MonoBehaviour
{
	public readonly ReactiveProperty<OwlcatToggle> ActiveToggle = new ReactiveProperty<OwlcatToggle>();

	[SerializeField]
	private bool m_AllowSwitchOff;

	private readonly Dictionary<OwlcatToggle, IDisposable> m_Toggles = new Dictionary<OwlcatToggle, IDisposable>();

	public bool AllowSwitchOff => m_AllowSwitchOff;

	public IReadOnlyCollection<OwlcatToggle> Toggles => m_Toggles.Keys;

	private OwlcatToggle FirstActiveToggle => ActiveToggles().FirstOrDefault();

	public event Action<OwlcatToggle> ToggleRegistered;

	public event Action<OwlcatToggle> ToggleUnregistered;

	private void Start()
	{
		EnsureValidState();
	}

	private void OnEnable()
	{
		EnsureValidState();
	}

	private void OnDisable()
	{
		foreach (IDisposable value in m_Toggles.Values)
		{
			value.Dispose();
		}
		m_Toggles.Clear();
	}

	private void EnsureValidState()
	{
		if (!m_AllowSwitchOff && !AnyTogglesOn() && m_Toggles.Count != 0)
		{
			m_Toggles.Keys.First().Set(value: true);
		}
		IEnumerable<OwlcatToggle> enumerable = ActiveToggles();
		if (enumerable.Count() > 1)
		{
			OwlcatToggle firstActiveToggle = FirstActiveToggle;
			foreach (OwlcatToggle item in enumerable)
			{
				if (!(item == firstActiveToggle))
				{
					item.Set(value: false);
				}
			}
		}
		UpdateActiveToggle();
	}

	public bool AnyTogglesOn()
	{
		return m_Toggles.Keys.Any((OwlcatToggle x) => x.IsOn.CurrentValue);
	}

	public IEnumerable<OwlcatToggle> ActiveToggles()
	{
		return m_Toggles.Keys.Where((OwlcatToggle x) => x.IsOn.CurrentValue);
	}

	public void RegisterToggle(OwlcatToggle toggle)
	{
		if (!m_Toggles.ContainsKey(toggle))
		{
			m_Toggles[toggle] = toggle.IsOn.Subscribe(delegate
			{
				HandleToggleChanged(toggle);
			});
			this.ToggleRegistered?.Invoke(toggle);
		}
	}

	public void UnregisterToggle(OwlcatToggle toggle)
	{
		if (m_Toggles.TryGetValue(toggle, out var value))
		{
			value.Dispose();
			m_Toggles.Remove(toggle);
			this.ToggleUnregistered?.Invoke(toggle);
		}
	}

	private void HandleToggleChanged(OwlcatToggle toggle)
	{
		if (toggle.IsOn.CurrentValue)
		{
			HandleToggleOn(toggle);
		}
		UpdateActiveToggle();
	}

	private void HandleToggleOn(OwlcatToggle currentToggle)
	{
		foreach (OwlcatToggle key in m_Toggles.Keys)
		{
			if (!(key == currentToggle))
			{
				key.Set(value: false);
			}
		}
	}

	private void UpdateActiveToggle()
	{
		ActiveToggle.Value = FirstActiveToggle;
	}
}
