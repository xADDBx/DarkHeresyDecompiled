using Kingmaker.Code.View.Bridge.Root;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public interface ICharGenPhaseRoadmapView : IInitializable
{
	RectTransform ViewRectTransform { get; }

	void SetParentTransform(Transform parent, int siblingIndex = 0);

	CharGenPhaseBaseVM GetPhaseBaseVM();
}
