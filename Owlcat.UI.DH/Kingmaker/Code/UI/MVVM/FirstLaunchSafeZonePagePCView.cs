using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchSafeZonePagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchSafeZonePageVM>
{
	[SerializeField]
	private SettingsEntitySliderPCView m_OffsetSlider;

	[SerializeField]
	private RectTransform m_Frame;

	private int ScreenWidth => Screen.width;

	private int ScreenHeight => Screen.height;

	protected override void OnBind()
	{
		m_OffsetSlider.Bind(base.ViewModel.Offset);
		base.ViewModel.Offset.TempFloatValue.Subscribe(delegate(float value)
		{
			float num = value / 200f;
			m_Frame.offsetMin = new Vector2((float)ScreenWidth * num, (float)ScreenHeight * num);
			m_Frame.offsetMax = new Vector2((float)(-ScreenWidth) * num, (float)(-ScreenWidth) * num);
		}).AddTo(this);
		base.OnBind();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.SetEntitiesVertical<SettingsEntitySliderPCView>(m_OffsetSlider);
		navigationBehaviour.AddRow(AdditionalEntities);
		navigationBehaviour.FocusOnFirstValidEntity();
	}
}
