using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabDlcsPCView : DlcManagerTabDlcsBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private DlcManagerTabDlcsDlcSelectorPCView m_DlcSelectorPCView;

	[SerializeField]
	private CustomUIVideoPlayerPCView m_CustomUIVideoPlayerPCView;

	public override void Initialize()
	{
		base.Initialize();
		if (!IsInit)
		{
			m_CustomUIVideoPlayerPCView.Initialize();
			IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_CustomUIVideoPlayerPCView.Bind(base.ViewModel.CustomUIVideoPlayerVM);
		base.OnBind();
		m_DlcSelectorPCView.Bind(base.ViewModel.SelectionGroup);
		ObservableSubscribeExtensions.Subscribe(m_PurchaseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowInStore();
		}).AddTo(this);
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerPCView.gameObject.SetActive(state);
	}

	protected override void UpdateDlcEntitiesImpl()
	{
		base.UpdateDlcEntitiesImpl();
		m_DlcSelectorPCView.UpdateDlcEntities();
		base.ViewModel.SetSelectedEntityVM(base.ViewModel.SelectionGroup.EntitiesCollection.FirstOrDefault());
		base.ViewModel.SelectedEntity.CurrentValue.IsSelected.Value = true;
		base.ViewModel.SelectedEntity.CurrentValue.IsSelected.ForceNotify();
	}
}
