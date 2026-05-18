using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAppearanceAnchorButtonView : View<CharGenAppearanceAnchorEntry>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Image m_Icon;

	public Observable<Unit> Clicked => m_Button.OnLeftClickAsObservable();

	protected override void OnBind()
	{
		base.OnBind();
		if (m_Label != null)
		{
			m_Label.text = UIStrings.Instance.CharGen.GetPageLabelByType(base.ViewModel.PageType);
		}
		if (m_Icon != null)
		{
			Sprite sprite = ((base.ViewModel.Icon != null) ? base.ViewModel.Icon : UIConfig.Instance.UIIcons.GetAppearanceIcon(base.ViewModel.PageType));
			m_Icon.sprite = sprite.GetDefaultIfNull(DefaultImageType.Appearance);
		}
		m_Button.SetActiveLayer("Normal");
		m_Button.CanConfirm = true;
		if (base.ViewModel.HintString != null)
		{
			m_Button.SetHint(base.ViewModel.HintString).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		if (m_Button != null)
		{
			m_Button.SetActiveLayer("Normal");
		}
		base.OnUnbind();
	}

	public void SetSelected(bool value)
	{
		if (!(m_Button == null))
		{
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
			m_Button.CanConfirm = !value;
		}
	}

	public void SetFocus(bool value)
	{
		if (m_Button != null)
		{
			m_Button.SetFocus(value);
		}
	}
}
