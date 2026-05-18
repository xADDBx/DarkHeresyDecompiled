using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.Code.UI.MVVM;

public static class BuffUIExtensions
{
	public static string GetDurationText(this Buff buff)
	{
		if (buff == null || buff.IsDisposed || buff.ExpirationInRounds <= 0)
		{
			return string.Empty;
		}
		if (buff.IsPermanent)
		{
			return ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.CommonTexts.PermanentBuffTimer;
		}
		string arg = ((buff.ExpirationInRounds == 1) ? ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round : ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds);
		return $"{buff.ExpirationInRounds} {arg}";
	}

	public static string GetSourceName(this Buff buff)
	{
		if (buff == null || buff.IsDisposed)
		{
			return string.Empty;
		}
		return (buff.Context.MaybeCaster as BaseUnitEntity)?.CharacterName ?? string.Empty;
	}

	public static string GetStacksText(this Buff buff)
	{
		if (buff == null || buff.IsDisposed)
		{
			return string.Empty;
		}
		if (buff.Blueprint.MaxRank <= 1)
		{
			return string.Empty;
		}
		return buff.GetRank() + "/" + buff.Blueprint.MaxRank;
	}
}
