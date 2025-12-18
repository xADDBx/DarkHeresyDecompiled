using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal.MainPage;

public class MainPageCardBaseView : View<CaseCardVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private GameObject m_NewItemsParent;

	[SerializeField]
	private TMP_Text m_NewItemsCount;

	[Header("Views")]
	[SerializeField]
	private CaseCardHeaderView m_Header;

	[SerializeField]
	private CaseCardIconView m_Icon;

	[SerializeField]
	private CaseCardDescriptionView m_Description;

	protected override void OnBind()
	{
		base.ViewModel.CurrentState.Subscribe(delegate(CardState value)
		{
			m_Button.SetActiveLayer(value.ToString());
		}).AddTo(this);
		base.ViewModel.NewItemsCount.Subscribe(delegate(int value)
		{
			m_NewItemsParent.SetActive(value > 0);
			m_NewItemsCount.text = "+" + value;
		}).AddTo(this);
		m_Button.OnLeftClickAsObservable().Subscribe(base.ViewModel.ClickOnCard).AddTo(this);
		m_Header.Bind(base.ViewModel);
		m_Icon.Bind(base.ViewModel);
		m_Description.Bind(base.ViewModel);
	}

	[ContextMenu("UpdateSelectables")]
	private void UpdateSelectables()
	{
		OwlcatMultiSelectable[] componentsInChildren = GetComponentsInChildren<OwlcatMultiSelectable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetActiveLayer(m_Button.MultiLayerNames[m_Button.ActiveLayerIndex]);
		}
	}
}
