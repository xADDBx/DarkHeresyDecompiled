using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;

namespace Kingmaker.Blueprints.Root;

[ComponentName("Root/ConsoleRoot")]
[TypeId("6d29fa7e398a4862b889eaca8eb0c605")]
public class ConsoleRoot : BlueprintScriptableObject
{
	public GamePadIcons Icons;

	public static ConsoleRoot Instance => ConfigRoot.Instance.ConsoleRoot;
}
