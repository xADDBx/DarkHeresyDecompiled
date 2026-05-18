using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.CombatNotifications.CombatObjectives;

public sealed class CombatObjectivesVM : ViewModel, IEncounterObjectiveHandler, ISubscriber, ICombatEndHandler
{
	private readonly List<CombatObjectiveVM> m_ObjectiveVMs;

	private readonly Subject<Unit> m_ObjectiveActivated = new Subject<Unit>();

	private readonly ReactiveProperty<bool> m_HasActiveObjectives;

	private readonly ReactiveCommand<(bool show, string text, RectTransform anchor)> m_ToggleHint;

	public readonly string TitleText;

	public readonly string ShowDirectivesHintText;

	public readonly string HideDirectivesHintText;

	public IReadOnlyList<CombatObjectiveVM> ObjectiveVMs => m_ObjectiveVMs;

	public Observable<Unit> ObjectiveActivated => m_ObjectiveActivated;

	public ReadOnlyReactiveProperty<bool> IsVisible { get; }

	public Observable<(bool show, string text, RectTransform anchor)> ToggleHint => m_ToggleHint;

	public CombatObjectivesVM(ActiveEncounter encounter, ReadOnlyReactiveProperty<bool> isForceHidden)
	{
		m_ToggleHint = new ReactiveCommand<(bool, string, RectTransform)>().AddTo(this);
		TitleText = UIStrings.Instance.CombatObjectivesTexts.ObjectivesTitle;
		ShowDirectivesHintText = UIStrings.Instance.CombatObjectivesTexts.ShowDirectivesHint;
		HideDirectivesHintText = UIStrings.Instance.CombatObjectivesTexts.HideDirectivesHint;
		PartEncounterObjectives partEncounterObjectives = encounter?.GetOptional<PartEncounterObjectives>();
		m_ObjectiveVMs = new List<CombatObjectiveVM>();
		if (partEncounterObjectives != null)
		{
			foreach (EncounterObjectiveInfo objective in partEncounterObjectives.GetObjectives())
			{
				CombatObjectiveVM combatObjectiveVM = new CombatObjectiveVM(objective, m_ToggleHint);
				combatObjectiveVM.AddTo(this);
				m_ObjectiveVMs.Add(combatObjectiveVM);
			}
		}
		m_HasActiveObjectives = new ReactiveProperty<bool>(HasAnyActive()).AddTo(this);
		m_ObjectiveActivated.AddTo(this);
		IsVisible = m_HasActiveObjectives.CombineLatest(isForceHidden, (bool hasObjectives, bool isHidden) => hasObjectives && !isHidden).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	void IEncounterObjectiveHandler.HandleObjectiveStateChanged(int objectiveIndex, EncounterObjectiveState newState)
	{
		if (objectiveIndex >= 0 && objectiveIndex < m_ObjectiveVMs.Count)
		{
			m_ObjectiveVMs[objectiveIndex].UpdateState(newState, out var previousState);
			m_HasActiveObjectives.Value = HasAnyActive();
			if (previousState == EncounterObjectiveState.Inactive && newState != 0)
			{
				m_ObjectiveActivated.OnNext(Unit.Default);
			}
		}
	}

	void IEncounterObjectiveHandler.HandleObjectiveCounterChanged(int objectiveIndex, int newValue, int? targetValue)
	{
		if (objectiveIndex >= 0 && objectiveIndex < m_ObjectiveVMs.Count)
		{
			m_ObjectiveVMs[objectiveIndex].UpdateCounter(newValue, targetValue);
		}
	}

	void ICombatEndHandler.HandleCombatEnd(EncounterCompletionType reason)
	{
		for (int i = 0; i < m_ObjectiveVMs.Count; i++)
		{
			CombatObjectiveVM combatObjectiveVM = m_ObjectiveVMs[i];
			if (combatObjectiveVM.State.CurrentValue == EncounterObjectiveState.Active)
			{
				EncounterObjectiveState previousState = combatObjectiveVM.ActiveStateResolution switch
				{
					EncounterActiveObjectiveResolution.AutoFail => EncounterObjectiveState.Failed, 
					EncounterActiveObjectiveResolution.AutoComplete => EncounterObjectiveState.Completed, 
					EncounterActiveObjectiveResolution.AutoHide => EncounterObjectiveState.Inactive, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
				EncounterObjectiveState newState = previousState;
				combatObjectiveVM.UpdateState(newState, out previousState);
			}
		}
	}

	private bool HasAnyActive()
	{
		for (int i = 0; i < m_ObjectiveVMs.Count; i++)
		{
			if (m_ObjectiveVMs[i].State.CurrentValue != 0)
			{
				return true;
			}
		}
		return false;
	}
}
