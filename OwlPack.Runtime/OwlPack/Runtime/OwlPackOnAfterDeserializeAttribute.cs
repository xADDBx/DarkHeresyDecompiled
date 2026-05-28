using System;

namespace OwlPack.Runtime;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class OwlPackOnAfterDeserializeAttribute : Attribute
{
}
