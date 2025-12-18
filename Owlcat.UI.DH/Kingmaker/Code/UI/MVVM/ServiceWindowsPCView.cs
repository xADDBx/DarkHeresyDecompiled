using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ServiceWindowsPCView : View<ServiceWindowsVM>
{
	[SerializeField]
	private ServiceWindowMenuPCView m_ServiceWindowMenuPcView;

	[SerializeField]
	private FadeAnimator m_Background;

	private bool m_IsInit;

	private readonly List<ServiceWindowsType> m_WindowsWithoutBgr = new List<ServiceWindowsType> { ServiceWindowsType.Inventory };

	public void Awake()
	{
		if (!m_IsInit)
		{
			m_ServiceWindowMenuPcView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		base.ViewModel.ServiceWindowsMenuVM.Subscribe(m_ServiceWindowMenuPcView.Bind).AddTo(this);
		base.ViewModel.ServiceWindowsMenuVM.Subscribe(delegate(ServiceWindowsMenuVM vm)
		{
			if (vm != null && !base.ViewModel.ForceHideBackground.CurrentValue)
			{
				m_Background.Or(null)?.AppearAnimation();
			}
			else
			{
				m_Background.Or(null)?.DisappearAnimation();
			}
		}).AddTo(this);
		base.ViewModel.OnOpen.Subscribe(delegate(ServiceWindowsType vm)
		{
			if (!m_WindowsWithoutBgr.Contains(vm))
			{
				m_Background.Or(null)?.AppearAnimation();
			}
			else
			{
				m_Background.Or(null)?.DisappearAnimation();
			}
		}).AddTo(this);
	}
}
