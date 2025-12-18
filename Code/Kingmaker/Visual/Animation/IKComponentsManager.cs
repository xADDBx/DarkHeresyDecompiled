using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

internal class IKComponentsManager : MonoBehaviour
{
	private readonly HashSet<IIKComponent> m_Components = new HashSet<IIKComponent>();

	private readonly List<IIKComponent> m_ToAdd = new List<IIKComponent>();

	private readonly List<IIKComponent> m_ToRemove = new List<IIKComponent>();

	public static IKComponentsManager Instance { get; private set; }

	private void Start()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	private void OnDestroy()
	{
		m_Components.Clear();
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void RegisterComponent(IIKComponent component)
	{
		m_ToAdd.Add(component);
	}

	public void UnregisterComponent(IIKComponent component)
	{
		m_ToRemove.Add(component);
	}

	private void LateUpdate()
	{
		m_Components.ForEach(delegate(IIKComponent c)
		{
			c?.DoLateUpdate();
		});
		m_Components.RemoveRange(m_ToRemove);
		m_Components.AddRange(m_ToAdd);
		m_ToRemove.Clear();
		m_ToAdd.Clear();
	}
}
