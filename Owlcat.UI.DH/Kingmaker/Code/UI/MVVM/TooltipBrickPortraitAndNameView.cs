using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickPortraitAndNameView : TooltipBaseBrickView<TooltipBrickPortraitAndNameVM>
{
	[SerializeField]
	protected OwlcatMultiSelectable m_MultiSelectableIcon;

	[SerializeField]
	protected OwlcatMultiSelectable m_MultiSelectableBorderAndDifficulty;

	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	protected TooltipBrickTitleView m_TitleView;

	[SerializeField]
	protected TextMeshProUGUI m_DifficultyText;

	[SerializeField]
	private float m_DefaultFontSize = 24f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 24f;

	protected override void OnBind()
	{
		base.OnBind();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Line;
		TooltipBrickTitleVM source = (base.ViewModel.BrickTitle?.GetVM() as TooltipBrickTitleVM).AddTo(this);
		m_TitleView.Bind(source);
		m_TitleView.gameObject.SetActive(base.ViewModel.BrickTitle != null);
		string text = (base.ViewModel.IsEnemy ? "Enemy" : (base.ViewModel.IsFriend ? "Friend" : "Default"));
		m_MultiSelectableBorderAndDifficulty.SetActiveLayer(text);
		m_MultiSelectableIcon.SetActiveLayer(base.ViewModel.IsUsedSubtypeIcon ? text : "Default");
		m_DifficultyText.gameObject.SetActive(base.ViewModel.Difficulty > 0);
		m_DifficultyText.text = UIUtilityText.ArabicToRoman(base.ViewModel.Difficulty);
		m_Title.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
	}
}
