using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.UI;

[CreateAssetMenu]
public class HintGamepadIconProvider : ScriptableObject, ISerializationCallbackReceiver, IHintIconProvider
{
	[Serializable]
	public struct Face
	{
		public Sprite North;

		public Sprite East;

		public Sprite South;

		public Sprite West;

		public readonly void RegisterTo(Dictionary<string, Sprite> map)
		{
			map.Add("buttonNorth", North);
			map.Add("buttonEast", East);
			map.Add("buttonSouth", South);
			map.Add("buttonWest", West);
		}
	}

	[Serializable]
	private struct DPad
	{
		public Sprite Up;

		public Sprite Right;

		public Sprite Down;

		public Sprite Left;

		public Sprite Horizontal;

		public Sprite Vertical;

		public Sprite Empty;

		public Sprite Full;

		public readonly void RegisterTo(Dictionary<string, Sprite> map)
		{
			map.Add("dpadUp", Up);
			map.Add("dpadRight", Right);
			map.Add("dpadDown", Down);
			map.Add("dpadLeft", Left);
		}
	}

	[Serializable]
	private struct Stick
	{
		public Sprite Button;

		public Sprite Up;

		public Sprite Right;

		public Sprite Down;

		public Sprite Left;

		public Sprite Horizontal;

		public Sprite Vertical;

		public Sprite All;
	}

	private static readonly string kScheme = "gamepad:";

	private static readonly char[] kSeparators = new char[2] { ';', '#' };

	[SerializeField]
	private Face m_Face;

	[SerializeField]
	private DPad m_DPad;

	[SerializeField]
	private Stick m_LeftStick;

	[SerializeField]
	private Stick m_RightStick;

	[SerializeField]
	private Sprite m_Options;

	[SerializeField]
	private Sprite m_FuncAdditional;

	private Dictionary<string, Sprite> m_Icons;

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		m_Icons = new Dictionary<string, Sprite>();
		m_Face.RegisterTo(m_Icons);
		m_DPad.RegisterTo(m_Icons);
	}

	public bool TryGetIcon(string binding, out Sprite sprite)
	{
		int num = binding.IndexOf(kScheme);
		if (num != -1)
		{
			num += kScheme.Length;
			int num2 = binding.IndexOfAny(kSeparators, num);
			num2 = ((num2 == -1) ? (binding.Length - 1) : (num2 - 1));
			string key = binding.Substring(num, num2 - num + 1);
			if (m_Icons.TryGetValue(key, out sprite))
			{
				return sprite != null;
			}
		}
		sprite = null;
		return sprite != null;
	}
}
