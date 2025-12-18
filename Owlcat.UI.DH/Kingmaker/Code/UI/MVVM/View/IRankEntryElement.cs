using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public interface IRankEntryElement
{
	MonoBehaviour MonoBehaviour { get; }

	void SetRotation(float angle, bool hasArrow);

	void StartHighlight(string key);

	void StopHighlight();
}
