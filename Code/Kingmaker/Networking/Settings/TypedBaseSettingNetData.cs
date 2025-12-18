using Kingmaker.Settings.Entities;
using MemoryPack;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackableOptIn]
public abstract class TypedBaseSettingNetData<T> : BaseSettingNetData
{
	[JsonProperty]
	[MemoryPackInclude]
	[MemoryPackOrder(0)]
	protected byte Index { get; set; }

	[JsonProperty]
	[MemoryPackInclude]
	[MemoryPackOrder(1)]
	protected T Value { get; set; }

	[JsonConstructor]
	[MemoryPackConstructor]
	protected TypedBaseSettingNetData()
	{
	}

	protected TypedBaseSettingNetData(byte index, T value)
	{
		Index = index;
		Value = value;
	}

	public override void ForceSet()
	{
		((SettingsEntity<T>)PhotonManager.Settings.SettingsForSync[Index]).SetValueAndConfirm(Value);
	}
}
