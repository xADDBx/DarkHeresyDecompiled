using System;
using System.Runtime.InteropServices;
using Kingmaker.Stores;
using Kingmaker.Utility.BuildModeUtils;
using Steamworks;

namespace Kingmaker.Networking;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct PhotonAppIdRealtime
{
	private const uint SteamReleaseAppId = 2186680u;

	private const string ReleaseAppIdRealtime = "8598483d-adba-4383-8630-196fd2fa98db";

	private const string DevelopmentAppIdRealtime = "8598483d-adba-4383-8630-196fd2fa98db";

	public bool IsRelease => "8598483d-adba-4383-8630-196fd2fa98db".Equals(Id, StringComparison.Ordinal);

	public bool IsDevelopment => "8598483d-adba-4383-8630-196fd2fa98db".Equals(Id, StringComparison.Ordinal);

	public string Id
	{
		get
		{
			if (!string.IsNullOrEmpty(BuildModeUtility.Data.AppIdRealtime))
			{
				return BuildModeUtility.Data.AppIdRealtime;
			}
			if (StoreManager.Store == StoreType.Steam)
			{
				if (!SteamManager.Initialized)
				{
					throw new StoreNotInitializedException();
				}
				_ = SteamUtils.GetAppID().m_AppId;
				_ = 2186680;
				return "8598483d-adba-4383-8630-196fd2fa98db";
			}
			return "8598483d-adba-4383-8630-196fd2fa98db";
		}
	}
}
