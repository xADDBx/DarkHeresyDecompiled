using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhasePresetVM : NewGamePhaseBaseVm
{
	public readonly SelectionGroupRadioVM<NewGamePhasePresetEntityVM> SelectionGroup;

	private readonly ReactiveProperty<NewGamePhasePresetEntityVM> m_SelectedEntity = new ReactiveProperty<NewGamePhasePresetEntityVM>();

	public NewGamePhasePresetVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		List<NewGamePhasePresetEntityVM> list = new List<NewGamePhasePresetEntityVM>();
		foreach (NewGamePreset item in ConfigRoot.Instance.NewGameSettings.NewGamePresetToChoose)
		{
			NewGamePhasePresetEntityVM newGamePhasePresetEntityVM = new NewGamePhasePresetEntityVM(item);
			list.Add(newGamePhasePresetEntityVM);
			newGamePhasePresetEntityVM.AddTo(this);
		}
		SelectionGroup = new SelectionGroupRadioVM<NewGamePhasePresetEntityVM>(list, m_SelectedEntity);
		SelectionGroup.AddTo(this);
		m_SelectedEntity.Value = list.First();
		m_SelectedEntity.Subscribe(UpdatePreset);
		SetNextButtonAvailable(available: true);
	}

	private void UpdatePreset(NewGamePhasePresetEntityVM entityVM)
	{
		Game.NewGamePreset = entityVM.Preset.GamePreset;
	}
}
