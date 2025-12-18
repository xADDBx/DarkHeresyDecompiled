using System;
using Kingmaker.Blueprints;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
public class VoIdIndex
{
	public int Index;

	public string GetVoGuid(BlueprintUnit blueprint)
	{
		string result = "";
		if (blueprint == null)
		{
			return result;
		}
		result = ((Index == 0) ? blueprint.VoId.Guid : ((!blueprint.HasAdditionalVoIds || blueprint.AdditionalVoIds.Count < Index) ? blueprint.VoId.Guid : blueprint.AdditionalVoIds[Index - 1].Guid));
		if (!string.IsNullOrEmpty(result))
		{
			return result;
		}
		return "Missing VoGuid for Unit " + blueprint.name;
	}
}
