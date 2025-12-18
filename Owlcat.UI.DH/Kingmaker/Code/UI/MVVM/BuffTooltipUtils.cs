using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.Code.UI.MVVM;

public static class BuffTooltipUtils
{
	public static string GetDuration(Buff buff)
	{
		if (buff.IsPermanent)
		{
			return ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.CommonTexts.PermanentBuffTimer;
		}
		if (buff.ExpirationInRounds <= 0)
		{
			return string.Empty;
		}
		string arg = ((buff.ExpirationInRounds == 1) ? ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round : ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds);
		return $"{buff.ExpirationInRounds} {arg}";
	}
}
