using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items;

[TypeId("ccc43623dd9341449b5d07be1dabaa23")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class BlueprintSharedVendorTable : BlueprintScriptableObject
{
	public bool AutoIdentifyAllItems = true;

	public VendorTableSlot[] Slots = new VendorTableSlot[0];
}
