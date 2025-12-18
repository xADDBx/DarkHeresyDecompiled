using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[ComponentName("Root/BlueprintTrapSettingsRoot")]
[TypeId("96b57c8904784e6d97c584ae370869c0")]
public class BlueprintTrapSettingsRoot : BlueprintScriptableObject
{
	[SerializeField]
	private float m_DefaultPerceptionRadius = 7f;

	[SerializeField]
	private int m_DisableDCMargin = 20;

	[SerializeField]
	private BlueprintTrapSettingsReference[] m_Settings;

	public int EasyDisableDCDelta;

	public int HardDisableDCDelta;

	public int DisableDCMargin => m_DisableDCMargin;

	public float DefaultPerceptionRadius => m_DefaultPerceptionRadius;

	public BlueprintTrapSettings Find(int cr)
	{
		if (m_Settings == null || m_Settings.Length == 0)
		{
			throw new Exception("Missing blueprint trap settings.");
		}
		return m_Settings[Mathf.Clamp(cr, 0, m_Settings.Length - 1)];
	}
}
