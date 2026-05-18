using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconValueStatView : BrickBaseView<BrickIconValueStatVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextValueTupleView m_Data;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private GameObject m_IconContainer;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_IconSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_TextSelectable;

	protected override void OnBind()
	{
		m_Data.Bind(base.ViewModel.Data);
		m_TextSelectable.SetActiveLayer(base.ViewModel.TextColor.ToString());
		m_Icon.sprite = base.ViewModel.Icon;
		m_IconSelectable.SetActiveLayer(base.ViewModel.IconColor.ToString());
		m_IconContainer.SetActive(base.ViewModel.Icon != null);
	}
}
