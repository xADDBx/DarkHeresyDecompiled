using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Add ability resources")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("fd82ca085bd60c04fb03d1091acc66cb")]
public class AddAbilityResources : UnitFactComponentDelegate, IUnitReapplyFeaturesOnLevelUpHandler<EntitySubscriber>, IUnitReapplyFeaturesOnLevelUpHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitReapplyFeaturesOnLevelUpHandler, EntitySubscriber>
{
	public bool UseThisAsResource;

	[HideIf("UseThisAsResource")]
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	[ShowIf("UseThisAsResource")]
	public int Amount;

	public bool RestoreAmount;

	public bool RestoreOnLevelUp;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	protected override void OnActivate()
	{
		BlueprintScriptableObject blueprint = (UseThisAsResource ? ((BlueprintScriptableObject)base.Fact.Blueprint) : ((BlueprintScriptableObject)Resource));
		if (!base.IsReapplying)
		{
			base.Owner.AbilityResources.Add(blueprint, RestoreAmount);
		}
	}

	protected override void OnDeactivate()
	{
		BlueprintScriptableObject blueprint = (UseThisAsResource ? ((BlueprintScriptableObject)base.Fact.Blueprint) : ((BlueprintScriptableObject)Resource));
		if (!base.IsReapplying)
		{
			base.Owner.AbilityResources.Remove(blueprint);
		}
	}

	public void HandleUnitReapplyFeaturesOnLevelUp()
	{
		if (RestoreOnLevelUp)
		{
			BlueprintScriptableObject blueprint = (UseThisAsResource ? ((BlueprintScriptableObject)base.Fact.Blueprint) : ((BlueprintScriptableObject)Resource));
			base.Owner.AbilityResources.Restore(blueprint);
		}
	}
}
