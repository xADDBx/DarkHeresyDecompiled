using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintAbilityGroup))]
[ComponentName("VirtualAbilityGroup")]
[TypeId("d8003d4d60e58124a8814799c08d955b")]
public class VirtualAbilityGroupComponent : EntityFactComponentDelegate
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}
}
