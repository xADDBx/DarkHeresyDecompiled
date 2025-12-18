using Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;

public abstract class ComponentBase : ScriptableObject
{
	[HideInInspector]
	public bool Active = true;

	public string DisplayName { get; protected set; } = "";


	public abstract ControllerBase CreateController();
}
