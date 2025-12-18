using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class UnitAnimationManagerEditorTicker : MonoBehaviour
{
	private void Update()
	{
		Object.FindObjectsByType<UnitAnimationManager>(FindObjectsSortMode.None).ForEach(delegate(UnitAnimationManager m)
		{
			m.Tick(Time.deltaTime);
		});
	}
}
