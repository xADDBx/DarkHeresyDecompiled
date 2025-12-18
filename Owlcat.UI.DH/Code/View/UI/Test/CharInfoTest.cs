using Kingmaker.Cheats;
using UnityEngine;

namespace Code.View.UI.Test;

public class CharInfoTest : MonoBehaviour
{
	private void OnEnable()
	{
		CheatsUnlock.UnlockCompanionAllStories(null);
	}
}
