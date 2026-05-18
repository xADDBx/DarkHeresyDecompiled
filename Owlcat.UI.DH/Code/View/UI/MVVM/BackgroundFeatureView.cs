using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM;

public class BackgroundFeatureView : View<BackgroundFeatureVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_NameText;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_TypeText;

	[SerializeField]
	private EnumToObjectSelector<FeatureGroup, Color> m_GroupOverrideColor;

	protected override void OnBind()
	{
		m_NameText.text = base.ViewModel.Name;
		m_TypeText.text = base.ViewModel.FeatureTypeName;
		m_Icon.sprite = base.ViewModel.Icon ?? UIConfig.Instance.TransparentImage;
		m_Icon.color = m_GroupOverrideColor.GetEntity(base.ViewModel.FeatureGroup);
	}

	public void SetupTooltip(TooltipConfig config)
	{
		this.SetTooltip(base.ViewModel.Tooltip, config).AddTo(this);
	}
}
