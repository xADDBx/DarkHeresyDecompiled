using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints.Root;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem.State;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("df78945bb9f434e40b897758499cb714")]
public class BlueprintAnswer : BlueprintAnswerBase, ILocalizedStringHolder, IAlignmentShiftProvider, IPossibleDialogEnding
{
	[Header("Text")]
	[Tooltip("Text of answer.")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.ByProperty)]
	public LocalizedString Text;

	[Tooltip("Link to next BlueprintCue or BlueprintCheck(for skill check)")]
	public CueSelection NextCue;

	[Tooltip("When FALSE answer will be hidden in history of dialog.")]
	public bool AddToHistory = true;

	public bool ShowOnce;

	[ShowIf("ShowOnce")]
	public bool ShowOnceCurrentDialog;

	[Header("Requirements")]
	[Space(20f)]
	[Tooltip("Answer will be showed only if skill check passed. The check occurs immediately, before the response is rendered.")]
	public bool HasSkillCheckRequirement;

	[ShowIf("HasSkillCheckRequirement")]
	public ShowCheck ShowCheck;

	[ShowIf("HasSkillCheckRequirement")]
	public ActionList OnCheckSuccess;

	[ShowIf("HasSkillCheckRequirement")]
	public ActionList OnCheckFail;

	[Space(20f)]
	public ConditionsChecker ShowConditions;

	public ConditionsChecker SelectConditions;

	[Space(10f)]
	[Tooltip("Show this answer only if it is followed by a valid cue.")]
	public bool RequireValidCue;

	[FormerlySerializedAs("ConvictionShift")]
	[Header("Actions")]
	public ActionList OnSelect;

	[Header("Character selection")]
	[Tooltip("Check character selection settings.")]
	public CharacterSelection CharacterSelection;

	[Header("Description")]
	public LocalizedString Description;

	[SerializeField]
	[KDB("Set it to true, if dialogue can end on this Cue and it's a valid ending \n For example: dialog should break with cutscene and continue after that")]
	private bool IsPossibleDialogEnding;

	public bool DebugMode;

	public static Func<BaseUnitEntity, BlueprintAnswer, SkillCheckResult> OverrideShowCheckResult;

	public LocalizedString LocalizedStringText => Text;

	public string DisplayText => Text;

	public bool CapitalPartyChecksEnabled
	{
		get
		{
			CharacterSelection characterSelection = CharacterSelection;
			if (characterSelection == null || characterSelection.SelectionType != CharacterSelection.Type.Capital)
			{
				CharacterSelection characterSelection2 = CharacterSelection;
				if (characterSelection2 != null && characterSelection2.SelectionType == CharacterSelection.Type.Clear)
				{
					return Game.Instance.Player.CapitalPartyMode;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasExchangeData => HasExchangeDataOnSelect();

	public bool HasConditions => HasConditionsOnSelect();

	public IEnumerable<AlignmentShift> AlignmentShifts => from AddAlignmentRank a in OnSelect.Actions.Where((GameAction a) => a is AddAlignmentRank)
		select a.Shift;

	public AlignmentAxis AlignmentShiftAxis
	{
		get
		{
			if (!AlignmentShifts.Any())
			{
				return AlignmentAxis.None;
			}
			return AlignmentShifts.First().Axis;
		}
	}

	public int AlignmentShiftRank
	{
		get
		{
			if (!AlignmentShifts.Any())
			{
				return 0;
			}
			return AlignmentShifts.First().Value;
		}
	}

	public bool IsPossibleEnding => IsPossibleDialogEnding;

	public List<SkillCheckDC> CollectNextCueSkillChecks()
	{
		List<SkillCheckDC> list = new List<SkillCheckDC>();
		BaseUnitEntity baseUnitEntity = CharacterSelection.SelectUnit(this, Game.Instance.Player.MainCharacterEntity);
		foreach (BlueprintCueBase item in NextCue.Cues.Dereference())
		{
			if (!(item is BlueprintCheck blueprintCheck) || !blueprintCheck.Conditions.Check() || !blueprintCheck.CanShow() || blueprintCheck.Hidden)
			{
				continue;
			}
			baseUnitEntity = blueprintCheck?.GetTargetUnit() ?? CharacterSelection.SelectUnit(this, Game.Instance.Player.MainCharacterEntity);
			try
			{
				if (baseUnitEntity != null)
				{
					RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(baseUnitEntity, blueprintCheck.Type, blueprintCheck.GetDC());
					list.Add(new SkillCheckDC(baseUnitEntity, rulePerformSkillCheck.StatType, rulePerformSkillCheck.Difficulty, rulePerformSkillCheck.StatValue, isBest: false, null));
				}
				else
				{
					RulePerformPartySkillCheck rulePerformPartySkillCheck = new RulePerformPartySkillCheck(blueprintCheck.Type, blueprintCheck.GetDC(), CapitalPartyChecksEnabled);
					rulePerformPartySkillCheck.Calculate(isTrigger: false, doCheck: false);
					list.Add(new SkillCheckDC(rulePerformPartySkillCheck.Roller, rulePerformPartySkillCheck.StatType, rulePerformPartySkillCheck.Difficulty, rulePerformPartySkillCheck.StatValue, isBest: true, null));
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Can't calculate Rule: {arg}");
				list.Add(new SkillCheckDC(baseUnitEntity, blueprintCheck.Type, blueprintCheck.GetDC(), 0, isBest: true, null));
			}
		}
		return list;
	}

	public List<SkillCheckDC> CollectRequiredSkillChecks()
	{
		List<SkillCheckDC> list = new List<SkillCheckDC>();
		BaseUnitEntity baseUnitEntity = CharacterSelection.SelectUnit(this, Game.Instance.Player.MainCharacterEntity);
		if (HasSkillCheckRequirement && ShowCheck.Type != 0)
		{
			try
			{
				if (baseUnitEntity != null)
				{
					RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(baseUnitEntity, ShowCheck.Type, ShowCheck.GetDC());
					list.Add(new SkillCheckDC(baseUnitEntity, rulePerformSkillCheck.StatType, rulePerformSkillCheck.Difficulty, rulePerformSkillCheck.StatValue, isBest: true, true));
				}
				else
				{
					RulePerformPartySkillCheck rulePerformPartySkillCheck = new RulePerformPartySkillCheck(ShowCheck.Type, ShowCheck.GetDC(), CapitalPartyChecksEnabled);
					rulePerformPartySkillCheck.Calculate(isTrigger: false, doCheck: false);
					list.Add(new SkillCheckDC(rulePerformPartySkillCheck.Roller, rulePerformPartySkillCheck.StatType, rulePerformPartySkillCheck.ResultDC, rulePerformPartySkillCheck.StatValue, isBest: true, true));
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Can't calculate Rule: {arg}");
				list.Add(new SkillCheckDC(baseUnitEntity, ShowCheck.Type, ShowCheck.GetDC(), 0, isBest: true, null));
			}
		}
		return list;
	}

	public bool CanShow()
	{
		DialogState dialogState = Game.Instance.DialogState;
		DialogController dialogController = Game.Instance.Controllers.DialogController;
		DialogDebug.Clear(this);
		if (DebugMode && !Debug.isDebugBuild)
		{
			DialogDebug.Add(this, "not in debug build", Color.red);
			return false;
		}
		if (ShowOnce)
		{
			if (ShowOnceCurrentDialog)
			{
				if (dialogController.LocalSelectedAnswers.Contains(this))
				{
					DialogDebug.Add(this, "(show once) was selected before (in current dialog)", Color.red);
					return false;
				}
			}
			else if (dialogState.SelectedAnswersContains(this))
			{
				DialogDebug.Add(this, "(show once) was selected before (global)", Color.red);
				return false;
			}
		}
		if (HasSkillCheckRequirement && ShowCheck.Type != 0)
		{
			if (dialogState.AnswerChecksTryGetValue(this, out var res))
			{
				if (res == CheckResult.Failed)
				{
					DialogDebug.Add(this, "check failed (before)", Color.red);
					return false;
				}
			}
			else
			{
				SkillCheckResult skillCheckResult = OverrideShowCheckResult?.Invoke(null, this);
				bool flag;
				if (skillCheckResult != null)
				{
					_ = skillCheckResult.Passed;
					flag = skillCheckResult.Passed;
				}
				else
				{
					RulePerformPartySkillCheck rulePerformPartySkillCheck = new RulePerformPartySkillCheck(ShowCheck.Type, ShowCheck.GetDC(), CapitalPartyChecksEnabled);
					flag = Rulebook.Trigger(rulePerformPartySkillCheck).Success;
					Metrics.SkillCheck.Type(SkillCheckMetricsEvent.Types.ShowAnswer).Initiator(rulePerformPartySkillCheck.Roller.Blueprint.AssetGuid).Target(AssetGuid)
						.Result(flag)
						.Send();
				}
				dialogState.AnswerChecksAdd(this, (!flag) ? CheckResult.Failed : CheckResult.Passed);
				if (flag)
				{
					OnCheckSuccess?.Run();
				}
				else
				{
					OnCheckFail?.Run();
				}
				if (!flag)
				{
					DialogDebug.Add(this, "check failed", Color.red);
					return false;
				}
			}
		}
		if (RequireValidCue && NextCue.Select() == null)
		{
			DialogDebug.Add(this, "no valid cue following", Color.red);
			return false;
		}
		ForcedConditionsState forcedCondition = DialogDebugRoot.Instance.GetForcedCondition(this);
		if (forcedCondition != 0)
		{
			DialogDebug.Add(this, $"condtions forced to {forcedCondition == ForcedConditionsState.ForceTrue}", Color.magenta);
			return forcedCondition == ForcedConditionsState.ForceTrue;
		}
		if (!ShowConditions.Check(this))
		{
			DialogDebug.Add(this, "conditions failed", Color.red);
			return false;
		}
		DialogDebug.Add(this, "answer shown", Color.green);
		return true;
	}

	public bool CanSelect()
	{
		return SelectConditions.Check();
	}

	private bool HasExchangeDataOnSelect()
	{
		List<Type> exchangeActions = new List<Type>
		{
			typeof(AddItemToPlayer),
			typeof(RemoveItemFromPlayer)
		};
		return ExpandConditionals(OnSelect.Actions.ToList()).Any((GameAction a) => exchangeActions.Contains(a.GetType()));
	}

	private List<GameAction> ExpandConditionals(List<GameAction> initActions)
	{
		List<Conditional> list = initActions.Where((GameAction a) => a is Conditional).Cast<Conditional>().ToList();
		if (!list.Any())
		{
			return initActions;
		}
		initActions = initActions.Except(list).ToList();
		foreach (Conditional item in list)
		{
			initActions.AddRange(item.ConditionsChecker.Check() ? item.IfTrue.Actions : item.IfFalse.Actions);
		}
		return ExpandConditionals(initActions);
	}

	private bool HasConditionsOnSelect()
	{
		List<Type> contextConditions = new List<Type>
		{
			typeof(ContextConditionHasItem),
			typeof(ItemsEnough),
			typeof(CheckCaseStatus)
		};
		if (SelectConditions.HasConditions)
		{
			return SelectConditions.Conditions.Any((Condition c) => contextConditions.Contains(c.GetType()));
		}
		return false;
	}
}
