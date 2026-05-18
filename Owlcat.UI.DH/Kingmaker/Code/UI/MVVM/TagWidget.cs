using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TagWidget : View<TagData>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Image m_Frame;

	protected override void OnBind()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		Color bgrColor = base.ViewModel.BgrColor;
		m_Background.color = bgrColor;
		bgrColor.a = 1f;
		m_Frame.color = bgrColor;
		string descriptionWithItemEquipped = UIUtilityItem.GetDescriptionWithItemEquipped(base.ViewModel.BlueprintItem, () => base.ViewModel.GetName());
		string descriptionWithItemEquipped2 = UIUtilityItem.GetDescriptionWithItemEquipped(base.ViewModel.BlueprintItem, () => base.ViewModel.GetDescription());
		m_Background.SetTooltip(new TooltipTemplateSimple(descriptionWithItemEquipped, descriptionWithItemEquipped2)).AddTo(this);
	}
}
