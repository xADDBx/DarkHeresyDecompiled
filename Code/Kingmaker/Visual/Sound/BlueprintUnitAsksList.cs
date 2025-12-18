using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Localization;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[TypeId("74167fee930157a49b88d49d1689ee9a")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintUnitAsksList : BlueprintScriptableObject
{
	public LocalizedString DisplayName;

	public AskComponentType AskType;

	[AkBankReference]
	public string[] SoundBanks = new string[0];

	[AkEventReference]
	public string PreviewSound = "";

	public AsksSet Aggro = new AsksSet();

	public AsksSet Pain = new AsksSet();

	public AsksSet Death = new AsksSet();

	public AsksSet Unconscious = new AsksSet();

	public AsksSet CriticalHit = new AsksSet();

	public AsksSet TraumaApplied = new AsksSet();

	public AsksSet Order = new AsksSet();

	public AsksSet Selected = new AsksSet();

	public AsksSet CantDo = new AsksSet();

	public AsksSet CheckSuccess = new AsksSet();

	public AsksSet CheckFail = new AsksSet();

	public AsksSet Discovery = new AsksSet();

	public AsksSet OrderMove = new AsksSet();

	public AsksSet OrderMoveExploration = new AsksSet();

	public AsksSet EnemyDeath = new AsksSet();

	public AsksSet PartyMemberUnconscious = new AsksSet();

	public AsksSet PsychicPhenomena = new AsksSet();

	public AsksSet PerilsOfTheWarp = new AsksSet();

	public AsksSet HealingAlly = new AsksSet();

	public AsksSet BeingHealed = new AsksSet();

	public AsksSet EnemyMassDeath = new AsksSet();

	public AsksSet FriendlyFire = new AsksSet();

	[Tooltip("Реакция юнита на переход в состояние морали Broken ")]
	public AsksSet MoraleBroken = new AsksSet();

	[Tooltip("Реакция юнита на переход в состояние морали Heroic ")]
	public AsksSet MoraleHeroic = new AsksSet();

	[Tooltip("Когда персонаж кастует абилку с концентрацией")]
	public AsksSet ChannellingOn = new AsksSet();

	[Tooltip("Реакция пати на то, что враг концентрируется")]
	public AsksSet ChannellingReaction = new AsksSet();

	[Tooltip("Реакция врага на то, что ему сбили концентрацию")]
	public AsksSet ChannellingOff = new AsksSet();

	[Tooltip("Реакция врага на то, что у него успешно закончилась концентрация")]
	public AsksSet ChannellingSuccessfulRelease = new AsksSet();

	[Tooltip("Реакция пати на Power Balance, когда проигрывают")]
	public AsksSet WeAreLoosing = new AsksSet();

	[Tooltip("Реакция пати на Power Balance, когда выигрывают")]
	public AsksSet WeAreWinning = new AsksSet();

	[Tooltip("Реакция одного врага на то, что враги сдались по морали")]
	public AsksSet WeAreLostByMorale = new AsksSet();

	[Tooltip("Реакция юнита на то, что ему уроном сбили броню")]
	public AsksSet ArmorBroken = new AsksSet();

	[Tooltip("Реакция члена пати на то, что для него стала доступна Study в улике")]
	public AsksSet DetectiveCanStudyClue = new AsksSet();

	[Tooltip("Реакция сервочерепа на то, что стал доступен новый Conclusion")]
	public AsksSet DetectiveNewConclusionAvailable = new AsksSet();

	[Tooltip("Реакция члена пати на приближение к ProximityAskComponent с параметром DetectiveSearch")]
	public AsksSet DetectiveSearch = new AsksSet();

	[Tooltip("Реакция члена пати первый интеракт со цепочкой следов")]
	public AsksSet DetectiveTracesFound = new AsksSet();

	[Tooltip("Реакция члена пати при заходе на локацию (загрузке), если на зоне есть незавершённая цепочка следов")]
	public AsksSet DetectiveReminder = new AsksSet();

	[Tooltip("Реакция члена пати на приближение к ProximityAskComponent с параметром DetectiveReconstructionFind")]
	public AsksSet DetectiveReconstructionFind = new AsksSet();

	[Tooltip("Реакция сервочерепа на срабатывание условия в DetectiveAsksSettings")]
	public AsksSet DetectiveReconstructionReady = new AsksSet();

	[Tooltip("Реакция сервочерепа на включение сигнала, если он ближайший к игроку")]
	public AsksSet DetectiveSignalFound = new AsksSet();

	public PersonalizedAsksContainer PartyMemberUnconsciousPersonalized = new PersonalizedAsksContainer();

	public AnimationAsksContainer AnimationAsks = new AnimationAsksContainer();

	public bool TryGetAskByType(AskType type, out AsksSet ask)
	{
		ask = type switch
		{
			Kingmaker.Visual.Sound.AskType.Aggro => Aggro, 
			Kingmaker.Visual.Sound.AskType.Pain => Pain, 
			Kingmaker.Visual.Sound.AskType.Death => Death, 
			Kingmaker.Visual.Sound.AskType.Unconscious => Unconscious, 
			Kingmaker.Visual.Sound.AskType.CriticalHit => CriticalHit, 
			Kingmaker.Visual.Sound.AskType.TraumaApplied => TraumaApplied, 
			Kingmaker.Visual.Sound.AskType.Order => Order, 
			Kingmaker.Visual.Sound.AskType.Selected => Selected, 
			Kingmaker.Visual.Sound.AskType.CantDo => CantDo, 
			Kingmaker.Visual.Sound.AskType.CheckSuccess => CheckSuccess, 
			Kingmaker.Visual.Sound.AskType.CheckFail => CheckFail, 
			Kingmaker.Visual.Sound.AskType.Discovery => Discovery, 
			Kingmaker.Visual.Sound.AskType.OrderMove => OrderMove, 
			Kingmaker.Visual.Sound.AskType.OrderMoveExploration => OrderMoveExploration, 
			Kingmaker.Visual.Sound.AskType.EnemyDeath => EnemyDeath, 
			Kingmaker.Visual.Sound.AskType.PartyMemberUnconscious => PartyMemberUnconscious, 
			Kingmaker.Visual.Sound.AskType.PsychicPhenomena => PsychicPhenomena, 
			Kingmaker.Visual.Sound.AskType.PerilsOfTheWarp => PerilsOfTheWarp, 
			Kingmaker.Visual.Sound.AskType.HealingAlly => HealingAlly, 
			Kingmaker.Visual.Sound.AskType.BeingHealed => BeingHealed, 
			Kingmaker.Visual.Sound.AskType.EnemyMassDeath => EnemyMassDeath, 
			Kingmaker.Visual.Sound.AskType.FriendlyFire => FriendlyFire, 
			Kingmaker.Visual.Sound.AskType.MoraleBroken => MoraleBroken, 
			Kingmaker.Visual.Sound.AskType.MoraleHeroic => MoraleHeroic, 
			Kingmaker.Visual.Sound.AskType.ChannellingOn => ChannellingOn, 
			Kingmaker.Visual.Sound.AskType.ChannellingOff => ChannellingOff, 
			Kingmaker.Visual.Sound.AskType.ChannellingReaction => ChannellingReaction, 
			Kingmaker.Visual.Sound.AskType.ChannellingSuccessfulRelease => ChannellingSuccessfulRelease, 
			Kingmaker.Visual.Sound.AskType.WeAreLoosing => WeAreLoosing, 
			Kingmaker.Visual.Sound.AskType.WeAreWinning => WeAreWinning, 
			Kingmaker.Visual.Sound.AskType.WeAreLostByMorale => WeAreLostByMorale, 
			Kingmaker.Visual.Sound.AskType.ArmorBroken => ArmorBroken, 
			Kingmaker.Visual.Sound.AskType.DetectiveCanStudyClue => DetectiveCanStudyClue, 
			Kingmaker.Visual.Sound.AskType.DetectiveNewConclusionAvailable => DetectiveNewConclusionAvailable, 
			Kingmaker.Visual.Sound.AskType.DetectiveSearch => DetectiveSearch, 
			Kingmaker.Visual.Sound.AskType.DetectiveTracesFound => DetectiveTracesFound, 
			Kingmaker.Visual.Sound.AskType.DetectiveReminder => DetectiveReminder, 
			Kingmaker.Visual.Sound.AskType.DetectiveReconstructionFind => DetectiveReconstructionFind, 
			Kingmaker.Visual.Sound.AskType.DetectiveReconstructionReady => DetectiveReconstructionReady, 
			Kingmaker.Visual.Sound.AskType.DetectiveSignalFound => DetectiveSignalFound, 
			_ => null, 
		};
		return ask != null;
	}

	public void PlayPreview()
	{
		GameObject gameObject = UIDollRooms.Instance.gameObject;
		SoundEventsManager.PostEvent(PreviewSound, gameObject);
	}

	public IEnumerable<AskEntry> GetAllAsks()
	{
		foreach (AskType item in Enum.GetValues(typeof(AskType)).Cast<AskType>())
		{
			if (!TryGetAskByType(item, out var ask) || ask.Entries == null)
			{
				continue;
			}
			foreach (AskEntry entry in ask.Entries)
			{
				yield return entry;
			}
		}
		if (AnimationAsks != null)
		{
			foreach (AnimationAsk animationAsk in AnimationAsks)
			{
				foreach (AskEntry entry2 in animationAsk.Entries)
				{
					yield return entry2;
				}
			}
		}
		if (PartyMemberUnconsciousPersonalized == null)
		{
			yield break;
		}
		foreach (PersonalizedAsk item2 in PartyMemberUnconsciousPersonalized)
		{
			foreach (AskEntry entry3 in item2.Entries)
			{
				yield return entry3;
			}
		}
	}
}
