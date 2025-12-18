using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.EntityBlackboard;

[AllowedOn(typeof(BlueprintEncounter))]
[TypeId("b056cad9628e9424c81ed068decb65f7")]
public class EntityBlackboardComponent : BlueprintComponent
{
	[SerializeField]
	private BlackboardVariablesList m_Variables = new BlackboardVariablesList();

	public BlackboardVariablesList Variables => m_Variables;
}
