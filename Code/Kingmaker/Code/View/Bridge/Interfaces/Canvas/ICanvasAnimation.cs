using Kingmaker.EntitySystem.Persistence;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Interfaces.Canvas;

public interface ICanvasAnimation : ILoadingScreen
{
	GameObject GameObject { get; }
}
