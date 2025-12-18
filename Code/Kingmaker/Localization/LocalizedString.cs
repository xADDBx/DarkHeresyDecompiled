using System;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.TextTools.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Localization;

[Serializable]
public class LocalizedString
{
	private const string JsonExt = ".json";

	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Localization");

	[SerializeField]
	private string m_Key = "";

	[SerializeField]
	private string m_Kind = "";

	private bool? m_ShouldProcess;

	private Locale m_ShouldProcessLocale;

	private bool m_IsReportedAsMissingString;

	public const string KIND_BARK = "bark";

	public const string KIND_BUFF = "buff";

	public const string KIND_CUE = "cue";

	public const string KIND_ANSWER = "answer";

	public const string KIND_BARKBANTER = "barkbanter";

	public const string KIND_ASK = "ask";

	public const string KIND_CASE = "cases";

	public const string KIND_BARKCUTSCENE = "barkcutscene";

	[CanBeNull]
	public SharedStringAsset Shared;

	public string Key
	{
		get
		{
			return m_Key;
		}
		set
		{
			m_Key = value;
		}
	}

	public string Text => GetText();

	public bool Empty => IsEmpty();

	private string GetText()
	{
		using (ProfileScope.New("LocalizedString GetText"))
		{
			if (IsEmpty())
			{
				return string.Empty;
			}
			if (!LoadImpl(out var txt) && !m_IsReportedAsMissingString)
			{
				m_IsReportedAsMissingString = true;
			}
			if (!UnitySyncContextHolder.IsInUnity || !Application.isPlaying)
			{
				return txt;
			}
			if (!m_ShouldProcess.HasValue || m_ShouldProcessLocale != LocalizationManager.Instance.CurrentLocale)
			{
				m_ShouldProcess = txt.Contains("{");
				m_ShouldProcessLocale = LocalizationManager.Instance.CurrentLocale;
			}
			return TextTemplateEngineProxy.Instance.Process(txt);
		}
	}

	private bool LoadImpl(out string txt)
	{
		if (LocalizationManager.Instance.CurrentPack != null && TryGetText(LocalizationManager.Instance.CurrentPack, out txt))
		{
			return true;
		}
		using (ProfileScope.New("Backup Locales"))
		{
			LocalizationPack localizationPack = LocalizationManager.Instance.BackupPacks.FindOrDefault((LocalizationPack pack) => pack?.Locale == LocalizationManager.Instance.CurrentLocale);
			if (localizationPack != null && TryGetText(localizationPack, out var text))
			{
				txt = text;
				return true;
			}
			LocalizationPack[] backupPacks = LocalizationManager.Instance.BackupPacks;
			foreach (LocalizationPack localizationPack2 in backupPacks)
			{
				if (localizationPack2 != localizationPack && TryGetText(localizationPack2, out var text2))
				{
					txt = ((!BuildModeUtility.IsPlayTest) ? $"[{localizationPack2.Locale}] {text2}" : text2);
					return true;
				}
			}
		}
		txt = string.Empty;
		return false;
	}

	public static implicit operator string(LocalizedString localizedString)
	{
		if (localizedString != null)
		{
			return localizedString.Text;
		}
		return "<null>";
	}

	public static LocalizedString Dereference(LocalizedString ls)
	{
		int num = 0;
		while ((bool)ls.Shared)
		{
			if (num++ > 50)
			{
				return null;
			}
			ls = ls.Shared.String;
		}
		return ls;
	}

	private bool TryGetText([NotNull] LocalizationPack pack, out string text)
	{
		LocalizedString localizedString = Dereference(this);
		if (localizedString == null)
		{
			Logger.Error("Cyclic reference in string {0}", this);
			text = "";
			return false;
		}
		string key = localizedString.m_Key;
		if (key == "")
		{
			text = "";
			return false;
		}
		return pack.TryGetText(key, out text);
	}

	public string ToString(Action scope)
	{
		using (GameLogContext.Scope)
		{
			try
			{
				scope();
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
			return this;
		}
	}

	public bool IsSet()
	{
		if (LocalizationManager.Instance.CurrentPack != null && TryGetText(LocalizationManager.Instance.CurrentPack, out var text) && !string.IsNullOrEmpty(text))
		{
			return true;
		}
		return false;
	}

	public bool IsEmpty()
	{
		if (!Shared)
		{
			return m_Key == "";
		}
		return Shared.String.IsEmpty();
	}
}
