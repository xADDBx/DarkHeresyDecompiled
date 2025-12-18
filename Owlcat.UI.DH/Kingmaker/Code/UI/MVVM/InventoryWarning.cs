using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryWarning : View<string>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Label;

	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Values")]
	[SerializeField]
	private float m_TimeOnScreen = 1f;

	public float DisappearTime => m_Animator.DisappearTime;

	protected override void OnBind()
	{
		m_Label.text = base.ViewModel;
		m_Animator.AppearAnimation();
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(m_TimeOnScreen.Seconds(), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			WidgetFactory.DisposeWidget(this);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Animator.DisappearAnimation();
	}
}
