using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Code.View._TmpUI;

public static class CursorTest
{
	private static int m_CurrentCursorIndex = -1;

	private static CursorTestTextures m_Textures;

	private static List<Texture2D> m_TexturesList;

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void TestCursor()
	{
		if (m_CurrentCursorIndex >= 0 && m_CurrentCursorIndex < m_TexturesList.Count)
		{
			Cursor.SetCursor(m_TexturesList[m_CurrentCursorIndex], Vector2.zero, CursorMode.Auto);
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning($"Cursor: {m_CurrentCursorIndex}");
			});
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void NextCursor()
	{
		if (m_Textures == null)
		{
			m_Textures = Resources.LoadAll<CursorTestTextures>("")?.FirstOrDefault();
			m_TexturesList = m_Textures.CursorTextures;
		}
		m_CurrentCursorIndex++;
		if (m_CurrentCursorIndex < 0 || m_CurrentCursorIndex >= m_TexturesList.Count)
		{
			m_CurrentCursorIndex = 0;
		}
		TestCursor();
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void PrevCursor()
	{
		if (m_Textures == null)
		{
			m_Textures = Resources.LoadAll<CursorTestTextures>("")?.FirstOrDefault();
			m_TexturesList = m_Textures.CursorTextures;
		}
		m_CurrentCursorIndex--;
		if (m_CurrentCursorIndex < 0 || m_CurrentCursorIndex >= m_TexturesList.Count)
		{
			m_CurrentCursorIndex = m_TexturesList.Count - 1;
		}
		TestCursor();
	}
}
