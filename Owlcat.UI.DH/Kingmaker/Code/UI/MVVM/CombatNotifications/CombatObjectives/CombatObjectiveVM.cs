using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.CombatNotifications.CombatObjectives;

public sealed class CombatObjectiveVM : ViewModel
{
	private readonly ReactiveProperty<EncounterObjectiveState> m_State;

	private readonly ReactiveProperty<string> m_Value;

	private readonly ReactiveProperty<bool> m_IsHighlighted = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveCommand<(bool show, string text, RectTransform anchor)> m_ShowHint;

	private readonly LocalizedString m_DefaultCounterFormat;

	private readonly LocalizedString m_DefaultCounterWithTargetFormat;

	private readonly LocalizedString m_CounterFormat;

	private readonly EncounterObjectiveType m_Type;

	private int m_CurrentValue;

	private int? m_TargetValue;

	private readonly LocalizedString m_HintText;

	public LocalizedString Description { get; }

	public EncounterActiveObjectiveResolution ActiveStateResolution { get; }

	public ReadOnlyReactiveProperty<EncounterObjectiveState> State => m_State;

	public ReadOnlyReactiveProperty<string> Value => m_Value;

	public ReadOnlyReactiveProperty<bool> IsHighlighted => m_IsHighlighted;

	public ReadOnlyReactiveProperty<bool> IsActive { get; }

	public CombatObjectiveVM(EncounterObjectiveInfo info, ReactiveCommand<(bool show, string text, RectTransform anchor)> showHint)
	{
		m_ShowHint = showHint;
		Description = info.Description;
		m_HintText = info.Hint;
		m_Type = info.Type;
		ActiveStateResolution = info.ActiveStateResolution;
		m_DefaultCounterFormat = UIStrings.Instance.CombatObjectivesTexts.DefaultCounterFormat;
		m_DefaultCounterWithTargetFormat = UIStrings.Instance.CombatObjectivesTexts.DefaultCounterWithTargetFormat;
		m_CounterFormat = info.CounterFormat;
		m_CurrentValue = info.CurrentValue;
		m_TargetValue = info.TargetValue;
		m_State = new ReactiveProperty<EncounterObjectiveState>(info.State).AddTo(this);
		m_Value = new ReactiveProperty<string>(FormatValue()).AddTo(this);
		m_IsHighlighted.AddTo(this);
		IsActive = m_State.Select((EncounterObjectiveState s) => s != EncounterObjectiveState.Inactive).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
	}

	public void UpdateState(EncounterObjectiveState newState, out EncounterObjectiveState previousState)
	{
		previousState = m_State.Value;
		m_State.Value = newState;
		if (previousState == EncounterObjectiveState.Inactive && newState == EncounterObjectiveState.Active)
		{
			m_IsHighlighted.Value = true;
		}
		else if (newState != EncounterObjectiveState.Active)
		{
			m_IsHighlighted.Value = false;
		}
	}

	public void UpdateCounter(int current, int? target)
	{
		m_CurrentValue = current;
		m_TargetValue = target;
		m_Value.Value = FormatValue();
	}

	public void SetHighlighted(bool highlighted)
	{
		m_IsHighlighted.Value = highlighted;
	}

	public void ToggleHint(bool show, RectTransform anchor)
	{
		show = show && m_HintText != null && !m_HintText.IsEmpty();
		m_ShowHint.Execute((show, m_HintText, anchor));
	}

	private string FormatValue()
	{
		if (m_Type != EncounterObjectiveType.Counter)
		{
			return string.Empty;
		}
		GameLogContext.CurrentValue = m_CurrentValue;
		GameLogContext.TargetValue = m_TargetValue.GetValueOrDefault();
		if (m_CounterFormat != null && !m_CounterFormat.IsEmpty())
		{
			return m_CounterFormat.Text;
		}
		return m_TargetValue.HasValue ? m_DefaultCounterWithTargetFormat : m_DefaultCounterFormat;
	}
}
