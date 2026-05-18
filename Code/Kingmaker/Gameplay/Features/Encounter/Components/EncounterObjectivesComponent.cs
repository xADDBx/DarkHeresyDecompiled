using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Features.Encounter.Components;

[Serializable]
[AllowedOn(typeof(BlueprintEncounter))]
[ComponentName("Combat/EncounterObjectivesComponent")]
[ClassInfoBox("Список доп. целей боя для отображения в UI.\nBinary — цель типа да/нет, отслеживается через ConditionsChecker.\nCounter — цель с числовым счётчиком (IntEvaluator).\nПустой ConditionsChecker считается невыполненным условием (переход не происходит).")]
[TypeId("eda9bab4b56c46e6ac6d66cb8b38e5f6")]
public sealed class EncounterObjectivesComponent : BlueprintComponent
{
	private const string InfoText = "Список доп. целей боя для отображения в UI.\nBinary — цель типа да/нет, отслеживается через ConditionsChecker.\nCounter — цель с числовым счётчиком (IntEvaluator).\nПустой ConditionsChecker считается невыполненным условием (переход не происходит).";

	public EncounterObjective[] Objectives = Array.Empty<EncounterObjective>();
}
