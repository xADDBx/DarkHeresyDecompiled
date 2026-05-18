using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public struct TitleElement
{
	public TextEntityWidget Text;

	public GameObject Container;

	public LayoutGroup LayoutGroup;
}
