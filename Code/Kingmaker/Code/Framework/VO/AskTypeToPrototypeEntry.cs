using System;
using Kingmaker.Blueprints;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
public class AskTypeToPrototypeEntry
{
	public AskComponentType AskComponentType;

	public BlueprintUnitAsksListReference Prototype;
}
