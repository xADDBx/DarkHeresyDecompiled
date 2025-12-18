using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("fd601a0246034ca38414c127dbcc65ea")]
public class DlcStoreCheat : DlcStore
{
	[SerializeField]
	private string m_TestShopLink = "https://roguetrader.owlcat.games/";

	[SerializeField]
	[Tooltip("Is the DLC available in editor playmode")]
	private bool m_IsAvailableInEditor;

	[ShowIf("m_IsAvailableInEditor")]
	[SerializeField]
	[Tooltip("Is the DLC active in editor playmode")]
	private bool m_IsActiveInEditor;

	[SerializeField]
	[Tooltip("Is the DLC available in development build")]
	private bool m_IsAvailableInDevBuild;

	private bool m_IsPurchased = true;

	private bool m_IsLoading;

	private bool m_IsMounted = true;

	private static Dictionary<string, bool> s_OverrideEnable = new Dictionary<string, bool>();

	public bool IsAvailableInEditor => m_IsAvailableInEditor;

	public bool IsActiveInEditor
	{
		get
		{
			if (m_IsAvailableInEditor)
			{
				return m_IsActiveInEditor;
			}
			return false;
		}
	}

	public bool IsAvailableInDevBuild => m_IsAvailableInDevBuild;

	public override bool IsSuitable => BuildModeUtility.CheatStoreEnabled;

	public override bool AllowsInstalling => true;

	public override bool AllowsDeleting => true;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on StoreCheat.";

	public override IDLCStatus GetStatus()
	{
		_ = (base.OwnerBlueprint as IBlueprintDlc)?.DlcType ?? DlcTypeEnum.CosmeticDlc;
		return new DLCStatus
		{
			Purchased = m_IsPurchased,
			DownloadState = (m_IsMounted ? DownloadState.Loaded : (m_IsLoading ? DownloadState.Loading : DownloadState.NotLoaded)),
			IsMounted = m_IsMounted
		};
	}

	public override bool OpenShop()
	{
		bool isAvailable = false;
		if (!IsSuitable)
		{
			return false;
		}
		try
		{
			UtilityMessageBox.ShowMessageBox("Would you like to buy DLC: " + ((base.OwnerBlueprint as IBlueprintDlc)?.DlcDisplayName ?? base.OwnerBlueprint.name), DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					isAvailable = true;
					m_IsPurchased = true;
					StoreManager.RefreshAllDLCStatuses();
					StartDownloadAsync();
				}
			});
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return isAvailable;
	}

	public override bool Mount()
	{
		return false;
	}

	public override bool Delete()
	{
		StartDeleteAsync();
		return true;
	}

	private async Task StartDeleteAsync()
	{
		m_IsMounted = false;
		await Task.Delay(3000);
		StoreManager.RefreshAllDLCStatuses();
	}

	public override bool Install()
	{
		StartDownloadAsync();
		return true;
	}

	private async Task StartDownloadAsync()
	{
		await Task.Delay(1000);
		m_IsLoading = true;
		StoreManager.RefreshAllDLCStatuses();
		await Task.Delay(3000);
		m_IsLoading = false;
		m_IsMounted = true;
		StoreManager.RefreshAllDLCStatuses();
	}

	public static void EnableDlc(BlueprintDlc dlc)
	{
		if (dlc != null && !string.IsNullOrEmpty(dlc.AssetGuid) && !s_OverrideEnable.TryAdd(dlc.AssetGuid, value: true))
		{
			s_OverrideEnable[dlc.AssetGuid] = true;
		}
	}

	public static void DisableDlc(BlueprintDlc dlc)
	{
		if (dlc != null && !string.IsNullOrEmpty(dlc.AssetGuid) && !s_OverrideEnable.TryAdd(dlc.AssetGuid, value: false))
		{
			s_OverrideEnable[dlc.AssetGuid] = false;
		}
	}

	public static bool TryIsDlcEnable([CanBeNull] BlueprintDlc dlc, out bool result)
	{
		result = false;
		if (dlc == null || string.IsNullOrEmpty(dlc.AssetGuid) || !s_OverrideEnable.ContainsKey(dlc.AssetGuid))
		{
			return false;
		}
		result = s_OverrideEnable[dlc.AssetGuid];
		return true;
	}
}
