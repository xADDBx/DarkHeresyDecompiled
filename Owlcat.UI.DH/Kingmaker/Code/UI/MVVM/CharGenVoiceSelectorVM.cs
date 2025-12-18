using System;
using System.Linq;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.Utility.GameConst;
using Kingmaker.Visual.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenVoiceSelectorVM : BaseCharGenAppearancePageComponentVM, ICharGenAppearancePhaseVoiceHandler, ISubscriber
{
	private readonly ReactiveProperty<BlueprintUnitAsksList> m_Barks = new ReactiveProperty<BlueprintUnitAsksList>();

	private readonly CharGenContext m_CharGenContext;

	private bool m_IsSelectedManually;

	private SelectionStateVoice m_SelectionStateVoice;

	private readonly ObservableList<CharGenVoiceItemVM> m_VoicesList = new ObservableList<CharGenVoiceItemVM>();

	private readonly ReactiveProperty<CharGenVoiceItemVM> m_SelectedVoiceVM = new ReactiveProperty<CharGenVoiceItemVM>();

	public SelectionGroupRadioVM<CharGenVoiceItemVM> VoiceSelector;

	public ReadOnlyReactiveProperty<BlueprintUnitAsksList> Barks => m_Barks;

	public ReadOnlyReactiveProperty<CharGenVoiceItemVM> SelectedVoiceVM => m_SelectedVoiceVM;

	private Gender DollGender => m_CharGenContext.Doll.Gender;

	public CharGenVoiceSelectorVM(CharGenContext ctx)
	{
		m_CharGenContext = ctx;
		AddDisposable(EventBus.Subscribe(this));
		CreateVoices();
		AddDisposable(m_CharGenContext.CurrentUnit.Subscribe(delegate
		{
			UpdateFromMechanic();
		}));
		AddDisposable(m_CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		AddDisposable(SelectedVoiceVM.Subscribe(delegate(CharGenVoiceItemVM value)
		{
			OnChooseVoice(value);
			m_IsSelectedManually = true;
		}));
		SoundBanksManager.LoadBank(UIConsts.PCDemoVoicesENGBank);
		m_IsSelectedManually = false;
	}

	void ICharGenAppearancePhaseVoiceHandler.HandleChangeVoice(BlueprintUnitAsksList blueprint)
	{
		CharGenVoiceItemVM charGenVoiceItemVM = m_VoicesList.FirstOrDefault((CharGenVoiceItemVM elem) => blueprint == elem?.Voice);
		if (charGenVoiceItemVM == null)
		{
			PFLog.UI.Error("BlueprintUnitAsksList not found! ID=" + blueprint.AssetGuid);
			return;
		}
		m_SelectedVoiceVM.Value = charGenVoiceItemVM;
		m_SelectionStateVoice?.SelectVoice(charGenVoiceItemVM.Voice);
		m_Barks.Value = charGenVoiceItemVM.Voice;
		Barks.CurrentValue.PlayPreview();
		Changed();
	}

	protected override void DisposeImplementation()
	{
		m_VoicesList.Clear();
		SoundBanksManager.UnloadBank(UIConsts.PCDemoVoicesENGBank);
	}

	private void CreateVoices()
	{
		m_VoicesList.Clear();
		foreach (BlueprintUnitAsksList voice in ConfigRoot.Instance.CharGenRoot.Voices)
		{
			CharGenVoiceItemVM charGenVoiceItemVM = new CharGenVoiceItemVM(voice);
			AddDisposable(charGenVoiceItemVM);
			if (!charGenVoiceItemVM.IsEmptyVoice)
			{
				m_VoicesList.Add(charGenVoiceItemVM);
			}
		}
		VoiceSelector = AddDisposableAndReturn(new SelectionGroupRadioVM<CharGenVoiceItemVM>(m_VoicesList, m_SelectedVoiceVM));
	}

	private void UpdateSelector()
	{
		m_IsAvailable.Value = m_VoicesList.Any();
	}

	public override void OnBeginView()
	{
		UpdateSelector();
		if (m_CharGenContext.Doll.TrackPortrait && !m_IsSelectedManually)
		{
			UpdateFromMechanic();
		}
		if (SelectedVoiceVM.CurrentValue != null)
		{
			OnChooseVoice(SelectedVoiceVM.CurrentValue);
		}
		else
		{
			UpdateFromMechanic();
		}
	}

	private void UpdateFromMechanic()
	{
		LevelUpManager currentValue = m_CharGenContext.LevelUpManager.CurrentValue;
		BlueprintUnitAsksList pregenVoice = currentValue.PreviewUnit.Asks.List;
		if (!m_CharGenContext.IsCustomCharacter.CurrentValue && pregenVoice != null)
		{
			m_SelectedVoiceVM.Value = VoiceSelector.EntitiesCollection.FirstOrDefault((CharGenVoiceItemVM item) => item.Voice == pregenVoice);
			return;
		}
		BlueprintCharGenRoot charGenRoot = ConfigRoot.Instance.CharGenRoot;
		int value = ((DollGender == Gender.Male) ? charGenRoot.MaleVoiceDefaultId : charGenRoot.FemaleVoiceDefaultId);
		value = Math.Clamp(value, 0, VoiceSelector.EntitiesCollection.Count - 1);
		m_SelectedVoiceVM.Value = VoiceSelector.EntitiesCollection.ElementAt(value);
		m_IsSelectedManually = false;
	}

	private void OnChooseVoice(CharGenVoiceItemVM voice)
	{
		if (voice?.Voice != null)
		{
			Game.Instance.GameCommandQueue.CharGenChangeVoice(voice.Voice);
		}
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			BlueprintVoiceSelection selectionByType = UtilityChargen.GetSelectionByType<BlueprintVoiceSelection>(manager.Path);
			if (selectionByType != null)
			{
				m_SelectionStateVoice = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStateVoice;
			}
		}
	}
}
