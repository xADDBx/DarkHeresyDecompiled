using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitInScriptZone")]
[AllowMultipleComponents]
[TypeId("f12047e0588c11f4ba28423a0b8c3e8e")]
public class UnitInScriptZone : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator ScriptZone;

	protected override string GetConditionCaption()
	{
		return $"{Unit} is in {ScriptZone}";
	}

	protected override bool CheckCondition()
	{
		if (ScriptZone.GetValue() is ScriptZoneEntity scriptZoneEntity && scriptZoneEntity.ContainsUnit(Unit.GetValue()))
		{
			return true;
		}
		return false;
	}
}
