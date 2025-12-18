using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("bd5e215abb372114fa3092696d5fee05")]
public class AddVendorItemsAction : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private VendorEvaluator m_VendorEvaluator;

	[SerializeField]
	private BlueprintUnitLootReference m_VendorTable;

	public override string GetCaption()
	{
		return $"Добавить {m_VendorTable.Get().NameSafe()} торговцу {m_VendorEvaluator}";
	}

	protected override void RunAction()
	{
	}
}
