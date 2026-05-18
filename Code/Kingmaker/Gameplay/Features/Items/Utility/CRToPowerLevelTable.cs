namespace Kingmaker.Gameplay.Features.Items.Utility;

public static class CRToPowerLevelTable
{
	public static ItemPowerLevel Lookup(CRToPowerLevelEntry[] entries, int cr)
	{
		if (entries == null)
		{
			return ItemPowerLevel.Undefined;
		}
		ItemPowerLevel result = ItemPowerLevel.Undefined;
		int num = int.MinValue;
		for (int i = 0; i < entries.Length; i++)
		{
			CRToPowerLevelEntry cRToPowerLevelEntry = entries[i];
			if (cRToPowerLevelEntry.MinCR <= cr && cRToPowerLevelEntry.MinCR > num)
			{
				num = cRToPowerLevelEntry.MinCR;
				result = cRToPowerLevelEntry.PowerLevel;
			}
		}
		return result;
	}
}
