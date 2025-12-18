namespace Kingmaker.EntitySystem.Properties.BaseGetter;

public abstract class BoolPropertyGetter : PropertyGetter
{
	public sealed override bool IsBool => true;

	protected sealed override int GetBaseValueInternal()
	{
		if (!GetBaseValue())
		{
			return 0;
		}
		return 1;
	}

	protected abstract bool GetBaseValue();
}
