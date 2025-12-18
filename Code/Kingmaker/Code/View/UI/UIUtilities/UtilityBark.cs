using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityBark
{
	public static float DefaultBarkTime = 5f;

	public static float CalculateBarkWidth(string text, float symWidth)
	{
		int length = text.Length;
		float num = 0f;
		if (text.Length > 25)
		{
			string[] array = text.Split(new char[1] { ' ' });
			int num2 = 0;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				num2 += text2.Length;
				if (num2 > 20)
				{
					break;
				}
				num2++;
			}
			num = symWidth * 0.58f * (float)num2;
			return Mathf.Max(Mathf.Ceil(symWidth * (float)length / Mathf.Sqrt(0.625f * (float)length)), num);
		}
		return Mathf.Ceil(symWidth * 0.58f * (float)length);
	}

	public static float GetBarkDuration(string text)
	{
		Locale currentLocale = LocalizationManager.Instance.CurrentLocale;
		if (currentLocale == Locale.zhCN || currentLocale == Locale.jaJP)
		{
			return (float)text.Length * 0.3f;
		}
		return (float)text.Split(' ').Length * 0.6f;
	}
}
