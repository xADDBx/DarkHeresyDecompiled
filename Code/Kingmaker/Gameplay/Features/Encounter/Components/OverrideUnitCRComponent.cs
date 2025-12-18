using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter.Components;

[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("Stats/OverrideUnitCRComponent")]
[TypeId("ea1857798ae84de39389023655160c33")]
public sealed class OverrideUnitCRComponent : BlueprintComponent
{
	[SerializeField]
	private int _overrideCRValue;

	public int OverrideCR => _overrideCRValue;
}
