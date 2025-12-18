using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components.Features;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("66343ffd992f4451aeaaa567c3c305b8")]
public class ArmourTagUISettings : BlueprintComponent
{
	[SerializeField]
	private ArmourTagPropertySelector m_Selector;

	public ArmourTagProperty Tag => m_Selector.PropertyTag;

	public PropertyType Type => m_Selector.PropertyType;
}
