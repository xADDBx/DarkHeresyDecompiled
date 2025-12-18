using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Visual;
using UnityEngine;

namespace Kingmaker.UI;

public class UICamera : MonoBehaviour
{
	public enum UICameraType
	{
		MainCamera,
		AdditionalCamera
	}

	private static UICamera s_Instance;

	[SerializeField]
	private Camera m_MainUICamera;

	[SerializeField]
	private Camera m_AdditionalUICamera;

	public static Camera Instance
	{
		get
		{
			if (!s_Instance)
			{
				return null;
			}
			return MainUICamera;
		}
	}

	private static UICamera Prefab => ConfigRoot.Instance.Prefabs.UICamera;

	public static Camera MainUICamera => s_Instance.m_MainUICamera;

	public static Camera AdditionalUICamera => s_Instance?.m_AdditionalUICamera;

	[NotNull]
	public static Camera Claim(UICameraType type = UICameraType.MainCamera)
	{
		if (Instance == null)
		{
			TryCreateCamera();
		}
		return type switch
		{
			UICameraType.MainCamera => MainUICamera, 
			UICameraType.AdditionalCamera => AdditionalUICamera, 
			_ => MainUICamera, 
		};
	}

	private static bool TryCreateCamera()
	{
		using (ProfileScope.New("Load UI Camera"))
		{
			if (Application.isPlaying && (bool)Prefab)
			{
				Object.Instantiate(Prefab);
				return true;
			}
			PFLog.System.Error("Failed to load UI camera prefab");
			return false;
		}
	}

	public void Awake()
	{
		if (m_MainUICamera == null)
		{
			PFLog.System.Error("UI Main Camera not found");
		}
		else if (m_AdditionalUICamera == null)
		{
			PFLog.System.Error("UI Additional Camera not found");
		}
		else if (Application.isPlaying)
		{
			if (s_Instance != null)
			{
				Object.Destroy(this);
			}
			else
			{
				Object.DontDestroyOnLoad(this);
			}
		}
	}

	private void OnEnable()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}
		if ((bool)m_MainUICamera)
		{
			CameraStackManager.Instance.AddCamera(m_MainUICamera, CameraStackManager.CameraStackType.Ui);
		}
	}

	private void OnDisable()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if ((bool)m_MainUICamera)
		{
			CameraStackManager.Instance.RemoveCamera(m_MainUICamera, CameraStackManager.CameraStackType.Ui);
		}
	}
}
