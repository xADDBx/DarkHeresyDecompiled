using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.UI;

public class UnityThreadHolder
{
	[CanBeNull]
	public static Thread UnityThread;

	public static bool IsMainThread => Thread.CurrentThread == UnityThread;

	[RuntimeInitializeOnLoadMethod]
	private static void Awake()
	{
		UnityThread = Thread.CurrentThread;
	}
}
