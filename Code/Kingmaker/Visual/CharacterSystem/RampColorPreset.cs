using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
public class RampColorPreset : ScriptableObject
{
	[Serializable]
	public class IndexSet
	{
		[SerializeField]
		public string Name = "";

		[SerializeField]
		public int PrimaryIndex;

		[SerializeField]
		public int SecondaryIndex;
	}

	[SerializeField]
	private ColorPresetFeatureFlag m_ColorPresetFlags;

	[SerializeField]
	public List<IndexSet> IndexPairs = new List<IndexSet>();

	public bool HasFlag(ColorPresetFeatureFlag flag)
	{
		return m_ColorPresetFlags.HasFlag(flag);
	}
}
