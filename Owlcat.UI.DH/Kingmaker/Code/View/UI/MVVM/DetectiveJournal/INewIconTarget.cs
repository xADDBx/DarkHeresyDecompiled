using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public interface INewIconTarget
{
	RectTransform Parent { get; }

	ReadOnlyReactiveProperty<RectTransform> NewIcon { get; }
}
