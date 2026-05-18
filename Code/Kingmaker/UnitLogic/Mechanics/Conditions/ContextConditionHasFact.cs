using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Framework.ContextContract;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("9706de75454abeb48bd4cfa7f526a1c2")]
[ReadsContext(new ContextField[] { ContextField.Target })]
public class ContextConditionHasFact : ContextCondition
{
	[FormerlySerializedAs("Feature")]
	[SerializeField]
	[FormerlySerializedAs("Fact")]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override string GetConditionCaption()
	{
		return "Check if target has " + Fact.NameSafe();
	}

	protected override bool CheckCondition()
	{
		if (base.Target.Entity != null)
		{
			return base.Target.Entity.Facts.Contains(Fact);
		}
		return false;
	}
}
