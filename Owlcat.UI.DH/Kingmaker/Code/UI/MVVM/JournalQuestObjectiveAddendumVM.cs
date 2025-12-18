using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.ElementsSystem;
using Kingmaker.Settings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalQuestObjectiveAddendumVM : ViewModel
{
	public readonly QuestObjective Addendum;

	public readonly string Description;

	public readonly string Destination;

	public bool IsFailed;

	public bool IsCompleted;

	public bool IsPostponed;

	public readonly ReactiveCommand<Unit> UpdateStatus = new ReactiveCommand<Unit>();

	public readonly float FontMultiplier = FontSizeMultiplier;

	public bool HasEtudeCounter;

	public int CurrentEtudeCounter;

	public int MinEtudeCounter;

	public string EtudeCounterDescription;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public bool IsAttention => Addendum.NeedToAttention;

	public bool IsViewed => Addendum.IsViewed;

	public JournalQuestObjectiveAddendumVM(QuestObjective addendum)
	{
		Addendum = addendum;
		Description = Addendum?.Blueprint?.GetDescription();
		string text = string.Empty;
		QuestObjective addendum2 = Addendum;
		if (addendum2 != null)
		{
			BlueprintQuestObjective blueprint = addendum2.Blueprint;
			if (blueprint != null && blueprint.Areas != null && Addendum.Blueprint.Areas.Any())
			{
				text = string.Join(", ", from mapPoint in Addendum.Blueprint.Areas
					where mapPoint != null
					select mapPoint.Name);
			}
		}
		Destination = ((!string.IsNullOrWhiteSpace(Addendum?.Blueprint?.Destination)) ? ((string)Addendum?.Blueprint?.Destination) : ((!string.IsNullOrWhiteSpace(text)) ? text : string.Empty));
		UpdateState();
		BlueprintQuestObjective blueprintQuestObjective = addendum?.Blueprint;
		if (blueprintQuestObjective != null)
		{
			SetCounterEtude(blueprintQuestObjective);
		}
	}

	public void UpdateState()
	{
		QuestObjective addendum = Addendum;
		IsFailed = addendum != null && addendum.State == QuestObjectiveState.Failed;
		QuestObjective addendum2 = Addendum;
		IsCompleted = addendum2 != null && addendum2.State == QuestObjectiveState.Completed;
		QuestObjective addendum3 = Addendum;
		IsPostponed = addendum3 != null && addendum3.State == QuestObjectiveState.Postponed;
		UpdateStatus?.Execute();
	}

	private void SetCounterEtude(BlueprintQuestObjective blueprint)
	{
		if (!blueprint.UiCounterEtude || blueprint.BlueprintCounterEtude == null)
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
}
