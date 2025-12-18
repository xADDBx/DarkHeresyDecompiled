using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("35d0c0936813454197973cab07403552")]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class AbstractFamiliarEvaluator : AbstractUnitEvaluator, IOwlPackable<AbstractFamiliarEvaluator>
{
	protected abstract BaseUnitEntity Leader { get; }

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Leader?.GetFamiliarLeaderOptional()?.FirstFamiliar;
	}
}
