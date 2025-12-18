using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGameMenuSelectorBaseView : View<SelectionGroupRadioVM<NewGameMenuEntityVM>>
{
	[SerializeField]
	private NewGameMenuEntityBaseView m_PresetButton;

	[SerializeField]
	private NewGameMenuEntityBaseView m_DifficultyButton;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_DifficultyButton.Initialize();
			m_PresetButton.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_PresetButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((NewGameMenuEntityVM e) => e.NewGamePhaseVM is NewGamePhasePresetVM));
		m_DifficultyButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((NewGameMenuEntityVM e) => e.NewGamePhaseVM is NewGamePhaseDifficultyVM));
		base.ViewModel.SelectedEntity.Skip(1).Subscribe(delegate(NewGameMenuEntityVM selectedEntity)
		{
			if (!(selectedEntity.NewGamePhaseVM is NewGamePhasePresetVM))
			{
				_ = m_DifficultyButton;
			}
			else
			{
				_ = m_PresetButton;
			}
		}).AddTo(this);
		ResetLensPosition();
	}

	protected override void OnUnbind()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
	}

	private void ResetLensPosition()
	{
	}
}
