using R3;
using R3.Triggers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuPCView : ContextMenuView
{
	private bool m_Hover;

	protected override void OnBind()
	{
		base.OnBind();
		Observable.EveryUpdate().Subscribe(OnUpdate).AddTo(this);
		this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			m_Hover = true;
		}).AddTo(this);
		this.OnPointerExitAsObservable().Subscribe(delegate
		{
			m_Hover = false;
		}).AddTo(this);
	}

	private void OnUpdate()
	{
		if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !m_Hover)
		{
			ContextMenuHelper.HideContextMenu();
		}
	}
}
