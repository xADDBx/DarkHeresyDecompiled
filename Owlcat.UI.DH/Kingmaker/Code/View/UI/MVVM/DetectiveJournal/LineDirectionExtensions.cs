namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public static class LineDirectionExtensions
{
	public static LineDirectionData Negate(this LineDirectionData directionData)
	{
		LineDirectionData lineDirectionData = new LineDirectionData(directionData);
		lineDirectionData.Length *= -1f;
		return lineDirectionData;
	}
}
