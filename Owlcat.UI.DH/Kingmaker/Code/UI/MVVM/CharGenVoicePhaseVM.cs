using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Sound.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.Utility.GameConst;
using Kingmaker.Visual.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenVoicePhaseVM : CharGenPhaseBaseVM, ICharGenAppearancePhaseVoiceHandler, ISubscriber
{
	private readonly SelectionStateVoice m_SelectionState;

	private readonly BlueprintVoiceSelection m_Blueprint;

	private readonly ObservableList<CharGenVoiceItemVM> m_VoiceItems = new ObservableList<CharGenVoiceItemVM>();

	private readonly ReactiveProperty<CharGenVoiceItemVM> m_SelectedVoiceVM = new ReactiveProperty<CharGenVoiceItemVM>();

	private readonly List<CharGenVoicePhraseButtonVM> m_PhraseButtonDisposables = new List<CharGenVoicePhraseButtonVM>();

	private const float VoiceAttenuationScalingFactor = 1000f;

	private bool m_IsSelectedManually;

	private int m_LastPlayedAskId = -1;

	private CharGenVoiceItemVM m_LastSelectedVoice;

	private VoiceOverStatus m_CurrentPhraseStatus;

	public readonly ObservableList<CharGenVoicePhraseButtonVM> PhraseButtons = new ObservableList<CharGenVoicePhraseButtonVM>();

	private Gender DollGender => m_CharGenContext.Doll.Gender;

	public SelectionGroupRadioVM<CharGenVoiceItemVM> VoiceSelector { get; private set; }

	public ReadOnlyReactiveProperty<CharGenVoiceItemVM> SelectedVoiceVM => m_SelectedVoiceVM;

	public override MusicStateHandler.MusicChargenState ChargenMusicState => MusicStateHandler.MusicChargenState.None;

	public CharGenVoicePhaseVM(CharGenContext charGenContext, SelectionStateVoice selectionState, BlueprintVoiceSelection blueprint)
		: base(charGenContext, CharGenPhaseType.Voice, allowSwitchOff: true)
	{
		base.DisplayMode = CharGenDisplayMode.PortraitOnly;
		base.HasSmallPortrait = true;
		m_SelectionState = selectionState;
		m_Blueprint = blueprint;
		base.BlueprintSelectionWithUI = blueprint;
		m_PhaseName.Value = blueprint.Title;
		CreateVoiceItems();
		VoiceSelector = new SelectionGroupRadioVM<CharGenVoiceItemVM>(m_VoiceItems, m_SelectedVoiceVM).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		m_SelectedVoiceVM.Subscribe(OnVoiceSelected).AddTo(this);
		m_CharGenContext.CurrentUnit.Subscribe(delegate
		{
			UpdateFromMechanic();
		}).AddTo(this);
		SoundBanksManager.LoadBank(UIConsts.PCDemoVoicesENGBank);
		SoundBanksManager.LoadVoiceBanks();
		m_IsSelectedManually = m_SelectedVoiceVM.Value != null;
	}

	protected override bool CheckIsCompleted()
	{
		if (m_IsSelectedManually)
		{
			SelectionStateVoice selectionState = m_SelectionState;
			if (selectionState != null && selectionState.IsMade)
			{
				return selectionState.IsValid;
			}
			return false;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		AkUnitySoundEngine.SetScalingFactor(UIDollRooms.Instance.gameObject, 1000f);
		if (m_IsSelectedManually)
		{
			UpdateFromMechanic();
		}
	}

	protected override void OnEndDetailedView()
	{
		AkUnitySoundEngine.SetScalingFactor(UIDollRooms.Instance.gameObject, 1f);
	}

	protected override void DisposeImplementation()
	{
		VoiceOverStatus currentPhraseStatus = m_CurrentPhraseStatus;
		if (currentPhraseStatus != null && !currentPhraseStatus.IsEnded)
		{
			m_CurrentPhraseStatus.Stop();
		}
		m_VoiceItems.Clear();
		DisposePhraseButtons();
		PhraseButtons.Clear();
		SoundBanksManager.UnloadBank(UIConsts.PCDemoVoicesENGBank);
		SoundBanksManager.UnloadVoiceBanks();
		SoundState.Instance.OnMusicChargenPCVoiceChange(MusicStateHandler.MusicChargenPCVoice.None);
		base.DisposeImplementation();
	}

	void ICharGenAppearancePhaseVoiceHandler.HandleChangeVoice(BlueprintUnitAsksList blueprint)
	{
		CharGenVoiceItemVM charGenVoiceItemVM = m_VoiceItems.FirstOrDefault((CharGenVoiceItemVM v) => blueprint == v?.Asks);
		if (charGenVoiceItemVM == null)
		{
			PFLog.UI.Error("[CharGenVoicePhaseVM] BlueprintUnitAsksList not found! ID=" + blueprint.AssetGuid);
			return;
		}
		m_SelectedVoiceVM.Value = charGenVoiceItemVM;
		m_SelectionState.SelectVoice(charGenVoiceItemVM.Asks);
		charGenVoiceItemVM.Asks.PlayPreview();
		m_IsSelectedManually = true;
		UpdateIsCompleted();
	}

	private void CreateVoiceItems()
	{
		VOSettings instance = VOSettings.Instance;
		BlueprintCharGenRoot.VoiceEntry[] voiceEntries = ConfigRoot.Instance.CharGenRoot.VoiceEntries;
		foreach (BlueprintCharGenRoot.VoiceEntry voiceEntry in voiceEntries)
		{
			if (voiceEntry.VoId.Empty)
			{
				continue;
			}
			BlueprintUnitAsksList asksByVoGuid = instance.GetAsksByVoGuid(voiceEntry.VoId.Guid);
			if (asksByVoGuid != null)
			{
				CharGenVoiceItemVM item = new CharGenVoiceItemVM(asksByVoGuid, voiceEntry);
				item.OnClicked = delegate
				{
					OnVoiceClicked(item);
				};
				AddDisposable(item);
				m_VoiceItems.Add(item);
			}
		}
	}

	private void BuildPhraseButtons(CharGenVoiceItemVM voice)
	{
		DisposePhraseButtons();
		PhraseButtons.Clear();
		if (voice?.Asks != null)
		{
			LocalizedString[] chargenAsksStrings = voice.Asks.ChargenAsksStrings;
			for (int i = 0; i < chargenAsksStrings.Length; i++)
			{
				CharGenVoicePhraseButtonVM item = new CharGenVoicePhraseButtonVM(chargenAsksStrings[i], voice.VoGuid, PlayPhrase);
				m_PhraseButtonDisposables.Add(item);
				PhraseButtons.Add(item);
			}
		}
	}

	private void DisposePhraseButtons()
	{
		foreach (CharGenVoicePhraseButtonVM phraseButtonDisposable in m_PhraseButtonDisposables)
		{
			phraseButtonDisposable.Dispose();
		}
		m_PhraseButtonDisposables.Clear();
	}

	private void UpdateFromMechanic()
	{
		LevelUpManager currentValue = m_CharGenContext.LevelUpManager.CurrentValue;
		if (currentValue == null)
		{
			return;
		}
		BlueprintUnitAsksList pregenVoice = currentValue.PreviewUnit.Asks.List;
		if (!m_CharGenContext.IsCustomCharacter.CurrentValue && pregenVoice != null)
		{
			m_SelectedVoiceVM.Value = VoiceSelector.EntitiesCollection.FirstOrDefault((CharGenVoiceItemVM v) => v.Asks == pregenVoice);
		}
	}

	private void OnVoiceSelected(CharGenVoiceItemVM item)
	{
		BuildPhraseButtons(item);
		if (item?.Asks != null && item != m_LastSelectedVoice)
		{
			Game.Instance.GameCommandQueue.CharGenChangeVoice(item.Asks);
			SoundState.Instance.OnMusicChargenPCVoiceChange(item.MusicChargenVoice);
			m_LastSelectedVoice = item;
			m_LastPlayedAskId = 0;
			UpdateIsCompleted();
		}
	}

	private void OnVoiceClicked(CharGenVoiceItemVM item)
	{
		m_IsSelectedManually = true;
		if (m_SelectedVoiceVM.Value != item)
		{
			m_SelectedVoiceVM.Value = item;
		}
		else
		{
			int valueOrDefault = (item?.Asks?.ChargenAsksStrings?.Length).GetValueOrDefault();
			m_LastPlayedAskId = ((valueOrDefault > m_LastPlayedAskId + 1) ? (m_LastPlayedAskId + 1) : 0);
		}
		UpdateIsCompleted();
		PlayAskByIndex();
	}

	private void PlayAskByIndex()
	{
		CharGenVoiceItemVM currentValue = m_SelectedVoiceVM.CurrentValue;
		LocalizedString[] array = currentValue?.Asks?.ChargenAsksStrings;
		if (array != null && array.Length != 0 && m_LastPlayedAskId >= 0)
		{
			LocalizedString str = array[m_LastPlayedAskId];
			PlayPhrase(str, currentValue.VoGuid);
		}
	}

	private bool PlayPhrase(LocalizedString str, string voGuid)
	{
		if (str == null || string.IsNullOrEmpty(voGuid))
		{
			return false;
		}
		VoiceOverStatus currentPhraseStatus = m_CurrentPhraseStatus;
		if (currentPhraseStatus != null && !currentPhraseStatus.IsEnded)
		{
			m_CurrentPhraseStatus.Stop();
		}
		PhraseButtons.ForEach(delegate(CharGenVoicePhraseButtonVM b)
		{
			b.IsPlaying.Value = b.String?.Key == str.Key;
		});
		m_CurrentPhraseStatus = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(str, voGuid, VoiceOverType.Ask, UIDollRooms.Instance.gameObject);
		return m_CurrentPhraseStatus != null;
	}
}
