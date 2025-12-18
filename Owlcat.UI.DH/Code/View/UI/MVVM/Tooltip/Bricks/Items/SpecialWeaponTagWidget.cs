using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Utils;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class SpecialWeaponTagWidget : View<(int, WeaponTagUISettings)>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Label;

	[SerializeField]
	private TMP_Text m_Value;

	[SerializeField]
	private OwlcatMultiSelectable m_TagSelectable;

	[SerializeField]
	private LayoutElement m_ValueLayoutElement;

	[Header("Values")]
	[SerializeField]
	private float m_DefaultValueWidth = 68.45f;

	protected override void OnBind()
	{
		m_Icon.sprite = UIConfig.Instance.FeatureTagsConfig.GetWeaponTagIcon(base.ViewModel.Item2);
		m_Label.text = UIStrings.Instance.Tooltips.GetWeaponTagLabel(base.ViewModel.Item2.Tag);
		m_Value.text = base.ViewModel.Item1.ToStringWithSign();
		string activeLayer = (TooltipBrickWeaponHeaderVM.SpecialTags.Contains(base.ViewModel.Item2.Tag) ? base.ViewModel.Item2.Tag.ToString() : "Default");
		m_TagSelectable.SetActiveLayer(activeLayer);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		UpdateValueWidth(m_DefaultValueWidth);
	}

	public void UpdateValueWidth(float size)
	{
		m_ValueLayoutElement.minWidth = size;
	}
}
