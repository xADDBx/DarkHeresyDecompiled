using Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;

public abstract class ControllerBase
{
	public ComponentBase Component { get; private set; }

	public bool Active
	{
		get
		{
			if (Component != null)
			{
				return Component.Active;
			}
			return false;
		}
	}

	public ControllerBase(ComponentBase component)
	{
		Component = component;
	}

	public virtual void Initialize(GlobalEffectContext context)
	{
	}

	public virtual void CleanUp()
	{
	}

	internal virtual void UpdateInternal(GlobalEffectContext context)
	{
		Update(context);
	}

	public abstract void Update(GlobalEffectContext context);
}
public abstract class ControllerBase<T> : ControllerBase where T : ComponentBase
{
	public new T Component { get; private set; }

	public ControllerBase(T component)
		: base(component)
	{
		Component = component;
	}
}
