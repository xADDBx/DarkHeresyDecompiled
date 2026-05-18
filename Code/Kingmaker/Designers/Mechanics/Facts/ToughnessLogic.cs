using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Add calculated hit points")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d32a32fa01d208c4bbf31462ec510fb3")]
public class ToughnessLogic : UnitFactComponentDelegate, IUnitGainPathRankHandler<EntitySubscriber>, IUnitGainPathRankHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitGainPathRankHandler, EntitySubscriber>
{
	protected override void OnActivateOrPostLoad()
	{
	}

	protected override void OnDeactivate()
	{
	}

	public void HandleUnitGainPathRank(BlueprintPath path)
	{
	}

	private void UpdateModifier()
	{
	}
}
