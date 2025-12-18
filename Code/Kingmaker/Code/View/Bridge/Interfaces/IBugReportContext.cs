using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Interfaces;

public interface IBugReportContext
{
	string GetInterfaceName();

	string GetTooltipData(TooltipData tooltipData);

	string GetUIBlueprintName(MonoBehaviour parent);

	BlueprintScriptableObject GetBlueprint(MonoBehaviour parent);

	string GetUIContext(GameObject parent);

	Dictionary<string, string> GetVMNameToContext();
}
