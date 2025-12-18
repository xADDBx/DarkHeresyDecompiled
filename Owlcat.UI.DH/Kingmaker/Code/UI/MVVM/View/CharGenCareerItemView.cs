using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerItemView : SelectionGroupEntityView<CharGenCareerSelectionItemVM>
{
	protected const string BUTTON_LAYER_NORMAL = "Normal";

	protected const string BUTTON_LAYER_CHOSEN = "Chosen";

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Image m_Icon;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_Label != null)
		{
			AddDisposable(base.ViewModel.Label.Subscribe(delegate(string l)
			{
				m_Label.text = l;
			}));
		}
		if (m_Icon != null)
		{
			AddDisposable(base.ViewModel.Sprite.Subscribe(delegate(Sprite s)
			{
				m_Icon.sprite = s;
			}));
		}
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		m_Button.SetActiveLayer(value ? "Chosen" : "Normal");
	}
}
