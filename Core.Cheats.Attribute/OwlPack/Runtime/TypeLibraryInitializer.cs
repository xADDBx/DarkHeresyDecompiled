using System;
using System.Collections.Generic;
using UnityEngine;

namespace OwlPack.Runtime;

public static class TypeLibraryInitializer
{
	public static void Initialize()
	{
		Dictionary<string, Type> dictionary = null;
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Type> item in dictionary)
		{
			if (!TypeLibrary.OldNames.ContainsKey(item.Key))
			{
				TypeLibrary.OldNames.Add(item.Key, item.Value);
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void InitializeInRuntime()
	{
		Initialize();
	}
}
