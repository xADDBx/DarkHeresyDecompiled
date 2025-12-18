using System;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Interaction;

[Serializable]
public class InteractionIconEntry
{
	[field: SerializeField]
	public UIInteractionType Type { get; private set; }

	[field: SerializeField]
	public Sprite Icon { get; private set; }
}
