using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[ExecuteInEditMode]
public class OwlcatDisallowGPUDrivenRendering : MonoBehaviour
{
	private bool m_AppliedRecursively;

	[FormerlySerializedAs("applyToChildrenRecursively")]
	public bool m_applyToChildrenRecursively;

	public bool applyToChildrenRecursively
	{
		get
		{
			return m_applyToChildrenRecursively;
		}
		set
		{
			OnDisable();
			m_applyToChildrenRecursively = value;
			if (base.enabled)
			{
				OnEnable();
			}
		}
	}

	private void OnEnable()
	{
		m_AppliedRecursively = applyToChildrenRecursively;
		if (applyToChildrenRecursively)
		{
			AllowGPUDrivenRenderingRecursively(base.transform, allow: false);
		}
		else
		{
			AllowGPUDrivenRendering(base.transform, allow: false);
		}
	}

	private void OnDisable()
	{
		if (m_AppliedRecursively)
		{
			AllowGPUDrivenRenderingRecursively(base.transform, allow: true);
		}
		else
		{
			AllowGPUDrivenRendering(base.transform, allow: true);
		}
	}

	private static void AllowGPUDrivenRendering(Transform transform, bool allow)
	{
		MeshRenderer component = transform.GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			RendererUtils.SetAllowGPUDrivenRendering(component, allow);
		}
	}

	private static void AllowGPUDrivenRenderingRecursively(Transform transform, bool allow)
	{
		AllowGPUDrivenRendering(transform, allow);
		foreach (Transform item in transform)
		{
			if (!item.GetComponent<OwlcatDisallowGPUDrivenRendering>())
			{
				AllowGPUDrivenRenderingRecursively(item, allow);
			}
		}
	}

	private void OnValidate()
	{
		OnDisable();
		if (base.enabled)
		{
			OnEnable();
		}
	}
}
