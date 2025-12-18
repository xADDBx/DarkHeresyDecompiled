using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[CreateAssetMenu(fileName = "CharacterAtlas", menuName = "Character System/CharacterAtlasData", order = 1)]
public class CharacterAtlasData : ScriptableObject
{
	[Serializable]
	public class BodyPartCoords
	{
		[Serializable]
		public class GpuCoords
		{
			public int x;

			public int y;

			public int z;

			public int w;

			public GpuCoords(int x, int y, int z, int w)
			{
				this.x = x;
				this.y = y;
				this.z = z;
				this.w = w;
			}
		}

		[SerializeField]
		public BodyPartType bodyPart;

		[SerializeField]
		public RectInt textureRectCoords;

		[SerializeField]
		public Color color = new Color(0f, 1f, 0f, 0.2f);

		[HideInInspector]
		public GpuCoords gpuCoords;
	}

	[SerializeField]
	private CharacterAtlasSize m_targetResolution = CharacterAtlasSize.Default;

	[SerializeField]
	public List<BodyPartCoords> BodyPartsCoords;

	public CharacterAtlasSize TargetResolution => m_targetResolution;

	private void OnValidate()
	{
		foreach (BodyPartCoords bodyPartsCoord in BodyPartsCoords)
		{
			if (bodyPartsCoord.textureRectCoords.x < 0)
			{
				bodyPartsCoord.textureRectCoords.x = 0;
			}
			if (bodyPartsCoord.textureRectCoords.y < 0)
			{
				bodyPartsCoord.textureRectCoords.y = 0;
			}
			if (bodyPartsCoord.textureRectCoords.width < 0)
			{
				bodyPartsCoord.textureRectCoords.width = 0;
			}
			if (bodyPartsCoord.textureRectCoords.height < 0)
			{
				bodyPartsCoord.textureRectCoords.height = 0;
			}
			bodyPartsCoord.gpuCoords = new BodyPartCoords.GpuCoords(bodyPartsCoord.textureRectCoords.x, (int)(m_targetResolution.Y - bodyPartsCoord.textureRectCoords.y - bodyPartsCoord.textureRectCoords.height), bodyPartsCoord.textureRectCoords.x + bodyPartsCoord.textureRectCoords.width, (int)(m_targetResolution.Y - bodyPartsCoord.textureRectCoords.y));
		}
	}
}
