using Kingmaker.Blueprints.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[AllowMultipleComponents]
[TypeId("3519a736d2aa5cf46a07a5b5b8abf4a1")]
public class ComponentsList : BlueprintComponent
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("List")]
	private BlueprintComponentListReference m_List;

	public BlueprintComponentList List => m_List?.Get();
}
