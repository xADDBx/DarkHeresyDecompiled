using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public abstract class MaterialCollection<R> where R : IRenderPipelineResources
{
	private List<Material> m_Materials;

	public abstract void Init(R resources);

	protected Material Load(Shader shader)
	{
		if (shader == null)
		{
			Debug.LogErrorFormat("Missing shader. PostProcessing render passes will not execute. Check for missing reference in the renderer resources.");
			return null;
		}
		if (!shader.isSupported)
		{
			return null;
		}
		Material material = CoreUtils.CreateEngineMaterial(shader);
		if (m_Materials == null)
		{
			m_Materials = new List<Material>();
		}
		m_Materials.Add(material);
		return material;
	}

	public void Cleanup()
	{
		foreach (Material material in m_Materials)
		{
			CoreUtils.Destroy(material);
		}
		m_Materials.Clear();
	}
}
