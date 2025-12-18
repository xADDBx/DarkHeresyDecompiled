using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components.Features;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("a97390795b894aadab9845d6db033ee8")]
public class WeaponTagUISettings : BlueprintComponent
{
	[SerializeField]
	private WeaponTagPropertySelector m_Selector;

	public WeaponTagProperty Tag => m_Selector.PropertyTag;

	public PropertyType Type => m_Selector.PropertyType;
}
