using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SpecialWeaponTagWidget : View<SpecialWeaponTagVM>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextValueTupleView m_Value;

	[SerializeField]
	private OwlcatMultiSelectable m_TagSelectable;

	[SerializeField]
	private LayoutElement m_ValueLayoutElement;

	[Header("Values")]
	[SerializeField]
	private float m_DefaultValueWidth = 68.45f;

	protected override void OnBind()
	{
		m_Icon.sprite = UIConfig.Instance.FeatureTagsConfig.GetSpecialWeaponTagIcon(base.ViewModel.Type);
		m_Value.Bind(base.ViewModel.Value);
		m_TagSelectable.SetActiveLayer(base.ViewModel.Type.ToString());
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
