using System;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem;

[Serializable]
[CreateAssetMenu(menuName = "Character System/Vertex Color Mask")]
public class VertexColorMask : ScriptableObject
{
	[SerializeField]
	private string m_Name;

	[SerializeField]
	private Color m_MaskColor;

	public string Name
	{
		get
		{
			if (!base.name.StartsWith("VCM_"))
			{
				return base.name;
			}
			return base.name.Substring(4);
		}
	}

	public Color MaskColor => m_MaskColor;
}
