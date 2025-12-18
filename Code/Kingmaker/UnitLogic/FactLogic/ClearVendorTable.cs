using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("887885018edbbb3468ac6ef71de67d57")]
public class ClearVendorTable : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintSharedVendorTableReference m_Table;

	public BlueprintSharedVendorTable Table => m_Table;

	public override string GetCaption()
	{
		return $"Clear vendor table: {Table}";
	}

	protected override void RunAction()
	{
	}
}
