using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums.Sound;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Sound.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class UnitAsksManager
{
	private AskWrapper _mCurrentlyActiveAsk;

	private uint m_CurrentlyPlayingId;

	public readonly AbstractUnitEntity Unit;

	private readonly string[] m_SoundBanks;

	public readonly AskWrapper Aggro;

	public readonly AskWrapper Pain;

	public readonly AskWrapper Death;

	public readonly AskWrapper Unconscious;

	public readonly AskWrapper CriticalHit;

	public readonly AskWrapper TraumaApplied;

	public readonly AskWrapper Order;

	public readonly AskWrapper Selected;

	public readonly AskWrapper CantDo;

	public readonly AskWrapper CheckSuccess;

	public readonly AskWrapper CheckFail;

	public readonly AskWrapper Discovery;

	public readonly AskWrapper OrderMove;

	public readonly AskWrapper OrderMoveExploration;

	public readonly AskWrapper EnemyDeath;

	public readonly AskWrapper PartyMemberUnconscious;

	public readonly AskWrapper PsychicPhenomena;

	public readonly AskWrapper PerilsOfTheWarp;

	public readonly AskWrapper HealingAlly;

	public readonly AskWrapper BeingHealed;

	public readonly AskWrapper EnemyMassDeath;

	public readonly AskWrapper FriendlyFire;

	public readonly AskWrapper MoraleBroken;

	public readonly AskWrapper MoraleHeroic;

	public readonly AskWrapper ChannellingOn;

	public readonly AskWrapper ChannellingReaction;

	public readonly AskWrapper ChannellingOff;

	public readonly AskWrapper ChannellingSuccessfulRelease;

	public readonly AskWrapper WeAreLoosing;

	public readonly AskWrapper WeAreWinning;

	public readonly AskWrapper WeAreLostByMorale;

	public readonly AskWrapper ArmorBroken;

	public readonly AskWrapper DetectiveCanStudyClue;

	public readonly AskWrapper DetectiveNewConclusionAvailable;

	public readonly AskWrapper DetectiveSearch;

	public readonly AskWrapper DetectiveTracesFound;

	public readonly AskWrapper DetectiveReminder;

	public readonly AskWrapper DetectiveReconstructionFind;

	public readonly AskWrapper DetectiveReconstructionReady;

	public readonly AskWrapper DetectiveSignalFound;

	public readonly AskWrapper[] PartyMemberUnconsciousPersonalized;

	public readonly AskWrapper[] AnimationBarks;

	public AskWrapper SelectAnimationBark(MappedAnimationEventType evt)
	{
		return (from b in AnimationBarks.EmptyIfNull()
			where ((AnimationAsk)b.Bark).AnimationEvent == evt
			select b).Random(PFStatefulRandom.Visuals.Sounds);
	}

	public UnitAsksManager(AbstractUnitEntity unit, BlueprintUnitAsksList asksList)
	{
		m_SoundBanks = asksList.SoundBanks;
		Unit = unit;
		Aggro = Wrap(asksList.Aggro);
		Pain = Wrap(asksList.Pain);
		Death = Wrap(asksList.Death);
		Unconscious = Wrap(asksList.Unconscious);
		CriticalHit = Wrap(asksList.CriticalHit);
		TraumaApplied = Wrap(asksList.TraumaApplied);
		Order = Wrap(asksList.Order);
		Selected = Wrap(asksList.Selected);
		CantDo = Wrap(asksList.CantDo);
		CheckSuccess = Wrap(asksList.CheckSuccess);
		CheckFail = Wrap(asksList.CheckFail);
		Discovery = Wrap(asksList.Discovery);
		OrderMove = Wrap(asksList.OrderMove);
		OrderMoveExploration = Wrap(asksList.OrderMoveExploration);
		EnemyDeath = Wrap(asksList.EnemyDeath);
		PartyMemberUnconscious = Wrap(asksList.PartyMemberUnconscious);
		PsychicPhenomena = Wrap(asksList.PsychicPhenomena);
		PerilsOfTheWarp = Wrap(asksList.PerilsOfTheWarp);
		HealingAlly = Wrap(asksList.HealingAlly);
		BeingHealed = Wrap(asksList.BeingHealed);
		EnemyMassDeath = Wrap(asksList.EnemyMassDeath);
		FriendlyFire = Wrap(asksList.FriendlyFire);
		MoraleBroken = Wrap(asksList.MoraleBroken);
		MoraleHeroic = Wrap(asksList.MoraleHeroic);
		ChannellingOn = Wrap(asksList.ChannellingOn);
		ChannellingReaction = Wrap(asksList.ChannellingReaction);
		ChannellingOff = Wrap(asksList.ChannellingOff);
		ChannellingSuccessfulRelease = Wrap(asksList.ChannellingSuccessfulRelease);
		WeAreLoosing = Wrap(asksList.WeAreLoosing);
		WeAreWinning = Wrap(asksList.WeAreWinning);
		WeAreLostByMorale = Wrap(asksList.WeAreLostByMorale);
		ArmorBroken = Wrap(asksList.ArmorBroken);
		DetectiveCanStudyClue = Wrap(asksList.DetectiveCanStudyClue);
		DetectiveNewConclusionAvailable = Wrap(asksList.DetectiveNewConclusionAvailable);
		DetectiveSearch = Wrap(asksList.DetectiveSearch);
		DetectiveTracesFound = Wrap(asksList.DetectiveTracesFound);
		DetectiveReminder = Wrap(asksList.DetectiveReminder);
		DetectiveReconstructionFind = Wrap(asksList.DetectiveReconstructionFind);
		DetectiveReconstructionReady = Wrap(asksList.DetectiveReconstructionReady);
		DetectiveSignalFound = Wrap(asksList.DetectiveSignalFound);
		PartyMemberUnconsciousPersonalized = asksList.PartyMemberUnconsciousPersonalized.EmptyIfNull().Select(Wrap).ToArray();
		AnimationBarks = asksList.AnimationAsks.EmptyIfNull().Select(Wrap).ToArray();
	}

	private AskWrapper Wrap(AsksSet bark)
	{
		return new AskWrapper(bark, this);
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

	public bool Schedule(AskWrapper wrapper, bool is2D = false, AskCallback callback = null, AsksContext asksContext = null)
	{
		AsksSet asksSet = wrapper?.Bark;
		if (asksSet == null)
		{
			return false;
		}
		if (asksSet.DoNotPlayWhileAlone && Game.Instance.Player.CapitalPartyMode)
		{
			return false;
		}
		if (asksSet.Entries == null || asksSet.Entries.Length == 0 || wrapper.IsOnCooldown)
		{
			return false;
		}
		if (wrapper.Bark.EnablePrioritization)
		{
			UnitAsksPriorityHelper.RegisterBark(wrapper);
			AskWrapper currentHighestPriorityBark = UnitAsksPriorityHelper.GetCurrentHighestPriorityBark(wrapper.Bark.PrioritizationGroup);
			if (wrapper != currentHighestPriorityBark && wrapper.Bark.Priority >= currentHighestPriorityBark.Bark.Priority)
			{
				return false;
			}
		}
		if (!asksSet.InterruptOthers && _mCurrentlyActiveAsk != null)
		{
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
			return false;
		}
		AskEntry entry = SelectRandomEntry(asksSet, Unit, asksContext);
		float num2 = PFStatefulRandom.Visuals.Sounds.Range(asksSet.DelayMin, asksSet.DelayMax);
		if (num2 < 0.01f || Unit == null)
		{
			Play(wrapper, entry, is2D, callback, asksContext);
		}
		else
		{
			Unit.View.StartCoroutine(PlayAfter(wrapper, num2, entry, is2D, callback, synced: false));
		}
		return true;
	}

	public void DiscardCurrentActiveBark()
	{
		SoundEventsManager.StopPlayingById(m_CurrentlyPlayingId);
		_mCurrentlyActiveAsk = null;
	}

	private static AskEntry SelectRandomEntry(AsksSet bark, AbstractUnitEntity target, AsksContext context)
	{
		if (bark.Entries.Length == 1)
		{
			return bark.Entries[0];
		}
		AskEntry askEntry = null;
		MechanicEntity caster = context.Caster;
		TargetWrapper clickedTarget = context.Target ?? ((TargetWrapper)target);
		MechanicsContext mechanicsContext = null;
		DisposableBag disposableBag = null;
		if (caster != null)
		{
			mechanicsContext = MechanicsContext.Claim(context.AbilityBlueprint ?? target.MainFact.Blueprint, caster, null, null, clickedTarget);
			disposableBag = mechanicsContext.SetScope();
		}
		foreach (IGrouping<int, AskEntry> item in from x in bark.Entries.Where(delegate(AskEntry x)
			{
				if (!x.IsEmpty && !x.Locked)
				{
					ConditionsChecker condition = x.Condition;
					if (condition != null && condition.HasConditions)
					{
						return x.Condition.Check();
					}
					return true;
				}
				return false;
			})
			group x by x.HasCondition ? x.ConditionPriority : 0 into x
			orderby x.Key descending
			select x)
		{
			askEntry = SelectRandom(item);
			if (askEntry != null)
			{
				break;
			}
		}
		if (askEntry == null)
		{
			askEntry = bark.Entries.FirstOrDefault();
		}
		askEntry.ExclusionCounter = askEntry.ExcludeTime;
		mechanicsContext?.Dispose();
		disposableBag?.Dispose();
		return askEntry;
	}

	private static AskEntry SelectRandom(IEnumerable<AskEntry> entries)
	{
		AskEntry askEntry = null;
		float num = 0f;
		List<AskEntry> list = entries.ToList();
		foreach (AskEntry item in list)
		{
			if (item.ExclusionCounter > 0)
			{
				item.ExclusionCounter--;
				continue;
			}
			float randomWeight = item.RandomWeight;
			if (PFStatefulRandom.Visuals.Sounds.Range(0f, num + randomWeight) >= num)
			{
				askEntry = item;
			}
			num += randomWeight;
		}
		if (askEntry == null)
		{
			askEntry = list.FirstOrDefault();
		}
		return askEntry;
	}

	private IEnumerator PlayAfter(AskWrapper wrapper, float delay, AskEntry entry, bool is2D, AskCallback callback = null, bool synced = true)
	{
		wrapper.IsPlaying = true;
		_mCurrentlyActiveAsk = wrapper;
		yield return new WaitForSeconds(delay);
		Play(wrapper, entry, is2D, callback, null, synced);
	}

	private void Play(AskWrapper wrapper, AskEntry entry, bool is2D, AskCallback callback = null, AsksContext askContext = null, bool synced = false)
	{
		wrapper.IsPlaying = true;
		_mCurrentlyActiveAsk = wrapper;
		if (!string.IsNullOrEmpty(entry.AkEvent) && !is2D && Unit == null)
		{
			PFLog.Default.Warning("Can not play " + entry.AkEvent + " in 3D cause no unit entity. Will play in 2D");
			is2D = true;
		}
		if (!string.IsNullOrEmpty(entry.AkEvent) && Unit != null && Game.Instance.Controllers.VoiceOverController.CanPlayAsk(Unit.VoGuid, is2D ? SoundState.Get2DSoundObject() : Unit.View.gameObject))
		{
			VoiceOverStatus voiceOverStatus = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(entry.AkEvent, Unit.VoGuid, VoiceOverType.Ask, is2D ? SoundState.Get2DSoundObject() : Unit.View.gameObject);
			if (voiceOverStatus != null)
			{
				voiceOverStatus.Ended += OnEndCallback;
				m_CurrentlyPlayingId = voiceOverStatus.PlayingSoundId;
			}
			else
			{
				m_CurrentlyPlayingId = 0u;
			}
			if (voiceOverStatus == null || m_CurrentlyPlayingId == 0)
			{
				OnEndCallback();
			}
		}
		else
		{
			if (!(entry.Text != null) || Unit == null)
			{
				return;
			}
			if (wrapper.Bark.ShowOnScreen)
			{
				IBarkHandle barkHandle = BarkPlayer.Bark(Unit, entry.Text.String, VoiceOverType.Ask, Unit.VoGuid, -1f, null, synced);
				m_CurrentlyPlayingId = barkHandle.VoiceOverStatus.PlayingSoundId;
				if (barkHandle.VoiceOverStatus != null)
				{
					barkHandle.VoiceOverStatus.Ended += OnEndCallback;
				}
				else
				{
					OnEndCallback();
				}
			}
			else
			{
				EventBus.RaiseEvent((IEntity)Unit, (Action<ICombatLogBarkHandler>)delegate(ICombatLogBarkHandler h)
				{
					h.HandleOnShowBark(entry.Text.String);
				}, isCheckRuntime: true);
				VoiceOverStatus voiceOverStatus2 = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(entry.Text.String, Unit.VoGuid, VoiceOverType.Ask, is2D ? SoundState.Get2DSoundObject() : Unit.View.gameObject);
				if (voiceOverStatus2 != null)
				{
					voiceOverStatus2.Ended += OnEndCallback;
					m_CurrentlyPlayingId = voiceOverStatus2.PlayingSoundId;
				}
				else
				{
					m_CurrentlyPlayingId = 0u;
				}
				if (voiceOverStatus2 == null || m_CurrentlyPlayingId == 0)
				{
					OnEndCallback();
				}
			}
			if (!wrapper.IsPlaying)
			{
				wrapper.LastPlayTime = (float)Game.Instance.Controllers.TimeController.RealTime.TotalSeconds;
			}
		}
		void OnEndCallback()
		{
			wrapper.IsPlaying = false;
			wrapper.LastPlayTime = (float)Game.Instance.Controllers.TimeController.RealTime.TotalSeconds;
			_mCurrentlyActiveAsk = null;
			callback?.Invoke(askContext);
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void OverridePlayerAsks(BlueprintUnitAsksList asksList)
	{
		Game.Instance.Player.MainCharacterEntity.Asks.SetOverride(asksList);
		Game.Instance.Player.MainCharacterEntity.View.UpdateAsks();
	}

	public static AsksContext CreateAsksContext()
	{
		AsksContext asksContext = new AsksContext();
		if (SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current is AbilityExecutionContext abilityExecutionContext)
		{
			asksContext.AbilityBlueprint = abilityExecutionContext.AbilityBlueprint;
		}
		if (SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current != null)
		{
			asksContext.Caster = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current.MaybeCaster;
			asksContext.Target = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current.ClickedTarget;
		}
		return asksContext;
	}
}
