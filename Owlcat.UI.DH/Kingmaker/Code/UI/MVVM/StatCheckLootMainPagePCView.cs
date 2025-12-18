using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootMainPagePCView : StatCheckLootMainPageBaseView<StatCheckLootUnitCardPCView>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	protected override void OnBind()
	{
		base.OnBind();
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.OnClose).AddTo(this);
		m_CloseButton.OnConfirmClickAsObservable().Subscribe(base.OnClose).AddTo(this);
	}
}
