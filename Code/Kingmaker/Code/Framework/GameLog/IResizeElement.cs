using UnityEngine;

namespace Kingmaker.Code.Framework.GameLog;

public interface IResizeElement
{
	void SetSizeDelta(Vector2 size);

	Vector2 GetSize();

	RectTransform GetTransform();
}
