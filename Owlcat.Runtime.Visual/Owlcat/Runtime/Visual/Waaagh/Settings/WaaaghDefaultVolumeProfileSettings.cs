using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Settings;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "Volume", Order = 0)]
public class WaaaghDefaultVolumeProfileSettings : IDefaultVolumeProfileSettings, IRenderPipelineGraphicsSettings
{
	internal enum Version
	{
		Initial
	}

	[SerializeField]
	[HideInInspector]
	private Version m_Version;

	[SerializeField]
	private VolumeProfile m_VolumeProfile;

	public int version => (int)m_Version;

	public VolumeProfile volumeProfile
	{
		get
		{
			return m_VolumeProfile;
		}
		set
		{
			this.SetValueAndNotify(ref m_VolumeProfile, value, "volumeProfile");
		}
	}
}
