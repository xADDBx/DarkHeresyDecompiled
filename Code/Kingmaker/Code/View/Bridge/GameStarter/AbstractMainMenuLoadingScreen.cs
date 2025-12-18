using System;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.GameStarter;

public abstract class AbstractMainMenuLoadingScreen : MonoBehaviour
{
	public abstract void StartLoading(Action callback);

	public abstract void EndLoading(Action callback);

	public abstract void SetLoadingProgress(float virtualProgress);
}
