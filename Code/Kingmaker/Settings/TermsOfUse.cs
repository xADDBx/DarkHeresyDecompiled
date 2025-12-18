using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Settings;

public static class TermsOfUse
{
	public static bool TermsOfUseAccepted => PlayerPrefs.GetInt(TermsOfUseAcceptedKey) != 0;

	public static string TermsOfUseAcceptedKey => "TermsOfUseAccepted";

	public static void AcceptTermOfUse()
	{
		PlayerPrefs.SetInt(TermsOfUseAcceptedKey, 1);
	}

	public static void DeclineTermOfUse()
	{
		SystemUtil.ApplicationQuit();
	}
}
