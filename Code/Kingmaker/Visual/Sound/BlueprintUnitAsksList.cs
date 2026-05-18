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
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Sound;

[TypeId("74167fee930157a49b88d49d1689ee9a")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintUnitAsksList : BlueprintScriptableObject
{
	public LocalizedString DisplayName;

	public AskComponentType AskType;

	[AkBankReference]
	public string[] SoundBanks = new string[0];

	public LocalizedString[] ChargenAsksStrings = new LocalizedString[0];

	[AkEventReference]
	public string PreviewSound = "";

	public AsksSet AggroBattleCry = new AsksSet();

	public AsksSet Pain = new AsksSet();

	public AsksSet Death = new AsksSet();

	public AsksSet Unconscious = new AsksSet();

	public AsksSet CriticalHit = new AsksSet();

	public AsksSet TraumaApplied = new AsksSet();

	public AsksSet Order = new AsksSet();

	public AsksSet Select = new AsksSet();

	public AsksSet CantDo = new AsksSet();

	public AsksSet CheckSuccessful = new AsksSet();

	public AsksSet CheckFailed = new AsksSet();

	public AsksSet Discovery = new AsksSet();

	public AsksSet MoveInCombat = new AsksSet();

	public AsksSet MoveInExploration = new AsksSet();

	public AsksSet EnemyDeath = new AsksSet();

	public AsksSet PartyMemberUnconsciousGeneral = new AsksSet();

	public AsksSet PsychicPhenomena = new AsksSet();

	public AsksSet PerilsOfTheWarp = new AsksSet();

	public AsksSet SupportAnAlly = new AsksSet();

	public AsksSet BeingSupported = new AsksSet();

	public AsksSet EnemyMassDeath = new AsksSet();

	public AsksSet FriendlyFire = new AsksSet();

	[Tooltip("Реакция юнита на переход в состояние морали Broken ")]
	public AsksSet BrokenMorale = new AsksSet();

	[Tooltip("Реакция юнита на переход в состояние морали Heroic ")]
	public AsksSet HeroicMorale = new AsksSet();

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
	public AsksSet BrokenArmour = new AsksSet();

	[Tooltip("Реакция члена пати на то, что для него стала доступна Study в улике")]
	public AsksSet ClueCanBeProcessed = new AsksSet();

	[Tooltip("Реакция сервочерепа на то, что стал доступен новый Conclusion")]
	public AsksSet ConclusionAvailable = new AsksSet();

	[Tooltip("Реакция члена пати на приближение к ProximityAskComponent с параметром DetectiveSearch")]
	public AsksSet DetectiveSearch = new AsksSet();

	[Tooltip("Реакция члена пати первый интеракт со цепочкой следов")]
	public AsksSet TracesFound = new AsksSet();

	[Tooltip("Реакция сервочерепа при заходе на локацию (загрузке), если на зоне есть незавершённая цепочка следов")]
	public AsksSet DetectiveReminder = new AsksSet();

	[Tooltip("Реакция члена пати на приближение к ProximityAskComponent с параметром DetectiveReconstructionFind")]
	public AsksSet DetectiveReconstructionFound = new AsksSet();

	[Tooltip("Реакция сервочерепа на срабатывание условия в DetectiveAsksSettings")]
	public AsksSet DetectiveReconstructionReady = new AsksSet();

	[Tooltip("Реакция сервочерепа на включение сигнала, если он ближайший к игроку")]
	public AsksSet SignalFound = new AsksSet();

	[Tooltip("Реакция псайкера или демона на вход в cohesion к Парии")]
	public AsksSet WithinPariahAura = new AsksSet();

	[Tooltip("Реакция псайкера или демона на получение урона в конце хода Парии")]
	public AsksSet PainPariahKeystone = new AsksSet();

	public PersonalizedAsksContainer PartyMemberUnconsciousPersonalized = new PersonalizedAsksContainer();

	[FormerlySerializedAs("AnimationAsks")]
	public AnimationAsksContainer MappedSound = new AnimationAsksContainer();

	public bool TryGetAskByType(AskType type, out AsksSet ask)
	{
		ask = type switch
		{
			Kingmaker.Visual.Sound.AskType.AggroBattleCry => AggroBattleCry, 
			Kingmaker.Visual.Sound.AskType.Pain => Pain, 
			Kingmaker.Visual.Sound.AskType.Death => Death, 
			Kingmaker.Visual.Sound.AskType.Unconscious => Unconscious, 
			Kingmaker.Visual.Sound.AskType.CriticalHit => CriticalHit, 
			Kingmaker.Visual.Sound.AskType.TraumaApplied => TraumaApplied, 
			Kingmaker.Visual.Sound.AskType.Order => Order, 
			Kingmaker.Visual.Sound.AskType.Select => Select, 
			Kingmaker.Visual.Sound.AskType.CantDo => CantDo, 
			Kingmaker.Visual.Sound.AskType.CheckSuccessful => CheckSuccessful, 
			Kingmaker.Visual.Sound.AskType.CheckFailed => CheckFailed, 
			Kingmaker.Visual.Sound.AskType.Discovery => Discovery, 
			Kingmaker.Visual.Sound.AskType.MoveInCombat => MoveInCombat, 
			Kingmaker.Visual.Sound.AskType.MoveInExploration => MoveInExploration, 
			Kingmaker.Visual.Sound.AskType.EnemyDeath => EnemyDeath, 
			Kingmaker.Visual.Sound.AskType.PartyMemberUnconsciousGeneral => PartyMemberUnconsciousGeneral, 
			Kingmaker.Visual.Sound.AskType.PsychicPhenomena => PsychicPhenomena, 
			Kingmaker.Visual.Sound.AskType.SupportAnAlly => SupportAnAlly, 
			Kingmaker.Visual.Sound.AskType.BeingSupported => BeingSupported, 
			Kingmaker.Visual.Sound.AskType.EnemyMassDeath => EnemyMassDeath, 
			Kingmaker.Visual.Sound.AskType.FriendlyFire => FriendlyFire, 
			Kingmaker.Visual.Sound.AskType.BrokenMorale => BrokenMorale, 
			Kingmaker.Visual.Sound.AskType.HeroicMorale => HeroicMorale, 
			Kingmaker.Visual.Sound.AskType.ChannellingOn => ChannellingOn, 
			Kingmaker.Visual.Sound.AskType.ChannellingOff => ChannellingOff, 
			Kingmaker.Visual.Sound.AskType.ChannellingReaction => ChannellingReaction, 
			Kingmaker.Visual.Sound.AskType.ChannellingSuccessfulRelease => ChannellingSuccessfulRelease, 
			Kingmaker.Visual.Sound.AskType.WeAreLoosing => WeAreLoosing, 
			Kingmaker.Visual.Sound.AskType.WeAreWinning => WeAreWinning, 
			Kingmaker.Visual.Sound.AskType.WeAreLostByMorale => WeAreLostByMorale, 
			Kingmaker.Visual.Sound.AskType.BrokenArmour => BrokenArmour, 
			Kingmaker.Visual.Sound.AskType.ClueCanBeProcessed => ClueCanBeProcessed, 
			Kingmaker.Visual.Sound.AskType.ConclusionAvailable => ConclusionAvailable, 
			Kingmaker.Visual.Sound.AskType.DetectiveSearch => DetectiveSearch, 
			Kingmaker.Visual.Sound.AskType.TracesFound => TracesFound, 
			Kingmaker.Visual.Sound.AskType.DetectiveReminder => DetectiveReminder, 
			Kingmaker.Visual.Sound.AskType.DetectiveReconstructionFound => DetectiveReconstructionFound, 
			Kingmaker.Visual.Sound.AskType.DetectiveReconstructionReady => DetectiveReconstructionReady, 
			Kingmaker.Visual.Sound.AskType.SignalFound => SignalFound, 
			Kingmaker.Visual.Sound.AskType.WithinPariahAura => WithinPariahAura, 
			Kingmaker.Visual.Sound.AskType.PainPariahKeystone => PainPariahKeystone, 
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
				if (entry.IsExist)
				{
					yield return entry;
				}
			}
		}
		if (MappedSound != null)
		{
			foreach (AnimationAsk item2 in MappedSound)
			{
				foreach (AskEntry entry2 in item2.Entries)
				{
					if (entry2.IsExist)
					{
						yield return entry2;
					}
				}
			}
		}
		if (PartyMemberUnconsciousPersonalized == null)
		{
			yield break;
		}
		foreach (PersonalizedAsk item3 in PartyMemberUnconsciousPersonalized)
		{
			foreach (AskEntry entry3 in item3.Entries)
			{
				if (entry3.IsExist)
				{
					yield return entry3;
				}
			}
		}
	}
}
