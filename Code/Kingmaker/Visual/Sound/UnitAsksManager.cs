using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums.Sound;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Sound.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class UnitAsksManager
{
	private AskSchedulingEntry m_CurrentlyActiveAsk;

	private Coroutine m_CurrentlyPlayAfterCoroutine;

	private uint m_CurrentlyPlayingId;

	public readonly BlueprintUnitAsksList Blueprint;

	public readonly AbstractUnitEntity Unit;

	private readonly string[] m_SoundBanks;

	private UnitAsksQueue m_Queue = new UnitAsksQueue();

	public readonly AskWrapper Aggro;

	public readonly AskWrapper Pain;

	public readonly AskWrapper Death;

	public readonly AskWrapper Unconscious;

	public readonly AskWrapper CriticalHit;

	public readonly AskWrapper TraumaApplied;

	public readonly AskWrapper Order;

	public readonly AskWrapper Select;

	public readonly AskWrapper CantDo;

	public readonly AskWrapper CheckSuccessful;

	public readonly AskWrapper CheckFailed;

	public readonly AskWrapper Discovery;

	public readonly AskWrapper MoveInCombat;

	public readonly AskWrapper MoveInExploration;

	public readonly AskWrapper EnemyDeath;

	public readonly AskWrapper PartyMemberUnconsciousGeneral;

	public readonly AskWrapper PsychicPhenomena;

	public readonly AskWrapper PerilsOfTheWarp;

	public readonly AskWrapper SupportAnAlly;

	public readonly AskWrapper BeingSupported;

	public readonly AskWrapper EnemyMassDeath;

	public readonly AskWrapper FriendlyFire;

	public readonly AskWrapper BrokenMorale;

	public readonly AskWrapper HeroicMorale;

	public readonly AskWrapper ChannellingOn;

	public readonly AskWrapper ChannellingReaction;

	public readonly AskWrapper ChannellingOff;

	public readonly AskWrapper ChannellingSuccessfulRelease;

	public readonly AskWrapper WeAreLoosing;

	public readonly AskWrapper WeAreWinning;

	public readonly AskWrapper WeAreLostByMorale;

	public readonly AskWrapper BrokenArmour;

	public readonly AskWrapper ClueCanBeProcessed;

	public readonly AskWrapper ConclusionAvailable;

	public readonly AskWrapper DetectiveSearch;

	public readonly AskWrapper TracesFound;

	public readonly AskWrapper DetectiveReminder;

	public readonly AskWrapper DetectiveReconstructionFound;

	public readonly AskWrapper DetectiveReconstructionReady;

	public readonly AskWrapper SignalFound;

	public readonly AskWrapper WithinPariahAura;

	public readonly AskWrapper PainPariahKeystone;

	public readonly AskWrapper[] PartyMemberUnconsciousPersonalized;

	public readonly AskWrapper[] AnimationBarks;

	public AskWrapper SelectAnimationBark(MappedAnimationEventType evt)
	{
		return (from b in AnimationBarks.EmptyIfNull()
			where ((AnimationAsk)b.AsksSet).AnimationEvent == evt
			select b).Random(PFStatefulRandom.Visuals.Sounds);
	}

	public UnitAsksManager(AbstractUnitEntity unit, BlueprintUnitAsksList asksList)
	{
		Blueprint = asksList;
		m_SoundBanks = asksList.SoundBanks;
		Unit = unit;
		Aggro = Wrap(asksList.AggroBattleCry, "AggroBattleCry");
		Pain = Wrap(asksList.Pain, "Pain");
		Death = Wrap(asksList.Death, "Death");
		Unconscious = Wrap(asksList.Unconscious, "Unconscious");
		CriticalHit = Wrap(asksList.CriticalHit, "CriticalHit");
		TraumaApplied = Wrap(asksList.TraumaApplied, "TraumaApplied");
		Order = Wrap(asksList.Order, "Order");
		Select = Wrap(asksList.Select, "Select");
		CantDo = Wrap(asksList.CantDo, "CantDo");
		CheckSuccessful = Wrap(asksList.CheckSuccessful, "CheckSuccessful");
		CheckFailed = Wrap(asksList.CheckFailed, "CheckFailed");
		Discovery = Wrap(asksList.Discovery, "Discovery");
		MoveInCombat = Wrap(asksList.MoveInCombat, "MoveInCombat");
		MoveInExploration = Wrap(asksList.MoveInExploration, "MoveInExploration");
		EnemyDeath = Wrap(asksList.EnemyDeath, "EnemyDeath");
		PartyMemberUnconsciousGeneral = Wrap(asksList.PartyMemberUnconsciousGeneral, "PartyMemberUnconsciousGeneral");
		PsychicPhenomena = Wrap(asksList.PsychicPhenomena, "PsychicPhenomena");
		PerilsOfTheWarp = Wrap(asksList.PerilsOfTheWarp, "PerilsOfTheWarp");
		SupportAnAlly = Wrap(asksList.SupportAnAlly, "SupportAnAlly");
		BeingSupported = Wrap(asksList.BeingSupported, "BeingSupported");
		EnemyMassDeath = Wrap(asksList.EnemyMassDeath, "EnemyMassDeath");
		FriendlyFire = Wrap(asksList.FriendlyFire, "FriendlyFire");
		BrokenMorale = Wrap(asksList.BrokenMorale, "BrokenMorale");
		HeroicMorale = Wrap(asksList.HeroicMorale, "HeroicMorale");
		ChannellingOn = Wrap(asksList.ChannellingOn, "ChannellingOn");
		ChannellingOff = Wrap(asksList.ChannellingOff, "ChannellingOff");
		ChannellingReaction = Wrap(asksList.ChannellingReaction, "ChannellingReaction");
		ChannellingSuccessfulRelease = Wrap(asksList.ChannellingSuccessfulRelease, "ChannellingSuccessfulRelease");
		WeAreLoosing = Wrap(asksList.WeAreLoosing, "WeAreLoosing");
		WeAreWinning = Wrap(asksList.WeAreWinning, "WeAreWinning");
		WeAreLostByMorale = Wrap(asksList.WeAreLostByMorale, "WeAreLostByMorale");
		BrokenArmour = Wrap(asksList.BrokenArmour, "BrokenArmour");
		ClueCanBeProcessed = Wrap(asksList.ClueCanBeProcessed, "ClueCanBeProcessed");
		ConclusionAvailable = Wrap(asksList.ConclusionAvailable, "ConclusionAvailable");
		DetectiveSearch = Wrap(asksList.DetectiveSearch, "DetectiveSearch");
		TracesFound = Wrap(asksList.TracesFound, "TracesFound");
		DetectiveReminder = Wrap(asksList.DetectiveReminder, "DetectiveReminder");
		DetectiveReconstructionFound = Wrap(asksList.DetectiveReconstructionFound, "DetectiveReconstructionFound");
		DetectiveReconstructionReady = Wrap(asksList.DetectiveReconstructionReady, "DetectiveReconstructionReady");
		SignalFound = Wrap(asksList.SignalFound, "SignalFound");
		WithinPariahAura = Wrap(asksList.WithinPariahAura, "WithinPariahAura");
		PainPariahKeystone = Wrap(asksList.PainPariahKeystone, "PainPariahKeystone");
		PartyMemberUnconsciousPersonalized = (from a in asksList.PartyMemberUnconsciousPersonalized.EmptyIfNull()
			select Wrap(a, "PartyMemberUnconsciousPersonalized")).ToArray();
		AnimationBarks = (from a in asksList.MappedSound.EmptyIfNull()
			select Wrap(a, "AnimationEvent")).ToArray();
	}

	private AskWrapper Wrap(AsksSet bark, string type)
	{
		return new AskWrapper(bark, this, type);
	}

	public void LoadBanks()
	{
		string[] soundBanks = m_SoundBanks;
		for (int i = 0; i < soundBanks.Length; i++)
		{
			SoundBanksManager.LoadBank(soundBanks[i]);
		}
		OnLoadBanks();
	}

	protected virtual void OnLoadBanks()
	{
	}

	public void UnloadBanks()
	{
		string[] soundBanks = m_SoundBanks;
		for (int i = 0; i < soundBanks.Length; i++)
		{
			SoundBanksManager.UnloadBank(soundBanks[i]);
		}
		OnUnloadBanks();
	}

	protected virtual void OnUnloadBanks()
	{
	}

	public bool TrySchedule(AskSchedulingEntry schedulingEntry, out string reason)
	{
		reason = "no reason";
		AskWrapper wrapper = schedulingEntry.Wrapper;
		AsksSet asksSet = wrapper?.AsksSet;
		if (asksSet == null)
		{
			reason = "ask is null";
			return false;
		}
		if (asksSet.DoNotPlayWhileAlone && Game.Instance.Player.CapitalPartyMode)
		{
			reason = "ask is DoNotPlayWhileAlone and we in CapitalPartyMode";
			return false;
		}
		if (asksSet.Entries == null || asksSet.Entries.Length == 0 || wrapper.IsOnCooldown)
		{
			reason = "ask entries are empty or on cooldown";
			return false;
		}
		if (m_Queue.IsInQueue(wrapper))
		{
			reason = "same ask already in queue";
			return false;
		}
		float num = 1f;
		AbstractUnitEntity unit = Unit;
		if (unit != null && unit.IsPlayerFaction)
		{
			num = SettingsRoot.Sound.VoicedAskFrequency.GetValue() switch
			{
				VoiceAskFrequency.Never => 0f, 
				VoiceAskFrequency.Occasionally => num / 6f, 
				VoiceAskFrequency.Frequently => num / 2f, 
				VoiceAskFrequency.Constantly => 1f, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		num = asksSet.Chance * num;
		if (!asksSet.CheckBarkChance(num))
		{
			reason = "ask didnt pass chance test";
			return false;
		}
		if (schedulingEntry.IsInterruptCurrent)
		{
			DiscardCurrentActiveBark();
			if (schedulingEntry.IsClearsQueue)
			{
				PFLog.VO.Log("[VO] Ask " + (schedulingEntry.Wrapper.Type ?? "unknown") + " Cleared Queue \n Caster: " + schedulingEntry.AsksContext.Caster?.Name + " \nTarget: " + schedulingEntry.AsksContext.Target?.Entity?.Name);
				m_Queue.ClearQueue();
			}
		}
		if (schedulingEntry.CannotBePlayedIfQueueNotEmpty && (m_CurrentlyActiveAsk != null || !m_Queue.IsEmpty))
		{
			reason = "ask is CannotBePlayedIfQueueNotEmpty and other ask is playing/in queue";
			return false;
		}
		AskSchedulingEntry currentlyActiveAsk = m_CurrentlyActiveAsk;
		if (currentlyActiveAsk != null && currentlyActiveAsk.IsForbidQueueing)
		{
			reason = "current ask forbids queueing new asks";
			return false;
		}
		m_Queue.Schedule(schedulingEntry);
		reason = "success";
		return true;
	}

	public void TryPlayNextAsk()
	{
		if (m_CurrentlyActiveAsk == null && m_Queue.TryPopFromQueue(out var nextEntry) && Unit != null && !Unit.IsDeadOrUnconscious)
		{
			AsksSet asksSet = nextEntry.Wrapper.AsksSet;
			AskEntry askEntry = SelectRandomEntry(asksSet, nextEntry.Wrapper, Unit, nextEntry.AsksContext);
			PFLog.VO.Log(string.Format("[VO] Playing Ask {0} \n Caster: {1} \nTarget: {2} \n ChoosenEntry: {3}", nextEntry.Wrapper.Type ?? "unknown", nextEntry.AsksContext.Caster?.Name, nextEntry.AsksContext.Target?.Entity?.Name, askEntry.Text));
			float num = PFStatefulRandom.Visuals.Sounds.Range(asksSet.DelayMin, asksSet.DelayMax);
			if (num < 0.01f || Unit == null)
			{
				Play(askEntry, nextEntry);
			}
			else
			{
				m_CurrentlyPlayAfterCoroutine = Unit.View.StartCoroutine(PlayAfter(askEntry, nextEntry, num, synced: false));
			}
		}
	}

	public void DiscardCurrentActiveBark()
	{
		SoundEventsManager.StopPlayingById(m_CurrentlyPlayingId);
		m_CurrentlyActiveAsk = null;
		if (m_CurrentlyPlayAfterCoroutine != null)
		{
			Unit.View.StopCoroutine(m_CurrentlyPlayAfterCoroutine);
			m_CurrentlyPlayAfterCoroutine = null;
		}
	}

	private static AskEntry SelectRandomEntry(AsksSet bark, AskWrapper wrapper, AbstractUnitEntity target, AsksContext context)
	{
		if (bark.Entries.Length == 1)
		{
			return bark.Entries[0];
		}
		MechanicEntity caster = context.Caster;
		TargetWrapper clickedTarget = context.Target ?? ((TargetWrapper)target);
		using ((caster != null) ? EvalContext.Build().Blueprint(context.AbilityBlueprint ?? target.MainFact.Blueprint).Caster(caster)
			.ClickedTarget(clickedTarget)
			.Push() : default(EvalContext.StackFrameHandle))
		{
			IOrderedEnumerable<IGrouping<int, (int Index, AskEntry ask)>> orderedEnumerable = from x in bark.Entries.AllWithIndex.Where(delegate((int Index, AskEntry ask) x)
				{
					if (x.ask.IsExist && !x.ask.Locked)
					{
						ConditionsChecker condition = x.ask.Condition;
						if (condition != null && condition.HasConditions)
						{
							return x.ask.Condition.Check();
						}
						return true;
					}
					return false;
				})
				group x by x.ask.HasCondition ? x.ask.ConditionPriority : 0 into x
				orderby x.Key descending
				select x;
			(int, AskEntry) tuple = default((int, AskEntry));
			foreach (IGrouping<int, (int, AskEntry)> item in orderedEnumerable)
			{
				tuple = SelectRandom(item, wrapper);
				if (tuple.Item2 != null)
				{
					break;
				}
			}
			AskEntry askEntry;
			int entryIndex;
			if (tuple.Item2 != null)
			{
				askEntry = tuple.Item2;
				(entryIndex, _) = tuple;
			}
			else
			{
				askEntry = bark.Entries.FirstOrDefault();
				entryIndex = 0;
			}
			wrapper.SetExclusionCounter(entryIndex, askEntry.ExcludeTime);
			return askEntry;
		}
	}

	private static (int Index, AskEntry Ask) SelectRandom(IEnumerable<(int Index, AskEntry ask)> entries, AskWrapper wrapper)
	{
		(int, AskEntry) result = default((int, AskEntry));
		(int, AskEntry) result2 = default((int, AskEntry));
		float num = 0f;
		foreach (var item in entries.ToList())
		{
			if (result2.Item2 == null)
			{
				result2 = (item.Index, item.ask);
			}
			if (wrapper.GetExclusionCounter(item.Index) > 0)
			{
				wrapper.DecrementExclusionCounter(item.Index);
				continue;
			}
			float randomWeight = item.ask.RandomWeight;
			if (PFStatefulRandom.Visuals.Sounds.Range(0f, num + randomWeight) >= num)
			{
				result = (item.Index, item.ask);
			}
			num += randomWeight;
		}
		if (result.Item2 == null)
		{
			return result2;
		}
		return result;
	}

	private IEnumerator PlayAfter(AskEntry entry, AskSchedulingEntry schedulingEntry, float delay, bool synced = true)
	{
		schedulingEntry.Wrapper.IsPlaying = true;
		m_CurrentlyActiveAsk = schedulingEntry;
		yield return new WaitForSeconds(delay);
		Play(entry, schedulingEntry, synced);
	}

	private void Play(AskEntry entry, AskSchedulingEntry schedulingEntry, bool synced = false)
	{
		schedulingEntry.Wrapper.IsPlaying = true;
		m_CurrentlyActiveAsk = schedulingEntry;
		bool flag = schedulingEntry.Is2D;
		AskWrapper wrapper = schedulingEntry.Wrapper;
		if (!string.IsNullOrEmpty(entry.AkEvent) && !flag && Unit == null)
		{
			PFLog.Default.Warning("Can not play " + entry.AkEvent + " in 3D cause no unit entity. Will play in 2D");
			flag = true;
		}
		if (!string.IsNullOrEmpty(entry.AkEvent) && Unit != null && Game.Instance.Controllers.VoiceOverController.CanPlayAsk(Unit.VoGuid, flag ? SoundState.Get2DSoundObject() : Unit.View.gameObject))
		{
			VoiceOverStatus status = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(entry.AkEvent, Unit.VoGuid, VoiceOverType.Ask, flag ? SoundState.Get2DSoundObject() : Unit.View.gameObject);
			BindPlayback(status, schedulingEntry);
		}
		else if (entry.Text != null && Unit != null)
		{
			if (wrapper.AsksSet.ShowOnScreen)
			{
				IBarkHandle barkHandle = BarkPlayer.Bark(Unit, entry.Text, VoiceOverType.Ask, Unit.VoGuid, -1f, null, synced);
				BindPlayback(barkHandle.VoiceOverStatus, schedulingEntry);
				return;
			}
			EventBus.RaiseEvent((IEntity)Unit, (Action<ICombatLogBarkHandler>)delegate(ICombatLogBarkHandler h)
			{
				h.HandleOnShowBark(entry.Text);
			}, isCheckRuntime: true);
			VoiceOverStatus status2 = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(entry.Text, Unit.VoGuid, VoiceOverType.Ask, flag ? SoundState.Get2DSoundObject() : Unit.View.gameObject);
			BindPlayback(status2, schedulingEntry);
		}
		else
		{
			OnEndCallback(schedulingEntry);
		}
	}

	private void BindPlayback(VoiceOverStatus status, AskSchedulingEntry entry)
	{
		if (status != null && status.PlayingSoundId != 0)
		{
			m_CurrentlyPlayingId = status.PlayingSoundId;
			status.Ended += delegate
			{
				OnEndCallback(entry);
			};
		}
		else
		{
			m_CurrentlyPlayingId = 0u;
			OnEndCallback(entry);
		}
	}

	private void OnEndCallback(AskSchedulingEntry entry)
	{
		entry.Wrapper.IsPlaying = false;
		entry.Wrapper.LastPlayTime = (float)Game.Instance.Controllers.TimeController.RealTime.TotalSeconds;
		m_CurrentlyActiveAsk = null;
		entry.Callback?.Invoke(entry.AsksContext);
		TryPlayNextAsk();
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void OverridePlayerAsks(BlueprintUnitAsksList asksList)
	{
		Game.Instance.Player.MainCharacterEntity.Asks.SetOverride(asksList);
		Game.Instance.Player.MainCharacterEntity.View.UpdateAsks();
	}

	public static AsksContext CreateAsksContext()
	{
		return new AsksContext
		{
			AbilityBlueprint = EvalContext.Current.AbilityBlueprint,
			Caster = EvalContext.Current.Caster,
			Target = EvalContext.Current.ClickedTarget
		};
	}
}
