using System;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityLink
{
	public static string PackKeys(params object[] word)
	{
		string text = "";
		for (int i = 0; i < word.Length; i++)
		{
			text += string.Format((i + 1 == word.Length) ? "{0}" : "{0}:", word[i]);
		}
		return text;
	}

	public static string[] GetKeysFromLink(string linkID)
	{
		string[] source = linkID.Split(':');
		_ = new string[0];
		return source.Where((string x) => !string.IsNullOrEmpty(x)).ToArray();
	}

	public static bool CheckLinkKeyHasContent(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		string[] keysFromLink = GetKeysFromLink(key);
		if (EntityLink.GetEntityType(keysFromLink[0]) != EntityLink.Type.Encyclopedia)
		{
			return true;
		}
		BlueprintEncyclopediaEntry encyclopediaEntry = UtilityEncyclopedy.GetEncyclopediaEntry(keysFromLink[1]);
		if (encyclopediaEntry != null)
		{
			string value = ((from b in encyclopediaEntry.GetTooltipInfo()?.Where((EncyclopediaEntryBlock b) => b.blockType == EncyclopediaEntryBlock.BlockType.Text)
				select b.GetDescription()?.Text) ?? Array.Empty<string>()).FirstOrDefault((string text) => !string.IsNullOrWhiteSpace(text));
			if (BuildModeUtility.IsDevelopment && !string.IsNullOrEmpty(encyclopediaEntry.Title.Text) && !string.IsNullOrEmpty(value))
			{
				return true;
			}
			if (encyclopediaEntry.Title.IsSet() && !string.IsNullOrEmpty(value))
			{
				return true;
			}
		}
		BlueprintEncyclopediaGlossaryEntry glossaryEntry = UtilityEncyclopedy.GetGlossaryEntry(keysFromLink[1]);
		if (glossaryEntry == null)
		{
			return false;
		}
		if (!glossaryEntry.GetDescription().IsSet() && !glossaryEntry.Title.IsSet())
		{
			return false;
		}
		return true;
	}

	public static IDisposable SetTextLink(TextMeshProUGUI text, Camera camera = null)
	{
		if (text == null)
		{
			return Disposable.Empty;
		}
		bool entered = false;
		int? linkIndex = null;
		string[] linkKeys = null;
		IDisposable enter = text.OnPointerEnterAsObservable().Subscribe(delegate
		{
			entered = true;
		});
		IDisposable exit = text.OnPointerExitAsObservable().Subscribe(delegate
		{
			entered = false;
		});
		IDisposable update = ObservableSubscribeExtensions.Subscribe(text.UpdateAsObservable(), delegate
		{
			int num;
			if (!entered || (num = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, (camera != null) ? camera : UICamera.Claim())) == -1)
			{
				if (linkIndex.HasValue)
				{
					linkIndex = null;
					linkKeys = null;
				}
			}
			else if (num != linkIndex)
			{
				linkIndex = num;
				linkKeys = GetKeysFromLink(text.textInfo.linkInfo[linkIndex.Value].GetLinkID());
			}
		});
		IDisposable click = text.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
		{
			if (linkKeys.Any() && data.button == PointerEventData.InputButton.Left)
			{
				string text2 = linkKeys[0];
				if (text2 == "http" || text2 == "https")
				{
					text2 = linkKeys[1];
				}
				if (text2.Contains("://") && !text2.Contains("http"))
				{
					text2 = "http" + text2;
				}
				else if (!text2.Contains("http://") && !text2.Contains("https://"))
				{
					text2 = "http://" + text2;
				}
				Application.OpenURL(text2);
			}
		});
		return Disposable.Create(delegate
		{
			if (entered)
			{
				entered = false;
			}
			enter?.Dispose();
			exit?.Dispose();
			update?.Dispose();
			click?.Dispose();
		});
	}
}
