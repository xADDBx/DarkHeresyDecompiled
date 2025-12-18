using System;
using R3;
using TMPro;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.Components.Text;

public static class TMPExtension
{
	public static bool IsAnyVisibleChars(this TextMeshProUGUI text)
	{
		if (text == null)
		{
			return false;
		}
		if (text.textInfo.characterCount > 0)
		{
			return Array.Exists(text.textInfo.characterInfo, (TMP_CharacterInfo x) => x.isVisible);
		}
		return false;
	}

	public static Observable<(PointerEventData Arg0, TMP_LinkInfo Arg1)> OnClickAsObservable(this TMPLinkHandler linkHandler)
	{
		return linkHandler.OnClick.AsObservable();
	}
}
