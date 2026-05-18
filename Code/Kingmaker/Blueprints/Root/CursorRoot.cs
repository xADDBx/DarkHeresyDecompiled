using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

public class CursorRoot : ScriptableObject
{
	[SerializeField]
	private List<CursorEntry> m_CursorEntries;

	public Sprite GetSprite(CursorType type)
	{
		return m_CursorEntries.FirstOrDefault((CursorEntry e) => e.Type == type)?.Sprite;
	}

	public Texture2D GetTexture(CursorType type)
	{
		return m_CursorEntries.FirstOrDefault((CursorEntry e) => e.Type == type)?.Texture;
	}

	public bool TryGetTexture(CursorType cursorType, out Texture2D texture, out Vector2 pivot)
	{
		texture = GetTexture(cursorType);
		bool flag = cursorType == CursorType.Vertical || cursorType == CursorType.Horizontal || cursorType == CursorType.DiagonalLeft || cursorType == CursorType.DiagonalRight || cursorType == CursorType.RotateCamera;
		pivot = (flag ? (Vector2.one * 0.5f) : Vector2.up);
		return texture != null;
	}
}
