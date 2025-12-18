using DG.Tweening;
using R3;

namespace Kingmaker.Code.View.Bridge.Canvas;

public class FadeCanvas
{
	public readonly ReactiveCommand<bool> FadeoutCommand = new ReactiveCommand<bool>();

	public readonly ReactiveCommand<FadeoutAdvancedParams> FadeoutAdvancedCommand = new ReactiveCommand<FadeoutAdvancedParams>();

	public readonly ReactiveCommand<Unit> ShowLoadingScreenCommand = new ReactiveCommand<Unit>();

	public readonly ReactiveCommand<Unit> HideLoadingScreenCommand = new ReactiveCommand<Unit>();

	private static FadeCanvas s_Instance;

	public static FadeCanvas Instance => s_Instance ?? Initialize();

	private static FadeCanvas Initialize()
	{
		return s_Instance = new FadeCanvas();
	}

	public void Fadeout(bool fade)
	{
		FadeoutCommand.Execute(fade);
	}

	public void Fadeout(bool fade, float duration, Ease ease)
	{
		FadeoutAdvancedCommand.Execute(new FadeoutAdvancedParams
		{
			Fade = fade,
			Duration = duration,
			Ease = ease
		});
	}

	public void ShowLoadingScreen()
	{
		ShowLoadingScreenCommand.Execute();
	}

	public void HideLoadingScreen()
	{
		HideLoadingScreenCommand.Execute();
	}
}
