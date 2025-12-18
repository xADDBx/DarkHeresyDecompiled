using Newtonsoft.Json;
using Owlcat.Bugreport;

namespace Kingmaker.Utility;

[JsonObject]
public class WHBugReportParameters : BugreportParameters
{
	[JsonProperty]
	public string BuildDateTime { get; set; }

	[JsonProperty]
	public bool IsSendMarketingMats { get; set; }

	[JsonProperty]
	public string PlayerLanguage { get; set; }

	[JsonProperty]
	public string UniqueIdentifier { get; set; }

	[JsonProperty]
	public string Blueprint { get; set; }

	[JsonProperty]
	public string BlueprintArea { get; set; }

	[JsonProperty]
	public string Chapter { get; set; }

	[JsonProperty]
	public string StaticScene { get; set; }

	[JsonProperty]
	public string LightScene { get; set; }

	[JsonProperty]
	public string MainCharacter { get; set; }

	[JsonProperty]
	public string CurrentDialog { get; set; }

	[JsonProperty]
	public string AreaDesigner { get; set; }

	[JsonProperty]
	public string ExtendedContext { get; set; }

	[JsonProperty]
	public string PartyContext { get; set; }

	[JsonProperty]
	public string OtherContext { get; set; }

	[JsonProperty]
	public string Cutscenes { get; set; }

	[JsonProperty]
	public string ModifiedSaveFiles { get; set; }

	[JsonProperty]
	public string ControllerModeType { get; set; }

	[JsonProperty]
	public string Platform { get; set; }

	[JsonProperty]
	public string ConsoleHardwareType { get; set; }

	[JsonProperty]
	public string Exception { get; set; }

	[JsonProperty]
	public string ModManagerMods { get; set; }

	[JsonProperty]
	public string CoopPlayersCount { get; set; }

	[JsonProperty]
	public string CameraPosition { get; set; }
}
