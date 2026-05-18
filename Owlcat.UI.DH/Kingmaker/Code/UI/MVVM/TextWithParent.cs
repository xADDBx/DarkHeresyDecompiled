using System;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public struct TextWithParent
{
	public GameObject Container;

	public TMP_Text Text;
}
