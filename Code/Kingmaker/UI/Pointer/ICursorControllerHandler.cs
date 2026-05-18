using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public interface ICursorControllerHandler : ISubscriber
{
	void HandleActiveChanged(bool value);

	void HandleTypeChanged(CursorType type);

	void HandleTypeChanged(CursorType type, Sprite icon);

	void HandleTextsChanged(string upper, string lower);

	void HandleNoMoveChanged(bool value);
}
