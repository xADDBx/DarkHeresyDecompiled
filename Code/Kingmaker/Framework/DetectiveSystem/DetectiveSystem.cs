using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Networking.Serialization;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Fmw.Blueprints;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[OwlPackable(OwlPackableMode.Generate)]
public class DetectiveSystem : Entity, IHashable, IOwlPackable<DetectiveSystem>
{
	public readonly struct CaseDisplayData
	{
		public readonly string Name;

		public readonly string Description;

		public readonly Sprite? Icon;

		public CaseDisplayData(string name, string description, Sprite? icon)
		{
			Name = name;
			Description = description;
			Icon = icon;
		}
	}

	[OwlPackInclude]
	private readonly HashSet<BlueprintClueStudy> m_CluesStudyWithUnlockedCondition = new HashSet<BlueprintClueStudy>();

	public const string ID = "detective-system-id";

	public new static readonly EntityRef<DetectiveSystem> Ref = new EntityRef<DetectiveSystem>("detective-system-id");

	[OwlPackInclude]
	[GameStateInclude]
	private readonly Dictionary<BlueprintCase, CaseState> m_Cases = new Dictionary<BlueprintCase, CaseState>();

	[OwlPackInclude]
	[GameStateInclude]
	private readonly Dictionary<BlueprintClue, ClueState> m_Clues = new Dictionary<BlueprintClue, ClueState>();

	[OwlPackInclude]
	[GameStateInclude]
	private readonly Dictionary<BlueprintClueAddendum, AddendumState> m_Addendums = new Dictionary<BlueprintClueAddendum, AddendumState>();

	[OwlPackInclude]
	[GameStateInclude]
	private readonly Dictionary<BlueprintConclusion, ConclusionState> m_Conclusions = new Dictionary<BlueprintConclusion, ConclusionState>();

	[OwlPackInclude]
	[GameStateInclude]
	private readonly HashSet<BlueprintClueStudy> m_Studies = new HashSet<BlueprintClueStudy>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectiveSystem",
		OldNames = null,
		Fields = new FieldInfo[16]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_CluesStudyWithUnlockedCondition", typeof(HashSet<BlueprintClueStudy>)),
			new FieldInfo("m_Cases", typeof(Dictionary<BlueprintCase, CaseState>)),
			new FieldInfo("m_Clues", typeof(Dictionary<BlueprintClue, ClueState>)),
			new FieldInfo("m_Addendums", typeof(Dictionary<BlueprintClueAddendum, AddendumState>)),
			new FieldInfo("m_Conclusions", typeof(Dictionary<BlueprintConclusion, ConclusionState>)),
			new FieldInfo("m_Studies", typeof(HashSet<BlueprintClueStudy>))
		}
	};

	[Cheat(Name = "ds_open_case")]
	public static void Cheat_OpenCase(BlueprintCase blueprint)
	{
		Game.Instance.DetectiveSystem.OpenCase(blueprint);
		Game.Instance.DetectiveSystem.GetState(blueprint).OpenedByCheats = true;
	}

	[Cheat(Name = "ds_open_case_whole")]
	public static void Cheat_OpenCaseWhole(BlueprintCase blueprint)
	{
		Cheat_OpenCase(blueprint);
		BpRef<BlueprintClue>[] clues = blueprint.Clues;
		for (int i = 0; i < clues.Length; i++)
		{
			Cheat_AddClue(clues[i], withAllAddendums: true);
		}
	}

	[Cheat(Name = "ds_reopen_case")]
	public static void Cheat_ReopenCase(BlueprintCase blueprint)
	{
		Game.Instance.DetectiveSystem.ReopenCase(blueprint);
	}

	[Obsolete("New Question/Answer approach, WIP")]
	[Cheat(Name = "ds_close_case")]
	public static void Cheat_CloseCase(BlueprintCase blueprint)
	{
		Game.Instance.DetectiveSystem.CloseCase(blueprint, (BlueprintConclusion[]?)null);
	}

	[Cheat(Name = "ds_add_clue")]
	public static void Cheat_AddClue(BlueprintClue blueprint, bool withAllAddendums = false)
	{
		Game.Instance.DetectiveSystem.AddClue(blueprint, null);
		if (!withAllAddendums)
		{
			return;
		}
		foreach (BlueprintClueAddendum item in blueprint.Addendums.Dereference().NotNull())
		{
			Game.Instance.DetectiveSystem.AddAddendum(item, null);
		}
	}

	[Cheat(Name = "ds_add_clue_with_all_addendums")]
	public static void Cheat_AddClueWithAllAddendums(BlueprintClue blueprint)
	{
		Cheat_AddClue(blueprint, withAllAddendums: true);
	}

	[Cheat(Name = "ds_remove_clue")]
	public static void Cheat_RemoveClue(BlueprintClue blueprint)
	{
		Game.Instance.DetectiveSystem.RemoveClue(blueprint);
	}

	[Cheat(Name = "ds_add_addendum")]
	public static void Cheat_AddAddendum(BlueprintClueAddendum blueprint)
	{
		Game.Instance.DetectiveSystem.AddAddendum(blueprint, null);
	}

	[Cheat(Name = "ds_remove_addendum")]
	public static void Cheat_RemoveAddendum(BlueprintClueAddendum blueprint)
	{
		Game.Instance.DetectiveSystem.RemoveAddendum(blueprint);
	}

	[Cheat(Name = "ds_study_clue")]
	public static void Cheat_StudyClue(BlueprintClue clue)
	{
		clue.Studies.Dereference().ForEach(delegate(BlueprintClueStudy s)
		{
			Game.Instance.DetectiveSystem.StudyClue(s, force: true);
		});
	}

	[Cheat(Name = "ds_move_to_new_clue")]
	public static void Cheat_SetMoveToClue(bool shouldMove)
	{
		UIConfig.Instance.DetectiveConfig.SetMoveToNewClue(shouldMove);
	}

	public IEnumerator WatchNewlyUnlockedClueStudies()
	{
		List<BlueprintClueStudy> _lockedStudiesBuffer = new List<BlueprintClueStudy>(128);
		while (true)
		{
			_lockedStudiesBuffer.Clear();
			CollectLockedStudies(_lockedStudiesBuffer);
			foreach (BlueprintClueStudy study in _lockedStudiesBuffer)
			{
				using (ElementsDebugger.Disable())
				{
					if (study.UnlockCondition.Check())
					{
						m_CluesStudyWithUnlockedCondition.Add(study);
						base.EventBus.RaiseEvent(delegate(IStudyConditionUnlocked h)
						{
							h.HandleStudyUnlockedCondition(study);
						});
					}
				}
				yield return null;
			}
			yield return null;
		}
	}

	private void CollectLockedStudies(List<BlueprintClueStudy> studies)
	{
		foreach (BlueprintCase item in GetCasesWithStatus(CaseStatus.Opened))
		{
			foreach (BlueprintClue item2 in item.Clues.Dereference())
			{
				if (!HasClueExcludingHidden(item2))
				{
					continue;
				}
				foreach (BlueprintClueStudy item3 in item2.Studies.Dereference())
				{
					if (!m_Studies.Contains(item3) && !m_CluesStudyWithUnlockedCondition.Contains(item3))
					{
						studies.Add(item3);
					}
				}
			}
		}
	}

	public DetectiveSystem()
		: base("detective-system-id", isInGame: true)
	{
	}

	public DetectiveSystem(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView? CreateViewForData()
	{
		return null;
	}

	public void OpenCase(BlueprintCase blueprint)
	{
		BlueprintCase blueprint = blueprint;
		CaseState orCreateState = GetOrCreateState(blueprint);
		if (orCreateState.Status != CaseStatus.Opened)
		{
			if (orCreateState.Status == CaseStatus.Closed)
			{
				throw new InvalidOperationException($"Can't open closed case {blueprint}");
			}
			orCreateState.Status = CaseStatus.Opened;
			PFLog.History.Detective.Log(blueprint, "case {0} opened", blueprint);
			Metrics.DetectiveCase.Id(blueprint.AssetGuid).State(orCreateState.Status).Send();
			blueprint.OnOpen.Run();
			base.EventBus.RaiseEvent(delegate(ICaseStatusChanged h)
			{
				h.HandleCaseStatusChanged(blueprint);
			});
		}
	}

	public void ReopenCase(BlueprintCase blueprint)
	{
		BlueprintCase blueprint = blueprint;
		CaseState orCreateState = GetOrCreateState(blueprint);
		if (orCreateState.Status == CaseStatus.Opened)
		{
			return;
		}
		if (orCreateState.Status != CaseStatus.Closed)
		{
			throw new InvalidOperationException($"Can't reopen not closed case {blueprint}");
		}
		orCreateState.Status = CaseStatus.Opened;
		orCreateState.Question = null;
		orCreateState.Answer = null;
		PFLog.History.Detective.Log(blueprint, "case {0} reopened", blueprint);
		Metrics.DetectiveCase.Id(blueprint.AssetGuid).State(orCreateState.Status).Send();
		base.EventBus.RaiseEvent(delegate(ICaseStatusChanged h)
		{
			h.HandleCaseStatusChanged(blueprint);
		});
		foreach (BlueprintCaseItem allItem in blueprint.AllItems)
		{
			Unhide(allItem);
		}
	}

	public void CloseCaseCheat(BlueprintCase blueprint)
	{
		BlueprintCaseQuestion blueprintCaseQuestion = GetActualCaseQuestion(blueprint);
		if (blueprintCaseQuestion.NoAnswer)
		{
			blueprintCaseQuestion = blueprint.Questions.Dereference().FirstOrDefault((BlueprintCaseQuestion i) => !i.NoAnswer);
		}
		if (blueprintCaseQuestion == null)
		{
			throw new InvalidOperationException($"Can't close case {blueprint} without question");
		}
		CloseCase(blueprint, blueprintCaseQuestion, blueprintCaseQuestion.RightAnswer.Blueprint);
	}

	public void CloseCase(BlueprintCase blueprint, BlueprintCaseAnswer answer)
	{
		BlueprintCaseQuestion actualCaseQuestion = GetActualCaseQuestion(blueprint);
		CloseCase(blueprint, actualCaseQuestion, answer);
	}

	public void CloseCaseWithoutAnswer(BlueprintCase blueprint)
	{
		BlueprintCaseQuestion actualCaseQuestion = GetActualCaseQuestion(blueprint);
		CloseCase(blueprint, actualCaseQuestion, null);
	}

	private void CloseCase(BlueprintCase blueprint, BlueprintCaseQuestion? question, BlueprintCaseAnswer? answer)
	{
		BlueprintCase blueprint = blueprint;
		CaseState orCreateState = GetOrCreateState(blueprint);
		if (orCreateState.Status != CaseStatus.Closed)
		{
			if (orCreateState.Status == CaseStatus.None)
			{
				throw new InvalidOperationException($"Can't close not opened case {blueprint}");
			}
			if (question != null && !blueprint.Questions.Dereference().Contains<BlueprintCaseQuestion>(question))
			{
				throw new InvalidOperationException($"Can't close case {blueprint} with question {question}");
			}
			if (question != null && answer != null && !question.AllAnswers.Dereference().Contains<BlueprintCaseAnswer>(answer))
			{
				throw new InvalidOperationException($"Can't close case {blueprint} with answer {answer}");
			}
			orCreateState.Status = CaseStatus.Closed;
			orCreateState.Question = question;
			orCreateState.Answer = answer;
			PFLog.History.Detective.Log(blueprint, "case {0} closed, question: {1}, answer: {2}", blueprint, ((object)question) ?? ((object)"none"), ((object)answer) ?? ((object)"none"));
			Metrics.DetectiveCase.Id(blueprint.AssetGuid).State(orCreateState.Status).Question(question?.AssetGuid)
				.Answer(answer?.AssetGuid)
				.Send();
			blueprint.OnClose.Run();
			base.EventBus.RaiseEvent(delegate(ICaseStatusChanged h)
			{
				h.HandleCaseStatusChanged(blueprint);
			});
		}
	}

	private bool HideIfReceivedWhileCaseClosed(BlueprintCaseItem blueprint, CaseItemState state)
	{
		return state.Hidden = GetCaseStatus(blueprint) == CaseStatus.Closed;
	}

	private void Unhide(BlueprintCaseItem blueprint)
	{
		CaseItemState state;
		if (!(blueprint is BlueprintClue blueprint2))
		{
			if (!(blueprint is BlueprintClueAddendum blueprint3))
			{
				if (!(blueprint is BlueprintConclusion blueprint4))
				{
					throw new ArgumentOutOfRangeException("blueprint");
				}
				state = GetState(blueprint4);
			}
			else
			{
				state = GetState(blueprint3);
			}
		}
		else
		{
			state = GetState(blueprint2);
		}
		CaseItemState caseItemState = state;
		if (caseItemState != null && caseItemState.Hidden)
		{
			caseItemState.Hidden = false;
			PFLog.History.Detective.Log(blueprint, "case item {0} unhidden", blueprint);
			NotifyCaseItemChanged(blueprint);
		}
	}

	public void AddClue(BlueprintClue blueprint, BlueprintScriptableObject? source)
	{
		BlueprintClue blueprint = blueprint;
		BlueprintScriptableObject source = source;
		ClueState orCreateState = GetOrCreateState(blueprint);
		if (!orCreateState.Found)
		{
			(from i in blueprint.ParentCase.Blueprint.Clues.Dereference()
				where i.HasOverride(blueprint)
				select i).ForEach(delegate(BlueprintClue i)
			{
				AddClue(i, source);
			});
			orCreateState.Found = true;
			orCreateState.Source = source;
			orCreateState.PlaceOfIssue = Game.Instance.CurrentlyLoadedArea;
			bool flag = HideIfReceivedWhileCaseClosed(blueprint, orCreateState);
			PFLog.History.Detective.Log(blueprint, "clue {0} added{1}, source: {2}", blueprint, flag ? " (hidden, case is closed)" : "", ((object)source) ?? ((object)"none"));
			Metrics.DetectivePiece.Id(blueprint.AssetGuid).Type(DetectivePieceMetricsEvent.PieceType.Clue).State(DetectivePieceMetricsEvent.PieceState.Added)
				.Send();
			if (!flag)
			{
				NotifyCaseItemChanged(blueprint);
			}
		}
	}

	public void RemoveClue(BlueprintClue blueprint)
	{
		ClueState state = GetState(blueprint);
		if (state != null && state.Found)
		{
			state.Found = false;
			PFLog.History.Detective.Log(blueprint, "clue {0} removed", blueprint);
			Metrics.DetectivePiece.Id(blueprint.AssetGuid).Type(DetectivePieceMetricsEvent.PieceType.Clue).State(DetectivePieceMetricsEvent.PieceState.Removed)
				.Send();
			if (!state.Hidden)
			{
				NotifyCaseItemChanged(blueprint);
			}
			blueprint.PossibleConclusions.Dereference().ForEach(RemoveConclusion);
		}
	}

	public void AddAddendum(BlueprintClueAddendum blueprint, BlueprintScriptableObject? source)
	{
		BlueprintClueAddendum blueprint = blueprint;
		BlueprintScriptableObject source = source;
		AddendumState orCreateState = GetOrCreateState(blueprint);
		if (!orCreateState.Found)
		{
			orCreateState.Found = true;
			orCreateState.Source = source;
			orCreateState.PlaceOfIssue = Game.Instance.CurrentlyLoadedArea;
			(from i in blueprint.ParentClue.Blueprint.Addendums.Dereference()
				where i.HasOverride(blueprint)
				select i).ForEach(delegate(BlueprintClueAddendum i)
			{
				AddAddendum(i, source);
			});
			Metrics.DetectivePiece.Id(blueprint.AssetGuid).Type(DetectivePieceMetricsEvent.PieceType.Addendum).State(DetectivePieceMetricsEvent.PieceState.Added)
				.Send();
			bool flag = HideIfReceivedWhileCaseClosed(blueprint, orCreateState);
			PFLog.History.Detective.Log(blueprint, "addendum {0} added{1}, source: {2}", blueprint, flag ? " (hidden, case is closed)" : "", ((object)source) ?? ((object)"none"));
			if (!flag)
			{
				NotifyCaseItemChanged(blueprint);
			}
		}
	}

	public void RemoveAddendum(BlueprintClueAddendum blueprint)
	{
		AddendumState state = GetState(blueprint);
		if (state != null && state.Found)
		{
			state.Found = false;
			Metrics.DetectivePiece.Id(blueprint.AssetGuid).Type(DetectivePieceMetricsEvent.PieceType.Addendum).State(DetectivePieceMetricsEvent.PieceState.Removed)
				.Send();
			PFLog.History.Detective.Log(blueprint, "addendum {0} removed", blueprint);
			if (!state.Hidden)
			{
				NotifyCaseItemChanged(blueprint);
			}
			blueprint.PossibleConclusions.Dereference().ForEach(RemoveConclusion);
		}
	}

	public void AddConclusion(BlueprintConclusion blueprint)
	{
		ConclusionState orCreateState = GetOrCreateState(blueprint);
		if (!orCreateState.Made)
		{
			orCreateState.Made = true;
			bool flag = HideIfReceivedWhileCaseClosed(blueprint, orCreateState);
			PFLog.History.Detective.Log(blueprint, "conclusion {0} added{1}", blueprint, flag ? " (hidden, case is closed)" : "");
			Metrics.DetectivePiece.Id(blueprint.AssetGuid).Type(DetectivePieceMetricsEvent.PieceType.Conclusion).State(DetectivePieceMetricsEvent.PieceState.Added)
				.Send();
			if (!flag)
			{
				NotifyCaseItemChanged(blueprint);
			}
			GetConflictedConclusions(blueprint).ForEach(RemoveConclusion);
		}
	}

	public List<BlueprintConclusion> GetConflictedConclusions(BlueprintConclusion blueprint)
	{
		BlueprintConclusion blueprint = blueprint;
		List<BlueprintConclusion> list = new List<BlueprintConclusion>();
		BlueprintConclusion.Source[] sources = blueprint.Sources;
		foreach (BlueprintConclusion.Source source in sources)
		{
			list.AddRange(from i in source.Item1.Blueprint.PossibleConclusions.Dereference()
				where i != blueprint
				select i);
		}
		return list;
	}

	public void RemoveConclusion(BlueprintConclusion blueprint)
	{
		ConclusionState state = GetState(blueprint);
		if (state != null && state.Made)
		{
			state.Made = false;
			PFLog.History.Detective.Log(blueprint, "conclusion {0} removed", blueprint);
			Metrics.DetectivePiece.Id(blueprint.AssetGuid).Type(DetectivePieceMetricsEvent.PieceType.Conclusion).State(DetectivePieceMetricsEvent.PieceState.Removed)
				.Send();
			if (!state.Hidden)
			{
				NotifyCaseItemChanged(blueprint);
			}
			blueprint.PossibleConclusions.Dereference().ForEach(RemoveConclusion);
		}
	}

	private void NotifyCaseItemChanged(BlueprintCaseItem blueprint)
	{
		BlueprintClue blueprintClue = blueprint as BlueprintClue;
		if (blueprintClue == null)
		{
			BlueprintClueAddendum blueprintClueAddendum = blueprint as BlueprintClueAddendum;
			if (blueprintClueAddendum == null)
			{
				BlueprintConclusion blueprintConclusion = blueprint as BlueprintConclusion;
				if (blueprintConclusion == null)
				{
					throw new ArgumentOutOfRangeException("blueprint");
				}
				base.EventBus.RaiseEvent(delegate(IConclusionStatusChanged h)
				{
					h.HandleConclusionStatusChanged(blueprintConclusion);
				});
			}
			else
			{
				base.EventBus.RaiseEvent(delegate(IClueAddendumStatusChanged h)
				{
					h.HandleClueAddendumStatusChanged(blueprintClueAddendum);
				});
			}
		}
		else
		{
			base.EventBus.RaiseEvent(delegate(IClueStatusChanged h)
			{
				h.HandleClueStatusChanged(blueprintClue);
			});
		}
	}

	public void TryAddConclusionWithDependencies(BlueprintConclusion conclusion)
	{
		if (TryAddWithDependencies(conclusion, test: true))
		{
			TryAddWithDependencies(conclusion, test: false);
		}
		bool TryAddWithDependencies(BlueprintCaseItem caseItem, bool test)
		{
			if (HasItemExcludingHidden(caseItem))
			{
				return true;
			}
			if (!(caseItem is BlueprintConclusion blueprintConclusion))
			{
				return false;
			}
			bool flag = false;
			BlueprintConclusion.Source[] sources = blueprintConclusion.Sources;
			foreach (BlueprintConclusion.Source obj in sources)
			{
				BlueprintCaseItem blueprint = obj.Item1.Blueprint;
				BlueprintCaseItem blueprint2 = obj.Item2.Blueprint;
				if (TryAddWithDependencies(blueprint, test) && TryAddWithDependencies(blueprint2, test))
				{
					flag = true;
					break;
				}
			}
			if (flag && !test)
			{
				AddConclusion(blueprintConclusion);
			}
			return flag;
		}
	}

	public CaseStatus GetCaseStatus(BlueprintCase blueprint)
	{
		return GetState(blueprint)?.Status ?? CaseStatus.None;
	}

	public CaseStatus GetCaseStatus(BlueprintCaseItem blueprint)
	{
		return GetState(blueprint.ParentCase.Blueprint)?.Status ?? CaseStatus.None;
	}

	public (BlueprintCaseQuestion Question, BlueprintCaseAnswer Answer)? GetCaseAnswer(BlueprintCase blueprint)
	{
		CaseState state = GetState(blueprint);
		if (state != null)
		{
			BlueprintCaseQuestion question = state.Question;
			if (question != null)
			{
				BlueprintCaseAnswer answer = state.Answer;
				if (answer != null)
				{
					return (question, answer);
				}
			}
		}
		return null;
	}

	public IEnumerable<BlueprintCase> GetCasesWithStatus(CaseStatus status)
	{
		return from i in m_Cases
			where i.Value.Status == status
			select i.Key;
	}

	public IEnumerable<BlueprintCase> GetAllAvailableCases()
	{
		return from i in m_Cases
			where i.Value.Status != CaseStatus.None
			select i.Key;
	}

	public IEnumerable<BlueprintClue> GetUnknownClues()
	{
		foreach (var (blueprintClue2, clueState2) in m_Clues)
		{
			if (clueState2.Found && GetCaseStatus(blueprintClue2.ParentCase.Blueprint) == CaseStatus.None)
			{
				yield return blueprintClue2;
			}
		}
	}

	public IEnumerable<BlueprintClueAddendum> GetOpenedAddendumsFor(BlueprintClue blueprintClue)
	{
		foreach (var (blueprintClueAddendum2, addendumState2) in m_Addendums)
		{
			if (!addendumState2.Hidden)
			{
				BlueprintClue blueprint = blueprintClueAddendum2.ParentClue.Blueprint;
				if (addendumState2.Found && (blueprint == blueprintClue || blueprint.HasOverride(blueprintClue)))
				{
					yield return blueprintClueAddendum2;
				}
			}
		}
	}

	public IEnumerable<BlueprintConclusion> GetConclusionsFrom(BlueprintCaseItem caseItem)
	{
		BlueprintCaseItem caseItem = caseItem;
		return from c in m_Conclusions
			where !c.Value.Hidden && c.Key.Sources.Any((BlueprintConclusion.Source s) => s.Contains(caseItem))
			select c into i
			select i.Key;
	}

	private bool HasClue(BlueprintClue blueprint, bool excludeHidden)
	{
		ClueState state = GetState(blueprint);
		if (state != null && state.Found)
		{
			if (excludeHidden)
			{
				return !state.Hidden;
			}
			return true;
		}
		return false;
	}

	private bool HasClueAddendum(BlueprintClueAddendum blueprint, bool excludeHidden)
	{
		AddendumState state = GetState(blueprint);
		if (state != null && state.Found)
		{
			if (excludeHidden)
			{
				return !state.Hidden;
			}
			return true;
		}
		return false;
	}

	private bool HasConclusion(BlueprintConclusion conclusion, bool excludeHidden)
	{
		ConclusionState state = GetState(conclusion);
		if (state != null && state.Made)
		{
			if (excludeHidden)
			{
				return !state.Hidden;
			}
			return true;
		}
		return false;
	}

	public bool HasItem(BlueprintCaseItem item, bool excludeHidden)
	{
		if (!(item is BlueprintClue blueprint))
		{
			if (!(item is BlueprintClueAddendum blueprint2))
			{
				if (item is BlueprintConclusion conclusion)
				{
					return HasConclusion(conclusion, excludeHidden);
				}
				throw new ArgumentOutOfRangeException("item");
			}
			return HasClueAddendum(blueprint2, excludeHidden);
		}
		return HasClue(blueprint, excludeHidden);
	}

	public bool HasClueExcludingHidden(BlueprintClue blueprint)
	{
		return HasClue(blueprint, excludeHidden: true);
	}

	public bool HasClueAddendumExcludingHidden(BlueprintClueAddendum blueprint)
	{
		return HasClueAddendum(blueprint, excludeHidden: true);
	}

	public bool HasConclusionExcludingHidden(BlueprintConclusion blueprint)
	{
		return HasConclusion(blueprint, excludeHidden: true);
	}

	public bool HasItemExcludingHidden(BlueprintCaseItem blueprint)
	{
		return HasItem(blueprint, excludeHidden: true);
	}

	public bool HasClueIncludingHidden(BlueprintClue blueprint)
	{
		return GetState(blueprint)?.Found ?? false;
	}

	public bool HasClueAddendumIncludingHidden(BlueprintClueAddendum blueprint)
	{
		return GetState(blueprint)?.Found ?? false;
	}

	public bool HasConclusionIncludingHidden(BlueprintConclusion conclusion)
	{
		return GetState(conclusion)?.Made ?? false;
	}

	public bool HasItemIncludingHidden(BlueprintCaseItem item)
	{
		if (!(item is BlueprintClue blueprint))
		{
			if (!(item is BlueprintClueAddendum blueprint2))
			{
				if (item is BlueprintConclusion conclusion)
				{
					return HasConclusionIncludingHidden(conclusion);
				}
				throw new ArgumentOutOfRangeException("item");
			}
			return HasClueAddendumIncludingHidden(blueprint2);
		}
		return HasClueIncludingHidden(blueprint);
	}

	public bool HasAvailableItem(BlueprintCaseItem item)
	{
		bool flag = HasItemExcludingHidden(item);
		if (flag)
		{
			bool flag2 = !(item is BlueprintClueAddendum blueprintClueAddendum) || HasClueExcludingHidden(blueprintClueAddendum.ParentClue.Blueprint);
			flag = flag2;
		}
		return flag;
	}

	public BlueprintScriptableObject? GetSource(BlueprintClue clue)
	{
		return GetState(clue)?.Source;
	}

	public BlueprintScriptableObject? GetSource(BlueprintClueAddendum addendum)
	{
		return GetState(addendum)?.Source;
	}

	public BlueprintArea? GetIssuePlace(BlueprintClue clue)
	{
		return GetState(clue)?.PlaceOfIssue;
	}

	public BlueprintArea? GetIssuePlace(BlueprintClueAddendum addendum)
	{
		return GetState(addendum)?.PlaceOfIssue;
	}

	public IEnumerable<BlueprintClue> GetAvailableClues(BlueprintCase @case)
	{
		return @case.Clues.Dereference().Where(HasItemExcludingHidden);
	}

	public IEnumerable<BlueprintConclusion> GetAvailableConclusions(BlueprintCase @case)
	{
		return from i in @case.Conclusions.Dereference()
			where i.Sources.HasItem((BlueprintConclusion.Source s) => HasAvailableItem(s.Item1.Blueprint) && HasAvailableItem(s.Item2.Blueprint))
			select i;
	}

	public BlueprintClue.Override? GetOverride(BlueprintClue clue)
	{
		return clue.Overrides.FirstOrDefault((BlueprintClue.Override i) => HasClueExcludingHidden(i.Clue.Blueprint));
	}

	public BlueprintClueAddendum.Override? GetOverride(BlueprintClueAddendum addendum)
	{
		return addendum.Overrides.FirstOrDefault((BlueprintClueAddendum.Override i) => HasClueAddendumExcludingHidden(i.Addendum.Blueprint));
	}

	public BlueprintCaseQuestion GetActualCaseQuestion(BlueprintCase @case)
	{
		CaseState state = GetState(@case);
		if (state != null && state.OpenedByCheats)
		{
			return @case.Questions.Dereference().FirstOrDefault() ?? throw new InvalidOperationException($"Can't get actual question for case {@case}");
		}
		return @case.Questions.Dereference().FirstOrDefault((BlueprintCaseQuestion i) => i.Condition.Check()) ?? throw new InvalidOperationException($"Can't get actual question for case {@case}");
	}

	public CaseDisplayData GetCaseDisplay(BlueprintCase @case)
	{
		BlueprintCaseQuestion.CaseDisplayOverride displayOverride = GetActualCaseQuestion(@case).DisplayOverride;
		string name = (string.IsNullOrEmpty(displayOverride.Name.Text) ? @case.Name.Text : displayOverride.Name.Text);
		string description = (string.IsNullOrEmpty(displayOverride.Description.Text) ? @case.Description.Text : displayOverride.Description.Text);
		Sprite icon = ((displayOverride.Icon != null) ? displayOverride.Icon : @case.Icon);
		return new CaseDisplayData(name, description, icon);
	}

	public bool TryGetAnswerDegree(BlueprintCaseAnswer answer, out int degree)
	{
		if (!HasItemExcludingHidden(answer.RelatedItem.Blueprint))
		{
			degree = -1;
			return false;
		}
		for (int num = answer.DegreeProgression.Length - 1; num >= 0; num--)
		{
			List<BlueprintCaseItem> list;
			using (answer.DegreeProgression[num].Items.Dereference().ToPooledList(out list))
			{
				if (!list.Any() || list.Any(HasItemExcludingHidden))
				{
					degree = num;
					return true;
				}
			}
		}
		degree = -1;
		return false;
	}

	public void ObserveClue(BlueprintClue clue)
	{
		ClueState state = GetState(clue);
		if (state == null)
		{
			throw new InvalidOperationException($"Can not observe clue {clue}: not found yet");
		}
		if (!state.Observed)
		{
			state.Observed = true;
			PFLog.History.Detective.Log(clue, "clue {0} observed", clue);
			Experience.TryGain(clue);
		}
	}

	private bool IsCompanionAvailableForStudy(BlueprintUnit? blueprint)
	{
		BlueprintUnit blueprint = blueprint;
		if (blueprint == null)
		{
			return true;
		}
		CompanionState? companionState = Game.Instance.Player.AllCrossSceneUnits.FirstOrDefault((BaseUnitEntity c) => c.Blueprint == blueprint)?.GetCompanionOptional()?.State;
		if (companionState != CompanionState.InParty)
		{
			if (Game.Instance.LoadedAreaState.Settings.CapitalPartyMode)
			{
				return companionState == CompanionState.Remote;
			}
			return false;
		}
		return true;
	}

	public bool IsAvailableForStudy(BlueprintClueStudy study)
	{
		if (!m_Studies.Contains(study) && IsCompanionAvailableForStudy(study.StudyCompanion))
		{
			return study.UnlockCondition.Check();
		}
		return false;
	}

	public bool IsAvailableForStudy(BlueprintClue clue)
	{
		return clue.Studies.Dereference().Any(IsAvailableForStudy);
	}

	public IEnumerable<BlueprintClue> GetCluesAvailableForStudy(BlueprintCase @case)
	{
		return @case.Clues.Dereference().Where(IsAvailableForStudy);
	}

	public void StudyClue(BlueprintClueStudy study, bool force = false)
	{
		if (!IsAvailableForStudy(study) && !force)
		{
			return;
		}
		m_Studies.Add(study);
		PFLog.History.Detective.Log(study, "clue study {0} completed", study);
		foreach (BlueprintCaseItem item in study.GiveItems.Dereference())
		{
			if (!(item is BlueprintClueAddendum blueprint))
			{
				if (item is BlueprintClue blueprint2)
				{
					AddClue(blueprint2, study);
				}
				else
				{
					PFLog.Default.Error($"Can't give {item} from {study} because it is {item.GetType().Name}");
				}
			}
			else
			{
				AddAddendum(blueprint, study);
			}
		}
	}

	public bool IsStudied(BlueprintClueStudy study)
	{
		return m_Studies.Contains(study);
	}

	private CaseState GetOrCreateState(BlueprintCase blueprint)
	{
		if (!m_Cases.TryGetValue(blueprint, out CaseState value))
		{
			value = (m_Cases[blueprint] = new CaseState(blueprint));
		}
		return value;
	}

	private ClueState GetOrCreateState(BlueprintClue blueprint)
	{
		if (!m_Clues.TryGetValue(blueprint, out ClueState value))
		{
			value = (m_Clues[blueprint] = new ClueState(blueprint));
		}
		return value;
	}

	private AddendumState GetOrCreateState(BlueprintClueAddendum blueprint)
	{
		if (!m_Addendums.TryGetValue(blueprint, out AddendumState value))
		{
			value = (m_Addendums[blueprint] = new AddendumState(blueprint));
		}
		return value;
	}

	private ConclusionState GetOrCreateState(BlueprintConclusion blueprint)
	{
		if (!m_Conclusions.TryGetValue(blueprint, out ConclusionState value))
		{
			value = (m_Conclusions[blueprint] = new ConclusionState(blueprint));
		}
		return value;
	}

	private CaseState? GetState(BlueprintCase blueprint)
	{
		return m_Cases.GetValueOrDefault(blueprint);
	}

	private ClueState? GetState(BlueprintClue blueprint)
	{
		return m_Clues.GetValueOrDefault(blueprint);
	}

	private AddendumState? GetState(BlueprintClueAddendum blueprint)
	{
		return m_Addendums.GetValueOrDefault(blueprint);
	}

	private ConclusionState? GetState(BlueprintConclusion blueprint)
	{
		return m_Conclusions.GetValueOrDefault(blueprint);
	}

	[Obsolete("New Question/Answer approach, WIP")]
	public void CloseCase(BlueprintCase blueprint, BlueprintConclusion[]? conclusions)
	{
		BlueprintCase blueprint = blueprint;
		CaseState orCreateState = GetOrCreateState(blueprint);
		if (orCreateState.Status != CaseStatus.Closed)
		{
			if (orCreateState.Status == CaseStatus.None)
			{
				throw new InvalidOperationException($"Can't close not opened case {blueprint}");
			}
			orCreateState.Status = CaseStatus.Closed;
			if (conclusions != null)
			{
				orCreateState.CorrectConclusionsCount = CountCorrectConclusions(blueprint, conclusions);
				orCreateState.Conclusions = conclusions;
			}
			blueprint.OnClose.Run();
			base.EventBus.RaiseEvent(delegate(ICaseStatusChanged h)
			{
				h.HandleCaseStatusChanged(blueprint);
			});
		}
	}

	[Obsolete("New Question/Answer approach, WIP")]
	public BlueprintConclusion[]? GetCaseConclusions(BlueprintCase blueprint)
	{
		return GetState(blueprint)?.Conclusions;
	}

	[Obsolete("New Question/Answer approach, WIP")]
	public int CountCorrectConclusions(BlueprintCase @case, BlueprintConclusion[] conclusions)
	{
		BlueprintCase @case = @case;
		return conclusions.Count((BlueprintConclusion i) => IsCorrectConclusion(@case, i));
	}

	[Obsolete("New Question/Answer approach, WIP")]
	public bool IsCorrectConclusion(BlueprintCase @case, BlueprintConclusion conclusion)
	{
		return @case.CorrectConclusions.Contains<BlueprintConclusion>(conclusion);
	}

	[Obsolete("New Question/Answer approach, WIP")]
	public int GetCorrectConclusionsCount(BlueprintCase @case)
	{
		return GetState(@case)?.CorrectConclusionsCount ?? 0;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintCase, CaseState> cases = m_Cases;
		if (cases != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintCase, CaseState> item in cases)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<CaseState>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		Dictionary<BlueprintClue, ClueState> clues = m_Clues;
		if (clues != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<BlueprintClue, ClueState> item2 in clues)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val6 = SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val6);
				Hash128 val7 = ClassHasher<ClueState>.GetHash128(item2.Value);
				hash2.Append(ref val7);
				val5 ^= hash2.GetHashCode();
			}
			result.Append(ref val5);
		}
		Dictionary<BlueprintClueAddendum, AddendumState> addendums = m_Addendums;
		if (addendums != null)
		{
			int val8 = 0;
			foreach (KeyValuePair<BlueprintClueAddendum, AddendumState> item3 in addendums)
			{
				Hash128 hash3 = default(Hash128);
				Hash128 val9 = SimpleBlueprintHasher.GetHash128(item3.Key);
				hash3.Append(ref val9);
				Hash128 val10 = ClassHasher<AddendumState>.GetHash128(item3.Value);
				hash3.Append(ref val10);
				val8 ^= hash3.GetHashCode();
			}
			result.Append(ref val8);
		}
		Dictionary<BlueprintConclusion, ConclusionState> conclusions = m_Conclusions;
		if (conclusions != null)
		{
			int val11 = 0;
			foreach (KeyValuePair<BlueprintConclusion, ConclusionState> item4 in conclusions)
			{
				Hash128 hash4 = default(Hash128);
				Hash128 val12 = SimpleBlueprintHasher.GetHash128(item4.Key);
				hash4.Append(ref val12);
				Hash128 val13 = ClassHasher<ConclusionState>.GetHash128(item4.Value);
				hash4.Append(ref val13);
				val11 ^= hash4.GetHashCode();
			}
			result.Append(ref val11);
		}
		HashSet<BlueprintClueStudy> studies = m_Studies;
		if (studies != null)
		{
			int num = 0;
			foreach (BlueprintClueStudy item5 in studies)
			{
				num ^= SimpleBlueprintHasher.GetHash128(item5).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectiveSystem source = new DetectiveSystem(default(OwlPackConstructorParameter));
		result = Unsafe.As<DetectiveSystem, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<DetectiveSystem>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		HashSet<BlueprintClueStudy> value2 = m_CluesStudyWithUnlockedCondition;
		formatter.Field(10, "m_CluesStudyWithUnlockedCondition", ref value2, state);
		Dictionary<BlueprintCase, CaseState> value3 = m_Cases;
		formatter.Field(11, "m_Cases", ref value3, state);
		Dictionary<BlueprintClue, ClueState> value4 = m_Clues;
		formatter.Field(12, "m_Clues", ref value4, state);
		Dictionary<BlueprintClueAddendum, AddendumState> value5 = m_Addendums;
		formatter.Field(13, "m_Addendums", ref value5, state);
		Dictionary<BlueprintConclusion, ConclusionState> value6 = m_Conclusions;
		formatter.Field(14, "m_Conclusions", ref value6, state);
		HashSet<BlueprintClueStudy> value7 = m_Studies;
		formatter.Field(15, "m_Studies", ref value7, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectiveSystem>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				Unsafe.AsRef(in m_CluesStudyWithUnlockedCondition) = formatter.ReadPackable<HashSet<BlueprintClueStudy>>(state);
				break;
			case 11:
				Unsafe.AsRef(in m_Cases) = formatter.ReadPackable<Dictionary<BlueprintCase, CaseState>>(state);
				break;
			case 12:
				Unsafe.AsRef(in m_Clues) = formatter.ReadPackable<Dictionary<BlueprintClue, ClueState>>(state);
				break;
			case 13:
				Unsafe.AsRef(in m_Addendums) = formatter.ReadPackable<Dictionary<BlueprintClueAddendum, AddendumState>>(state);
				break;
			case 14:
				Unsafe.AsRef(in m_Conclusions) = formatter.ReadPackable<Dictionary<BlueprintConclusion, ConclusionState>>(state);
				break;
			case 15:
				Unsafe.AsRef(in m_Studies) = formatter.ReadPackable<HashSet<BlueprintClueStudy>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
