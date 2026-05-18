using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class VendorConfig
{
	[field: SerializeField]
	public float DealPriceRatio { get; private set; } = 1.5f;

}
