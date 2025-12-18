using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class DigitalColors
{
	[field: SerializeField]
	public Color32 DigitalNarratorColor { get; private set; } = new Color32(144, 189, 161, byte.MaxValue);

}
