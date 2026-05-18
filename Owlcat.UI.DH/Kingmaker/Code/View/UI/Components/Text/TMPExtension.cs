using System;
using R3;
using TMPro;
using UnityEngine;
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

	public static string FindTruncatedLinkId(this TMP_Text text, Vector3 position, UnityEngine.Camera camera)
	{
		int num = TMP_TextUtilities.FindIntersectingCharacter(text, position, camera, visibleOnly: true);
		if (num == -1)
		{
			return null;
		}
		int index = text.textInfo.characterInfo[num].index;
		string text2 = text.text;
		string result = null;
		int num2 = 0;
		while (num2 <= index && num2 < text2.Length)
		{
			int num3 = text2.IndexOf("<link=", num2, StringComparison.OrdinalIgnoreCase);
			if (num3 == -1 || num3 > index)
			{
				break;
			}
			int num4 = num3 + 6;
			if (num4 >= text2.Length)
			{
				break;
			}
			string text3;
			int num6;
			if (text2[num4] == '"')
			{
				num4++;
				int num5 = text2.IndexOf('"', num4);
				if (num5 == -1)
				{
					break;
				}
				text3 = text2.Substring(num4, num5 - num4);
				num6 = text2.IndexOf('>', num5);
			}
			else
			{
				num6 = text2.IndexOf('>', num4);
				if (num6 == -1)
				{
					break;
				}
				text3 = text2.Substring(num4, num6 - num4);
			}
			if (num6 == -1)
			{
				break;
			}
			num6++;
			int num7 = text2.IndexOf("</link>", num6, StringComparison.OrdinalIgnoreCase);
			result = ((num7 == -1 || num7 > index) ? text3 : null);
			num2 = num6;
		}
		return result;
	}
}
