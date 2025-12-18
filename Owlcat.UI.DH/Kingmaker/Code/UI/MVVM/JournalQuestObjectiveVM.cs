using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalQuestObjectiveVM : ViewModel
{
	public readonly QuestObjective Objective;

	public readonly List<JournalQuestObjectiveAddendumVM> Addendums;

	public readonly string Title;

	public readonly string Description;

	public readonly string Destination;

	public bool HasEtudeCounter;

	public int CurrentEtudeCounter;

	public int MinEtudeCounter;

	public string EtudeCounterDescription;

	public bool IsFailed;

	public bool IsCompleted;

	public bool IsPostponed;

	public ReactiveCommand<Unit> UpdateStatus = new ReactiveCommand<Unit>();

	public readonly int ObjectiveNumber;

	public bool IsAttention => Objective.NeedToAttention;

	public bool IsViewed => Objective.IsViewed;

	public bool CanBePinned
	{
		get
		{
			if (Objective.Blueprint.CanBePinned)
			{
				if (!IsFailed)
				{
					return !IsCompleted;
				}
				return false;
			}
			return false;
		}
	}

	public JournalQuestObjectiveVM(QuestObjective objective)
	{
		Objective = objective;
		BlueprintQuestObjective blueprintQuestObjective = objective?.Blueprint;
		Title = blueprintQuestObjective?.GetTitile();
		Description = blueprintQuestObjective?.GetDescription();
		string text = string.Empty;
		if (objective != null)
		{
			BlueprintQuestObjective blueprint = objective.Blueprint;
			if (blueprint != null && blueprint.Areas != null && objective.Blueprint.Areas.Any())
			{
				text = string.Join(", ", (from mapPoint in objective.Blueprint?.Areas?.Where((BlueprintArea mapPoint) => mapPoint != null)
					select mapPoint.Name) ?? Array.Empty<string>());
			}
		}
		Destination = ((!string.IsNullOrWhiteSpace(blueprintQuestObjective?.Destination)) ? ((string)blueprintQuestObjective?.Destination) : ((!string.IsNullOrWhiteSpace(text)) ? text : string.Empty));
		QuestObjective questObjective = objective;
		ObjectiveNumber = ((questObjective == null) ? null : (questObjective.Quest?.Objectives?.Where((QuestObjective o) => o.IsVisible && o.State != 0 && !o.Blueprint.IsAddendum && !o.Blueprint.IsErrandObjective).ToList().IndexOf(objective) + 1)).GetValueOrDefault();
		RumourMapMarker rumourMapMarker = blueprintQuestObjective?.GetComponent<RumourMapMarker>();
		if (rumourMapMarker != null)
		{
			Destination = string.Join(", ", from mapPoint in rumourMapMarker.SectorMapPointsToVisit.Dereference()
				select mapPoint.Name);
		}
		IOrderedEnumerable<QuestObjective> orderedEnumerable = (from b in objective?.Blueprint?.Addendums?.Where((BlueprintQuestObjective b) => b != null)
			select objective?.Quest?.TryGetObjective(b) into a
			where a != null
			where a.IsVisible
			orderby a?.Order descending
			select a);
		Addendums = new List<JournalQuestObjectiveAddendumVM>();
		if (orderedEnumerable != null)
		{
			foreach (QuestObjective item in orderedEnumerable)
			{
				Addendums.Add(new JournalQuestObjectiveAddendumVM(item));
			}
		}
		UpdateState();
		if (blueprintQuestObjective != null)
		{
			SetCounterEtude(blueprintQuestObjective);
		}
	}

	public void UpdateState()
	{
		QuestObjective objective = Objective;
		IsFailed = objective != null && objective.State == QuestObjectiveState.Failed;
		QuestObjective objective2 = Objective;
		IsCompleted = objective2 != null && objective2.State == QuestObjectiveState.Completed;
		QuestObjective objective3 = Objective;
		IsPostponed = objective3 != null && objective3.State == QuestObjectiveState.Postponed;
		UpdateStatus?.Execute();
		Addendums.ForEach(delegate(JournalQuestObjectiveAddendumVM a)
		{
			a.UpdateState();
		});
	}

	private void SetCounterEtude(BlueprintQuestObjective blueprint)
	{
		if (blueprint == null || !blueprint.UiCounterEtude || blueprint.BlueprintCounterEtude == null)
		{
			return;
		}
		Condition condition = blueprint.BlueprintCounterEtude?.Get()?.ActivationCondition?.Conditions?.FirstOrDefault((Condition c) => c is FlagInRange);
		HasEtudeCounter = condition != null;
		if (HasEtudeCounter && condition is FlagInRange flagInRange)
		{
			CurrentEtudeCounter = Mathf.Min(flagInRange.Flag.Value, flagInRange.MinValue);
			MinEtudeCounter = flagInRange.MinValue;
			if (blueprint.BlueprintCounterEtudeDescription != null)
			{
				EtudeCounterDescription = blueprint.BlueprintCounterEtudeDescription;
			}
		}
	}

	public void DoPin()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
		IEnumerable<BlueprintQuestObjective> addendums = from a in Addendums
			where !a.IsCompleted && !a.IsFailed
			select a.Addendum.Blueprint;
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleInfoRequest(new TooltipTemplateQuestObjective(Objective.Blueprint, addendums));
		});
	}
}
