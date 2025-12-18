using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UnitLogic.Parts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("61431ca715b94fe1adeec85a9330da29")]
public class AddMachineTrait : UnitFactComponentDelegate
{
	[SerializeField]
	private ContextValue m_Value = 0;

	protected override void OnActivateOrPostLoad()
	{
		PartMachineTrait orCreate = base.Owner.Parts.GetOrCreate<PartMachineTrait>();
		orCreate.Retain();
		orCreate.MachineTrait.AddModifier(m_Value.Calculate(base.Context), base.Runtime, ModifierDescriptor.UntypedUnstackable);
	}

	protected override void OnDeactivate()
	{
		PartMachineTrait orCreate = base.Owner.Parts.GetOrCreate<PartMachineTrait>();
		orCreate.Release();
		orCreate.MachineTrait.RemoveModifiersFrom(base.Runtime);
	}
}
