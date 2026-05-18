using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.Cheats;

public class StringDebugCopyHandler : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Init()
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			GameObject obj = new GameObject("StringDebugCopyHandler");
			Object.DontDestroyOnLoad(obj);
			obj.AddComponent<StringDebugCopyHandler>();
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			CheatsCommon.CopyStringDebugName();
		}
	}
}
