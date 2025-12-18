using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaNavigationElementBaseView : View<EncyclopediaNavigationElementVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public bool IsSelected => base.ViewModel.IsSelected.CurrentValue;

	protected override void OnBind()
	{
		UISounds.Instance.SetHoverSound(m_MultiButton, Game.Instance.IsControllerGamepad ? ButtonSoundsEnum.PaperComponentSound : ButtonSoundsEnum.NormalSound);
		m_Label.text = base.ViewModel.Title;
		m_Label.alignment = ((base.ViewModel.Title.Length > 1) ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center);
		base.ViewModel.IsAvailablePage.Subscribe(base.gameObject.SetActive).AddTo(this);
		base.ViewModel.IsSelected.Subscribe(OnSelected).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MultiButton.OnLeftClickAsObservable(), delegate
		{
			SelectPage();
		}).AddTo(this);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	private void OnSelected(bool value)
	{
		m_MultiButton.SetActiveLayer(value ? "On" : "Off");
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		return m_MultiButton.IsValid();
	}

	public void SelectPage()
	{
		base.ViewModel.SelectPage();
	}

	public bool HaveChilds()
	{
		if (base.ViewModel.ChildsVM.Count > 0)
		{
			return base.ViewModel.ChildsVM != null;
		}
		return false;
	}
}
