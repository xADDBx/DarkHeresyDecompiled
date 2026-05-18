using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public class CommandHintIconSet
{
	[Serializable]
	private class Entry
	{
		public string Binding;

		public Sprite Sprite;
	}

	[SerializeField]
	private List<Entry> m_Icons;

	public Sprite GetIcon(string binding)
	{
		string text = Regex.Replace(binding, "#.+$", string.Empty);
		foreach (Entry icon in m_Icons)
		{
			if (icon.Binding == text)
			{
				return icon.Sprite;
			}
		}
		return null;
	}
}
