namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public interface IEntityInfoProvider<in TValue>
{
	bool TryGetEntityInfo(TValue value, out IEntityInfo entityInfo);
}
