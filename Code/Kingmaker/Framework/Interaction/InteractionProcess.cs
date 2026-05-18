using System;
using System.Threading.Tasks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Framework.Interaction;

public abstract class InteractionProcess
{
	private sealed class FinishedInteractionProcess : InteractionProcess
	{
		public FinishedInteractionProcess()
			: base(null, null)
		{
			base.IsFinished = true;
		}

		protected override Task RunProcess()
		{
			return Task.CompletedTask;
		}
	}

	public static readonly InteractionProcess Finished = new FinishedInteractionProcess();

	private readonly EntityRef<AbstractUnitEntity> m_User;

	private readonly EntityRef<MapObjectEntity> m_Target;

	public bool IsFinished { get; private set; }

	protected AbstractUnitEntity? User => m_User;

	protected MapObjectEntity? Target => m_Target;

	protected IEntityEventBus EventBus => Target?.EventBus ?? throw new InvalidOperationException();

	protected InteractionProcess(AbstractUnitEntity user, MapObjectEntity target)
	{
		m_User = user;
		m_Target = target;
	}

	protected abstract Task RunProcess();

	public void Run()
	{
		RunInternal();
		if (!IsFinished)
		{
			User?.GetOrCreate<InteractionProcessPart>().Add(this);
		}
	}

	private async void RunInternal()
	{
		try
		{
			await RunProcess();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		finally
		{
			IsFinished = true;
			User?.GetOptional<InteractionProcessPart>()?.Remove(this);
		}
	}
}
