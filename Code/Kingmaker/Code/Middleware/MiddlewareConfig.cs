using System;
using UnityEngine;

namespace Kingmaker.Code.Middleware;

public class MiddlewareConfig : ScriptableObject
{
	[Serializable]
	public class Data
	{
		public string MetricsProjectId;
	}

	private static MiddlewareConfig _instance;

	[SerializeField]
	private Data _data = new Data();

	public static string FileName = "MiddlewareConfig";

	public static Data Get
	{
		get
		{
			if (_instance != null)
			{
				return _instance._data;
			}
			_instance = Resources.Load<MiddlewareConfig>(FileName) ?? ScriptableObject.CreateInstance<MiddlewareConfig>();
			return _instance._data;
		}
	}
}
