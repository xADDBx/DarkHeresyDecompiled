using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Facts;

[TypeId("7e85fcaa0f664874f8d214d73cff305b")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintUnitFact : BlueprintMechanicEntityFact
{
	[SerializeField]
	private bool m_AllowNonContextActions;

	public override bool AllowContextActionsOnly => !m_AllowNonContextActions;

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, BuffDuration duration, int rank = 1)
	{
		return new UnitFact(this, parentContext);
	}
}
