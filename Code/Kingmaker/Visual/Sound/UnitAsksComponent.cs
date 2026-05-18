using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[Obsolete("Use fields in BlueprintAsksList itself")]
[AllowedOn(typeof(BlueprintUnitAsksList))]
[TypeId("95968ed93cfb4dc45b8141526f932ba4")]
public class UnitAsksComponent : BlueprintComponent, IUnlockableFlagReference
{
	[AkBankReference]
	public string[] SoundBanks = new string[0];

	[AkEventReference]
	public string PreviewSound = "";

	public Bark Aggro = new Bark();

	public Bark Pain = new Bark();

	public Bark Death = new Bark();

	public Bark Unconscious = new Bark();

	public Bark CriticalHit = new Bark();

	public Bark TraumaApplied = new Bark();

	public Bark Order = new Bark();

	public Bark Selected = new Bark();

	public Bark CantDo = new Bark();

	public Bark CheckSuccess = new Bark();

	public Bark CheckFail = new Bark();

	public Bark Discovery = new Bark();

	public Bark OrderMove = new Bark();

	public Bark OrderMoveExploration = new Bark();

	public Bark EnemyDeath = new Bark();

	public Bark PartyMemberUnconscious = new Bark();

	public Bark PsychicPhenomena = new Bark();

	public Bark PerilsOfTheWarp = new Bark();

	public Bark HealingAlly = new Bark();

	public Bark BeingHealed = new Bark();

	public Bark EnemyMassDeath = new Bark();

	public Bark FriendlyFire = new Bark();

	[Tooltip("Реакция юнита на переход в состояние морали Broken ")]
	public Bark MoraleBroken = new Bark();

	[Tooltip("Реакция юнита на переход в состояние морали Heroic ")]
	public Bark MoraleHeroic = new Bark();

	[Tooltip("Когда персонаж кастует абилку с концентрацией")]
	public Bark ChannellingOn = new Bark();

	[Tooltip("Реакция пати на то, что враг концентрируется")]
	public Bark ChannellingReaction = new Bark();

	[Tooltip("Реакция врага на то, что ему сбили концентрацию")]
	public Bark ChannellingOff = new Bark();

	[Tooltip("Реакция врага на то, что у него успешно закончилась концентрация")]
	public Bark ChannellingSuccessfulRelease = new Bark();

	[Tooltip("Реакция пати на Power Balance, когда проигрывают")]
	public Bark WeAreLoosing = new Bark();

	[Tooltip("Реакция пати на Power Balance, когда выигрывают")]
	public Bark WeAreWinning = new Bark();

	[Tooltip("Реакция одного врага на то, что враги сдались по морали")]
	public Bark WeAreLostByMorale = new Bark();

	[Tooltip("Реакция юнита на то, что ему уроном сбили броню")]
	public Bark BrokenArmour = new Bark();

	[Tooltip("Реакция члена пати на то, что для него стала доступна Study в улике")]
	public Bark DetectiveCanStudyClue = new Bark();

	[Tooltip("Реакция сервочерепа на то, что стал доступен новый Conclusion")]
	public Bark DetectiveNewConclusionAvailable = new Bark();

	[Tooltip("Реакция члена пати на приближение к ProximityAskComponent с параметром DetectiveSearch")]
	public Bark DetectiveSearch = new Bark();

	[Tooltip("Реакция члена пати первый интеракт со цепочкой следов")]
	public Bark DetectiveTracesFound = new Bark();

	[Tooltip("Реакция члена пати при заходе на локацию (загрузке), если на зоне есть незавершённая цепочка следов")]
	public Bark DetectiveReminder = new Bark();

	[Tooltip("Реакция члена пати на приближение к ProximityAskComponent с параметром DetectiveReconstructionFind")]
	public Bark DetectiveReconstructionFind = new Bark();

	[Tooltip("Реакция сервочерепа на срабатывание условия в DetectiveBarktings")]
	public Bark DetectiveReconstructionReady = new Bark();

	[Tooltip("Реакция сервочерепа на включение сигнала, если он ближайший к игроку")]
	public Bark DetectiveSignalFound = new Bark();

	public PersonalizedBark[] PartyMemberUnconsciousPersonalized;

	public AnimationBark[] AnimationBarks;

	public bool TryGetAskByType(AskType type, out Bark ask)
	{
		ask = type switch
		{
			AskType.AggroBattleCry => Aggro, 
			AskType.Pain => Pain, 
			AskType.Death => Death, 
			AskType.Unconscious => Unconscious, 
			AskType.CriticalHit => CriticalHit, 
			AskType.TraumaApplied => TraumaApplied, 
			AskType.Order => Order, 
			AskType.Select => Selected, 
			AskType.CantDo => CantDo, 
			AskType.CheckSuccessful => CheckSuccess, 
			AskType.CheckFailed => CheckFail, 
			AskType.Discovery => Discovery, 
			AskType.MoveInCombat => OrderMove, 
			AskType.MoveInExploration => OrderMoveExploration, 
			AskType.EnemyDeath => EnemyDeath, 
			AskType.PartyMemberUnconsciousGeneral => PartyMemberUnconscious, 
			AskType.PsychicPhenomena => PsychicPhenomena, 
			AskType.SupportAnAlly => HealingAlly, 
			AskType.BeingSupported => BeingHealed, 
			AskType.EnemyMassDeath => EnemyMassDeath, 
			AskType.FriendlyFire => FriendlyFire, 
			AskType.BrokenMorale => MoraleBroken, 
			AskType.HeroicMorale => MoraleHeroic, 
			AskType.ChannellingOn => ChannellingOn, 
			AskType.ChannellingOff => ChannellingOff, 
			AskType.ChannellingReaction => ChannellingReaction, 
			AskType.ChannellingSuccessfulRelease => ChannellingSuccessfulRelease, 
			AskType.WeAreLoosing => WeAreLoosing, 
			AskType.WeAreWinning => WeAreWinning, 
			AskType.WeAreLostByMorale => WeAreLostByMorale, 
			AskType.BrokenArmour => BrokenArmour, 
			AskType.ClueCanBeProcessed => DetectiveCanStudyClue, 
			AskType.ConclusionAvailable => DetectiveNewConclusionAvailable, 
			AskType.DetectiveSearch => DetectiveSearch, 
			AskType.TracesFound => DetectiveTracesFound, 
			AskType.DetectiveReminder => DetectiveReminder, 
			AskType.DetectiveReconstructionFound => DetectiveReconstructionFind, 
			AskType.DetectiveReconstructionReady => DetectiveReconstructionReady, 
			AskType.SignalFound => DetectiveSignalFound, 
			_ => null, 
		};
		return ask != null;
	}

	public void PlayPreview()
	{
		GameObject gameObject = UIDollRooms.Instance.gameObject;
		SoundEventsManager.PostEvent(PreviewSound, gameObject);
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		List<Bark> list = new List<Bark>();
		list.Add(Aggro);
		list.Add(Pain);
		list.Add(Death);
		list.Add(Unconscious);
		list.Add(CriticalHit);
		list.Add(TraumaApplied);
		list.Add(Order);
		list.Add(Selected);
		list.Add(CantDo);
		list.Add(CheckSuccess);
		list.Add(CheckFail);
		list.Add(Discovery);
		list.Add(OrderMove);
		list.Add(OrderMoveExploration);
		list.Add(EnemyDeath);
		list.Add(PartyMemberUnconscious);
		list.Add(PsychicPhenomena);
		list.Add(PerilsOfTheWarp);
		list.Add(HealingAlly);
		list.Add(BeingHealed);
		list.Add(EnemyMassDeath);
		list.Add(FriendlyFire);
		list.Add(MoraleBroken);
		list.Add(MoraleHeroic);
		list.Add(ChannellingOn);
		list.Add(ChannellingOff);
		list.Add(ChannellingReaction);
		list.Add(ChannellingSuccessfulRelease);
		list.Add(WeAreLoosing);
		list.Add(WeAreWinning);
		list.Add(WeAreLostByMorale);
		list.Add(BrokenArmour);
		list.Add(DetectiveCanStudyClue);
		list.Add(DetectiveNewConclusionAvailable);
		list.Add(DetectiveSearch);
		list.Add(DetectiveTracesFound);
		list.Add(DetectiveReminder);
		list.Add(DetectiveReconstructionFind);
		list.Add(DetectiveReconstructionReady);
		list.Add(DetectiveSignalFound);
		list.AddRange(AnimationBarks);
		list.AddRange(PartyMemberUnconsciousPersonalized);
		if ((from f in list.SelectMany((Bark b) => b.Entries).SelectMany((AskEntry e) => e.ExcludedFlags.Concat(e.RequiredFlags))
			where f
			select f).Contains(flag))
		{
			return UnlockableFlagReferenceType.Check;
		}
		return UnlockableFlagReferenceType.None;
	}

	public IEnumerable<AskEntry> GetAllAsks()
	{
		foreach (AskType item in Enum.GetValues(typeof(AskType)).Cast<AskType>())
		{
			if (TryGetAskByType(item, out var ask) && ask.Entries != null)
			{
				AskEntry[] entries = ask.Entries;
				for (int i = 0; i < entries.Length; i++)
				{
					yield return entries[i];
				}
			}
		}
		if (AnimationBarks != null)
		{
			AnimationBark[] animationBarks = AnimationBarks;
			foreach (AnimationBark animationBark in animationBarks)
			{
				AskEntry[] entries = animationBark.Entries;
				for (int j = 0; j < entries.Length; j++)
				{
					yield return entries[j];
				}
			}
		}
		if (PartyMemberUnconsciousPersonalized == null)
		{
			yield break;
		}
		PersonalizedBark[] partyMemberUnconsciousPersonalized = PartyMemberUnconsciousPersonalized;
		foreach (PersonalizedBark personalizedBark in partyMemberUnconsciousPersonalized)
		{
			AskEntry[] entries = personalizedBark.Entries;
			for (int j = 0; j < entries.Length; j++)
			{
				yield return entries[j];
			}
		}
	}
}
