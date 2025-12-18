using System;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class GameCommand : IOwlPackable, IOwlPackable<GameCommand>
{
	public virtual bool IsSynchronized => false;

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

	protected abstract void ExecuteInternal();

	public virtual void AfterDeserialization()
	{
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
