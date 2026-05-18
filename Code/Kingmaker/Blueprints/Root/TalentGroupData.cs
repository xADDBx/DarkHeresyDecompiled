using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class TalentGroupData
{
	[field: SerializeField]
	public Sprite Icon { get; private set; }

	[field: SerializeField]
	public Color BgrColor { get; private set; }
}
