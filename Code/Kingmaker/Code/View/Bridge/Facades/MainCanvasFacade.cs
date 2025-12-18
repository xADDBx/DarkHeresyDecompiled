using Kingmaker.Code.View.Bridge.Interfaces.Canvas;

namespace Kingmaker.Code.View.Bridge.Facades;

public class MainCanvasFacade
{
	private static IMainCanvas s_Instance;

	public static IMainCanvas Instance => GetMainCanvas();

	private static IMainCanvas GetMainCanvas()
	{
		if (s_Instance != null && (bool)s_Instance.RectTransform)
		{
			return s_Instance;
		}
		IMainCanvas mainCanvas = Game.Instance.RootUIContext.MainCanvas;
		if (mainCanvas == null || !mainCanvas.RectTransform)
		{
			PFLog.UI.Error("Failed to get Main Canvas!");
			s_Instance = null;
			return null;
		}
		s_Instance = mainCanvas;
		return s_Instance;
	}
}
