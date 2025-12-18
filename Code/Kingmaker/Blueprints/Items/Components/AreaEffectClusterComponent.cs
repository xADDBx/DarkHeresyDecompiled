using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAreaEffect))]
[TypeId("cb2b03882dc24aeb8fe64ff7fd35f8d8")]
public class AreaEffectClusterComponent : MechanicEntityFactComponentDelegate
{
	[InfoBox("Presence of AreaEffectClusterComponent overrides all other components. For overriden logic look at ClusterLogicBlueprint field")]
	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintAreaEffectClusterLogicReference m_ClusterLogicBlueprint;

	public BlueprintAreaEffectClusterLogic ClusterLogicBlueprint => m_ClusterLogicBlueprint.Get();
}
