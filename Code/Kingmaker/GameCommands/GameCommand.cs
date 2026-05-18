using System;
using CodeGenerators.MemoryPackUnionGenerator;
using Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;
using MemoryPack;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackDynamicUnion]
public abstract class GameCommand : IOwlPackable, IOwlPackable<GameCommand>
{
	[MemoryPackIgnore]
	public virtual bool IsSynchronized => false;

	[MemoryPackIgnore]
	public virtual bool IsForcedSynced => false;

	public void Execute()
	{
		try
		{
			ExecuteInternal();
		}
		catch (Exception ex)
		{
			PFLog.System.Exception(ex, "Failed to execute game command " + GetType().Name);
		}
	}

	[ValidCommitPoint]
	protected abstract void ExecuteInternal();

	public virtual void AfterDeserialization()
	{
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
