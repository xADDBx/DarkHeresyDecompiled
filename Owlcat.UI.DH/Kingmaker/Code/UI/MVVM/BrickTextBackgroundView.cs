using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTextBackgroundView : BrickBaseView<BrickTextBackgroundVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextEntityWidget m_Text;

	[SerializeField]
	private LayoutGroup m_LayoutGroup;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_BackgroundStateSelectable;

	private TextAnchor m_DefaultTextAnchor;

	private void Awake()
	{
		m_DefaultTextAnchor = m_LayoutGroup.childAlignment;
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_Text.Bind(base.ViewModel.Text)?.AddTo(this);
		m_BackgroundStateSelectable.SetActiveLayer(base.ViewModel.Palette.ToString());
		m_LayoutGroup.childAlignment = base.ViewModel.TextAnchor;
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_LayoutGroup.childAlignment = m_DefaultTextAnchor;
	}
}
