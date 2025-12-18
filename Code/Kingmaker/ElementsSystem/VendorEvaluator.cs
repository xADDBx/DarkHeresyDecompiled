using System;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Obsolete]
[TypeId("c0912a1cfadd45d498ce4c80d2d48150")]
public class VendorEvaluator : GenericEvaluator<PartVendor>
{
	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_VendorEvaluator;

	public override string GetCaption()
	{
		return $"Торговец {m_VendorEvaluator}";
	}

	protected override PartVendor GetValueInternal()
	{
		return m_VendorEvaluator.GetValue().GetOptional<PartVendor>();
	}
}
