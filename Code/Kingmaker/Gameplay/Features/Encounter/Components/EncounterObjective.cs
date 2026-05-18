using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter.Components;

[Serializable]
public sealed class EncounterObjective
{
	public string Comment;

	public LocalizedString Description;

	public LocalizedString Hint;

	[Tooltip("Цель активна со старта боя?")]
	public bool ActiveFromStart = true;

	[Tooltip("Тип цели: Binary (да/нет) или Counter (числовой счётчик).")]
	public EncounterObjectiveType Type;

	[ShowIf("IsCounter")]
	[SerializeReference]
	[Tooltip("Текущее значение счётчика.")]
	public IntEvaluator CurrentValue;

	[ShowIf("IsCounter")]
	[SerializeReference]
	[Tooltip("Целевое значение счётчика (знаменатель вида '3 из 4'). Может быть пустым.")]
	public IntEvaluator TargetValue;

	[ShowIf("IsCounter")]
	[Tooltip("Формат отображения значений в UI. Может быть пустым.")]
	[InfoBox("Кастомное отображение счетчика. Пример: 1 Round Left. \n{0} подставляет CurrentValue, {1} - TargetValue.\n Может быть пустым")]
	[CanBeNull]
	public LocalizedString CounterFormat;

	[Tooltip("Условия перехода в Active (используется если InitialState == Inactive). Пустой — переход не происходит.")]
	[HideIf("ActiveFromStart")]
	public ConditionsChecker ActivationCondition = new ConditionsChecker();

	[Tooltip("Условия перехода в Inactive (используется если InitialState == Active). Пустой — переход не происходит.")]
	public ConditionsChecker DeactivationCondition = new ConditionsChecker();

	[Tooltip("Условия выполнения цели (→ Complete). Пустой — переход не происходит.")]
	public ConditionsChecker CompletionCondition = new ConditionsChecker();

	[Tooltip("Условия провала цели (→ Failed). Пустой — переход не происходит.")]
	public ConditionsChecker FailureCondition = new ConditionsChecker();

	[Tooltip("Можно выйти из состояния Complete/Failed? Если да - условия для перехода в состояние Active должны быть указаны явно.")]
	public bool CanExitFromFinalState;

	[Tooltip("В какое состояние перейти, если Active на конец боя")]
	public EncounterActiveObjectiveResolution ActiveStateResolution;

	private bool IsCounter => Type == EncounterObjectiveType.Counter;
}
