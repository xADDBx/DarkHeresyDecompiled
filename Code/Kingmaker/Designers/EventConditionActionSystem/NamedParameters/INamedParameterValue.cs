using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[OwlPackable(OwlPackableMode.Generate)]
public interface INamedParameterValue : IOwlPackable, IOwlPackable<INamedParameterValue>
{
	object Value { get; }
}
