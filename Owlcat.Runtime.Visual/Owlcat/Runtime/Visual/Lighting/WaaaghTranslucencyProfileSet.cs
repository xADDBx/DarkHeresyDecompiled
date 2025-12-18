using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Lighting;

[CreateAssetMenu(menuName = "Owlcat/Translucency Profile Set")]
public class WaaaghTranslucencyProfileSet : ScriptableObject
{
	[Serializable]
	public struct TranslucencyProfile
	{
		public string Name;

		public Color Color;

		public SelfShadowingWorkaroundData SelfShadowingWorkaround;
	}

	[Serializable]
	public struct SelfShadowingWorkaroundData
	{
		[Range(0f, 0.5f)]
		public float DepthBias;

		[Range(0f, 0.5f)]
		public float NormalBias;

		[Range(0f, 0.5f)]
		public float DepthBiasPointSpot;

		[Range(0f, 0.5f)]
		public float NormalBiasPointSpot;
	}

	public const int kMaxCount = 16;

	public const string kFileName = "TranslucencyProfileSet";

	[CanBeNull]
	private static WaaaghTranslucencyProfileSet s_Instance;

	[SerializeField]
	private List<TranslucencyProfile> m_Profiles = new List<TranslucencyProfile>();

	public List<TranslucencyProfile> Profiles => m_Profiles;

	[CanBeNull]
	public static WaaaghTranslucencyProfileSet Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = Resources.Load<WaaaghTranslucencyProfileSet>("TranslucencyProfileSet");
			}
			return s_Instance;
		}
	}

	private void OnValidate()
	{
		if (m_Profiles.Count > 16)
		{
			Debug.LogError($"Translucency Profile Set may contain no more than {16} profiles. Deleting all profiles over the limit...", this);
			while (m_Profiles.Count > 16)
			{
				m_Profiles.RemoveAt(m_Profiles.Count - 1);
			}
		}
	}
}
