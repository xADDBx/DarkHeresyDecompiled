using Code.UI.Common.Animations;
using Kingmaker.UI.Common.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.Code.UI.MVVM.SignalDevice;

public class DeviceAnimator : MonoBehaviour, IUIAnimator
{
	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private ScaleAnimator m_ScaleAnimator;

	public void Initialize()
	{
		m_MoveAnimator.Initialize();
		m_ScaleAnimator.Initialize();
	}

	public void AppearAnimation(UnityAction action = null)
	{
		m_MoveAnimator.AppearAnimation(action);
		m_ScaleAnimator.AppearAnimation(action);
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		m_MoveAnimator.DisappearAnimation(action);
		m_ScaleAnimator.DisappearAnimation(action);
	}
}
