using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.UI.Canvases;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Init;

internal static class CanvasServiceInit
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitializeCanvasService()
	{
		CanvasService.CreateCanvasAnimation = (GameObject go) => go.GetComponent<CanvasAnimation>();
	}
}
