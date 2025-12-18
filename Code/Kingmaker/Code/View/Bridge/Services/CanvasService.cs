using System;
using Kingmaker.Code.View.Bridge.Interfaces.Canvas;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Services;

public static class CanvasService
{
	public static Func<GameObject, ICanvasAnimation> CreateCanvasAnimation;
}
