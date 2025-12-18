using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

[Serializable]
internal struct DollPosition
{
	public CharacterDollPosition Position;

	public RectTransform Transform;
}
