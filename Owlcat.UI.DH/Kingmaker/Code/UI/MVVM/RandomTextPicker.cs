using System;
using System.Text;
using Code.Framework.Utility.UnityExtensions;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RandomTextPicker : RandomPickerBase
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private string m_Format = "";

	private string m_InitString;

	public override void Randomize(string seed)
	{
		Reset();
		if (m_Text == null || m_Format.IsNullOrEmpty())
		{
			return;
		}
		System.Random random = new System.Random(seed.GetHashCode() & 0x7FFFFFFF);
		StringBuilder stringBuilder = new StringBuilder(m_Format.Length);
		bool flag = false;
		string format = m_Format;
		foreach (char c in format)
		{
			if (flag)
			{
				stringBuilder.Append(c);
				flag = false;
				continue;
			}
			if (c == '\\')
			{
				flag = true;
				continue;
			}
			char value;
			switch (c)
			{
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
			case 'G':
			case 'H':
			case 'I':
			case 'J':
			case 'K':
			case 'L':
			case 'M':
			case 'N':
			case 'O':
			case 'P':
			case 'Q':
			case 'R':
			case 'S':
			case 'T':
			case 'U':
			case 'V':
			case 'W':
			case 'X':
			case 'Y':
			case 'Z':
				value = (char)(65 + random.Next(26));
				break;
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				value = (char)(97 + random.Next(26));
				break;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				value = (char)(48 + random.Next(10));
				break;
			default:
				value = c;
				break;
			}
			stringBuilder.Append(value);
		}
		if (flag)
		{
			stringBuilder.Append('\\');
		}
		m_Text.text = stringBuilder.ToString();
	}

	public override void Reset()
	{
		if (m_InitString == null)
		{
			m_InitString = m_Text?.text;
		}
		m_Text.text = m_InitString;
	}
}
