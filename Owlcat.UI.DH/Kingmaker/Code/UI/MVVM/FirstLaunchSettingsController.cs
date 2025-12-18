using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchSettingsController : MonoBehaviour
{
	public static FirstLaunchSettingsController Instance;

	[SerializeField]
	private FirstLaunchSettingsPCView m_FirstLaunchSettingsPCView;

	private Action m_OnCompleteCallback;

	private FirstLaunchSettingsVM m_FirstLaunchSettingsVM;
}
