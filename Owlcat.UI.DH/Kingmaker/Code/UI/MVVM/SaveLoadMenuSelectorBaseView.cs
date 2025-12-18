using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadMenuSelectorBaseView : View<SelectionGroupRadioVM<SaveLoadMenuEntityVM>>
{
	[SerializeField]
	private SaveLoadMenuEntityBaseView m_SaveButton;

	[SerializeField]
	private SaveLoadMenuEntityBaseView m_LoadButton;

	[SerializeField]
	private GameObject m_Selector;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_SaveButton.Initialize();
			m_LoadButton.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Initialize();
		m_SaveButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((SaveLoadMenuEntityVM e) => e.Mode == SaveLoadMode.Save));
		m_LoadButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((SaveLoadMenuEntityVM e) => e.Mode == SaveLoadMode.Load));
		base.ViewModel.SelectedEntity.Subscribe(delegate(SaveLoadMenuEntityVM selectedEntity)
		{
			SaveLoadMenuEntityBaseView selectedButton = ((selectedEntity.Mode == SaveLoadMode.Save) ? m_SaveButton : m_LoadButton);
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				UIUtilityLens.MoveXLensPosition(m_Selector.transform, selectedButton.transform.localPosition.x, 0.55f);
			});
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
	}
}
