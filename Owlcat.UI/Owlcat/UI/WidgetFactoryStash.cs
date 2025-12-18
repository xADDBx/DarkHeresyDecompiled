using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Owlcat.UI;

public class WidgetFactoryStash : MonoBehaviour
{
	private static Action s_DestoryAll;

	private static WidgetFactoryStash s_Instance;

	public static WidgetFactoryStash Instance => s_Instance;

	public static bool Exists
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance.enabled;
			}
			return false;
		}
	}

	public static void RegisterDestroyAll(Action action)
	{
		s_DestoryAll = (Action)Delegate.Combine(s_DestoryAll, action);
	}

	public static void ResetStash()
	{
		WidgetFactory.DestroyAll();
		s_DestoryAll?.Invoke();
		if (s_Instance == null)
		{
			s_Instance = UnityEngine.Object.FindFirstObjectByType<WidgetFactoryStash>();
			if (s_Instance == null && SceneManager.GetActiveScene().isLoaded)
			{
				s_Instance = new GameObject("[WidgetFactoryStash]").AddComponent<WidgetFactoryStash>();
			}
			s_Instance.transform.position = new Vector3(10000f, 10000f, 10000f);
			UnityEngine.Object.DontDestroyOnLoad(s_Instance);
		}
	}

	private void LateUpdate()
	{
		WidgetFactory.DeactivateDisposedWidgets();
	}

	private void OnDestroy()
	{
		WidgetFactory.DestroyAll(fromOnDestroy: true);
		s_DestoryAll?.Invoke();
	}
}
