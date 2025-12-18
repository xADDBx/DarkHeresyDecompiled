using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("018ffdf3c815bfa4189c0fc4cf1c9b19")]
public class RecalculateOnStatChange : UnitFactComponentDelegate
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public ModifiableValue AppliedToStat;
	}

	public StatType Stat;

	public bool CheckCaster;

	protected override void OnActivateOrPostLoad()
	{
		StatType stat = Stat;
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.AppliedToStat = ((!CheckCaster) ? base.Owner.Stats.GetStat(stat) : (base.Context.MaybeCaster?.GetStatOptional(stat) ?? null));
		if (componentData.AppliedToStat != null)
		{
			componentData.AppliedToStat.AddDependentFact(base.Fact);
		}
	}

	protected override void OnDeactivate()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.AppliedToStat?.RemoveDependentFact(base.Fact);
		componentData.AppliedToStat = null;
	}
}
