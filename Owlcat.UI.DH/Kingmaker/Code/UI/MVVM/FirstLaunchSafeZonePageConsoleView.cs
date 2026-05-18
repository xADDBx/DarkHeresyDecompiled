using Kingmaker.UI.Canvases;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchSafeZonePageConsoleView : FirstLaunchSettingsPageBaseView<FirstLaunchSafeZonePageVM>
{
	[SerializeField]
	private SettingsEntitySliderConsoleView m_OffsetSlider;

	[SerializeField]
	private RectTransform m_Frame;

	protected override void OnBind()
	{
		m_OffsetSlider.Bind(base.ViewModel.Offset);
		base.ViewModel.Offset.TempFloatValue.Subscribe(delegate(float value)
		{
			float num = value / 200f;
			Rect rect = MainCanvas.Instance.RectTransform.rect;
			m_Frame.offsetMin = new Vector2(rect.width * num, rect.height * num);
			m_Frame.offsetMax = new Vector2((0f - rect.width) * num, (0f - rect.height) * num);
		}).AddTo(this);
		base.OnBind();
	}
}
