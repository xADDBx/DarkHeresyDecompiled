using System.Collections.Generic;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

[AddComponentMenu("")]
public sealed class OcclusionGeometryClipLinkVolumeProxy : MonoBehaviour, IRendererProxy
{
	[SerializeField]
	private PlaneBox m_Bounds;

	private readonly List<OcclusionGeometryClipLinkProxy> m_Proxies;

	private OcclusionGeometryClipLinkVolumeProxy m_Prev;

	private OcclusionGeometryClipLinkVolumeProxy m_Next;

	private float m_Opacity;

	internal int RegistryIndex;

	public PlaneBox Bounds
	{
		get
		{
			return m_Bounds;
		}
		set
		{
			m_Bounds = value;
			OcclusionGeometryClipLinkSystem.UpdateVolume(this);
		}
	}

	private void OnEnable()
	{
		OcclusionGeometryClipLinkSystem.AddVolume(this);
	}

	private void OnDisable()
	{
		OcclusionGeometryClipLinkSystem.RemoveVolume(this);
	}

	public void AddProxy(OcclusionGeometryClipLinkProxy proxy)
	{
		m_Proxies.Add(proxy);
		proxy.SetOpacity(m_Opacity);
	}

	public void RemoveProxy(OcclusionGeometryClipLinkProxy proxy)
	{
		m_Proxies.Remove(proxy);
		proxy.SetOpacity(1f);
	}

	public float GetOpacity()
	{
		return m_Opacity;
	}

	public void SetOpacity(float value)
	{
		if (m_Opacity == value)
		{
			return;
		}
		m_Opacity = value;
		foreach (OcclusionGeometryClipLinkProxy proxy in m_Proxies)
		{
			proxy.SetOpacity(m_Opacity);
		}
	}

	public OcclusionGeometryClipLinkVolumeProxy()
	{
		Vector3 center = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		Vector3 size = Vector3.one;
		m_Bounds = new PlaneBox(in center, in rotation, in size);
		m_Proxies = new List<OcclusionGeometryClipLinkProxy>();
		m_Opacity = 1f;
		RegistryIndex = -1;
		base._002Ector();
	}
}
