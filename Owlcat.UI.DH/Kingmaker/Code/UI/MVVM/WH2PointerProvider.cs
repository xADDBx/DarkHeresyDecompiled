using Kingmaker.UI.Pointer;
using Owlcat.UI.Navigation;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal class WH2PointerProvider : IPointerProvider
{
	private readonly CursorController m_Cursor;

	public bool Enabled
	{
		get
		{
			return m_Cursor.IsCursorActive;
		}
		set
		{
			m_Cursor.SetActive(value);
		}
	}

	public Vector2 Position
	{
		get
		{
			return m_Cursor.Position;
		}
		set
		{
		}
	}

	public WH2PointerProvider(CursorController cursor)
	{
		m_Cursor = cursor;
	}
}
