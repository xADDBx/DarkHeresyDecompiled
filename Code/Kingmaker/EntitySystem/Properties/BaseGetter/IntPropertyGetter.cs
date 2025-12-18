namespace Kingmaker.EntitySystem.Properties.BaseGetter;

public abstract class IntPropertyGetter : PropertyGetter
{
	public sealed override bool IsBool => false;

	protected sealed override int GetBaseValueInternal()
	{
		return GetBaseValue();
	}

	protected abstract int GetBaseValue();
}
