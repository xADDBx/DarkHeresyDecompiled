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
		UpdateModifier();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Health.HitPoints.RemoveModifiersFrom(base.Runtime);
	}

	public void HandleUnitGainPathRank(BlueprintPath path)
	{
		UpdateModifier();
	}

	private void UpdateModifier()
	{
		base.Owner.Health.HitPoints.RemoveModifiersFrom(base.Runtime);
		int value = (base.Owner.Progression.CharacterLevel + 1) / 2;
		base.Owner.Health.HitPoints.AddModifier(value, base.Runtime);
	}
}
