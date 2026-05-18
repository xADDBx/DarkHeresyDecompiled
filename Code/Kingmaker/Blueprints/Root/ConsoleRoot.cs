using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Root;

[ComponentName("Root/ConsoleRoot")]
[TypeId("6d29fa7e398a4862b889eaca8eb0c605")]
public class ConsoleRoot : BlueprintScriptableObject
{
	public static ConsoleRoot Instance => ConfigRoot.Instance.ConsoleRoot;
}
