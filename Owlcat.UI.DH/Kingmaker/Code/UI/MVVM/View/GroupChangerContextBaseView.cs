using Kingmaker.ResourceLinks;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class GroupChangerContextBaseView : View<GroupChangerContext>
{
	[SerializeField]
	private UIViewLinkTemp<GroupChangerBaseView, GroupChangerVM> m_GroupChangerPCView;

	private bool m_IsInit;

	public void Awake()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		base.ViewModel.GroupChangerVm.Subscribe(m_GroupChangerPCView.Bind).AddTo(this);
	}
}
