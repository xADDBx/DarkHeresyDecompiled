using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SaveFullScreenshotBaseView : View<SaveSlotVM>
{
	[Header("Elements")]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private RawImage m_ScreenshotImage;

	[SerializeField]
	private TMP_Text m_LocationLabel;

	[SerializeField]
	private TMP_Text m_InGameTimeLabel;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		m_FadeAnimator.AppearAnimation();
		base.ViewModel.ScreenShotHighRes.Subscribe(SetScreenshot).AddTo(this);
		m_LocationLabel.text = base.ViewModel.LocationName.CurrentValue;
		m_InGameTimeLabel.text = base.ViewModel.TimeInGame.CurrentValue;
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
		base.ViewModel.DisposeHighResScreenshot();
	}

	private void SetScreenshot(Texture2D screenshot)
	{
		if (screenshot != null && screenshot.width == 4 && screenshot.height == 4)
		{
			screenshot = null;
		}
		m_ScreenshotImage.gameObject.SetActive(screenshot != null);
		if (screenshot != null)
		{
			m_ScreenshotImage.texture = screenshot;
			m_ScreenshotImage.GetComponent<AspectRatioFitter>().aspectRatio = screenshot.GetAspect();
		}
	}
}
