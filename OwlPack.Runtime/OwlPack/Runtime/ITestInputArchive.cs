using System;

namespace OwlPack.Runtime;

internal interface ITestInputArchive
{
	Action<TypeLibrary> OnTypeLibraryDeserialize { get; set; }
}
