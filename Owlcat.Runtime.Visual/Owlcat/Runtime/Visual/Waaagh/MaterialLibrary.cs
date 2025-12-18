using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public class MaterialLibrary<T, R> where T : MaterialCollection<R>, new() where R : IRenderPipelineResources
{
	private R m_Resources;

	private Dictionary<Camera, T> m_Materials = new Dictionary<Camera, T>();

	public MaterialLibrary(R resources)
	{
		m_Resources = resources;
	}

	public T Get(Camera camera)
	{
		if (!m_Materials.TryGetValue(camera, out var value))
		{
			value = new T();
			value.Init(m_Resources);
			m_Materials.Add(camera, value);
		}
		return value;
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<Camera, T> material in m_Materials)
		{
			material.Value.Cleanup();
		}
		m_Materials.Clear();
	}
}
