using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Blueprints.Root;

[Serializable]
[ComponentName("Root/PreciseAttackRoot")]
[TypeId("7e95115315874e0993dd3a5cdf4b54c2")]
public class PreciseAttackRoot : BlueprintScriptableObject
{
	[Serializable]
	public class PreciseAttackCameraOffsetEntry
	{
		public Size Size;

		public float HeightOffset;

		public float Zoom;

		public float ZoomTime;
	}

	[SerializeField]
	private PreciseAttackCameraOffsetEntry[] m_CameraOffsetBySize;

	public PreciseAttackCameraOffsetEntry GetCameraOffsetBySize(Size size)
	{
		return m_CameraOffsetBySize.FirstOrDefault((PreciseAttackCameraOffsetEntry entry) => entry.Size == size);
	}
}
