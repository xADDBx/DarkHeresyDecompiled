using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerItemView : SelectionGroupEntityView<CharGenCareerSelectionItemVM>
{
	protected const string ButtonLayerNormal = "Normal";

	protected const string ButtonLayerChosen = "Chosen";

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_Label != null)
		{
			base.ViewModel.Label.Subscribe(delegate(string l)
			{
				m_Label.text = l;
			}).AddTo(this);
		}
		if (m_Icon != null)
		{
			base.ViewModel.Sprite.Subscribe(delegate(Sprite s)
			{
				m_Icon.sprite = s;
			}).AddTo(this);
		}
		m_Button.OnHoverAsObservable().Subscribe(base.ViewModel.OnHover).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnRightClickAsObservable(), delegate
		{
			TooltipHelper.ShowInfo(base.ViewModel.Template);
		}).AddTo(this);
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		m_Button.SetActiveLayer(value ? "Chosen" : "Normal");
	}
}
