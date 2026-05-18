using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Test;

public class TooltipBrickFeatureAcronymTestView : BrickFeatureView
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	private Color m_DefaultTextColor;

	private Color m_DefaultBgrColor;

	protected override void OnBind()
	{
		Image component = m_Background.GetComponent<Image>();
		if (!(m_Label == null) && !(component == null))
		{
			m_DefaultTextColor = m_Label.color;
			m_DefaultBgrColor = component.color;
			base.OnBind();
			if (base.ViewModel.Feature != null && string.IsNullOrEmpty(base.ViewModel.Name))
			{
				m_IconBlock.SetActive(value: false);
				component.color = Color.red;
				m_Label.text = base.ViewModel.Feature.name;
				m_Label.color = Color.red;
			}
			m_Background.SetActive(value: true);
			ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
			{
				ShowInfo();
			}).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Image component = m_Background.GetComponent<Image>();
		if (!(m_Label == null) && !(component == null))
		{
			m_Label.color = m_DefaultTextColor;
			component.color = m_DefaultBgrColor;
		}
	}

	private void ShowInfo()
	{
	}
}
