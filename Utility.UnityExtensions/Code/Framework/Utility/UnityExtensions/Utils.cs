using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Code.Framework.Utility.UnityExtensions;

public static class Utils
{
	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T val = lhs;
		lhs = rhs;
		rhs = val;
	}

	public static void EditorSafeDestroy(UnityEngine.Object obj)
	{
		UnityEngine.Object.Destroy(obj);
	}

	public static bool IsNullOrEmpty(this string _this)
	{
		return string.IsNullOrEmpty(_this);
	}

	public static string EmptyToNull(this string @this)
	{
		if (!(@this != ""))
		{
			return null;
		}
		return @this;
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

	public static void ShowWindowsMessage(string message, string title = "Info")
	{
		MessageBox(IntPtr.Zero, message, title, 64u);
	}
}
