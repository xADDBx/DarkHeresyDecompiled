using Kingmaker.Code.UI.MVVM.CombatNotifications.CombatObjectives;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.CombatNotifications;

public class CombatHUDNotificationView : View<CombatHUDNotificationsVM>
{
	[SerializeField]
	private ServoSkullBarkView m_ServoSkullBark;

	[SerializeField]
	private CombatObjectivesView m_CombatObjectivesView;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void OnBind()
	{
		m_ServoSkullBark.Bind(base.ViewModel.ServoSkullBarkVM);
		m_CombatObjectivesView.Bind(base.ViewModel.CombatObjectivesVM);
		base.ViewModel.IsHidden.Subscribe(delegate(bool isHidden)
		{
			m_FadeAnimator.PlayAnimation(!isHidden);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_ServoSkullBark.Unbind();
		m_CombatObjectivesView.Unbind();
	}
}
