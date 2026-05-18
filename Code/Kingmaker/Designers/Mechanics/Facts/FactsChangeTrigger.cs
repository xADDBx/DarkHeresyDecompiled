using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("8082db8243913b948a4baf54c9ab1787")]
public class FactsChangeTrigger : UnitFactComponentDelegate, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>
{
	[SerializeField]
	private BlueprintUnitFactReference[] m_CheckedFacts;

	[SerializeField]
	public ActionList OnFactGainedActions;

	[SerializeField]
	public ActionList OnFactLostActions;

	public ReferenceArrayProxy<BlueprintUnitFact> CheckedFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] checkedFacts = m_CheckedFacts;
			return checkedFacts;
		}
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact.Owner is BaseUnitEntity baseUnitEntity && baseUnitEntity == base.Owner && OnFactGainedActions.HasActions && (!CheckedFacts.Any() || CheckedFacts.Contains(fact.Blueprint)))
		{
			base.Fact.RunActionInContext(OnFactGainedActions, base.Owner);
		}
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		if (fact.Owner is BaseUnitEntity baseUnitEntity && baseUnitEntity == base.Owner && OnFactLostActions.HasActions && (!CheckedFacts.Any() || CheckedFacts.Contains(fact.Blueprint)))
		{
			base.Fact.RunActionInContext(OnFactLostActions, base.Owner);
		}
	}
}
