using Owlcat.Plugins.DotNetExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpFeatureView : TooltipBaseBrickView<TooltipBrickLevelUpFeatureVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Subtitle;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private RectTransform m_IconRect;

	[SerializeField]
	private GameObject m_IconFrame;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	protected override void OnBind()
	{
		base.OnBind();
		m_Title.gameObject.SetActive(!base.ViewModel.Data.Title.IsNullOrEmpty());
		m_Subtitle.gameObject.SetActive(!base.ViewModel.Data.Subtitle.IsNullOrEmpty());
		m_Value.gameObject.SetActive(!base.ViewModel.Data.Value.IsNullOrEmpty());
		m_Acronym.transform.parent.gameObject.SetActive(!base.ViewModel.Data.Acronym.IsNullOrEmpty());
		m_IconRect.gameObject.SetActive(base.ViewModel.Data.Icon != null);
		m_IconFrame.SetActive(base.ViewModel.Data.IconWithFrame);
		m_TalentGroupView.gameObject.SetActive(base.ViewModel.Data.TalentIconInfo != null);
		if (!base.ViewModel.Data.Title.IsNullOrEmpty())
		{
			m_Title.text = base.ViewModel.Data.Title;
		}
		if (!base.ViewModel.Data.Subtitle.IsNullOrEmpty())
		{
			m_Subtitle.text = base.ViewModel.Data.Subtitle;
		}
		if (!base.ViewModel.Data.Value.IsNullOrEmpty())
		{
			m_Value.text = base.ViewModel.Data.Value;
		}
		if (!base.ViewModel.Data.Acronym.IsNullOrEmpty())
		{
			m_Acronym.text = base.ViewModel.Data.Acronym;
		}
		if (base.ViewModel.Data.Icon != null)
		{
			m_Icon.sprite = base.ViewModel.Data.Icon;
		}
		if (base.ViewModel.Data.IconSize != default(Vector2))
		{
			m_IconRect.sizeDelta = base.ViewModel.Data.IconSize;
		}
		if (base.ViewModel.Data.TalentIconInfo != null)
		{
			m_TalentGroupView.SetupView(base.ViewModel.Data.TalentIconInfo);
		}
		if (base.ViewModel.Data.Tooltip != null)
		{
			this.SetTooltip(base.ViewModel.Data.Tooltip);
		}
	}
}
