using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentMixView : CharInfoComponentView<CharInfoAlignmentMixVM>
{
	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		base.OnBind();
		m_StateSelectable.SetActiveLayer(base.ViewModel.Status.ToString());
		m_StateSelectable.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}
}
