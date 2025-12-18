using Kingmaker;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

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
		base.ViewModel.GetNameAndDescription(out var header, out var description);
		description = UIUtilityText.UpdateDescriptionWithUIProperties(description, Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.CurrentValue);
		m_Background.SetTooltip(new TooltipTemplateSimple(header, description)).AddTo(this);
	}
}
