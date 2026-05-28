using System;

namespace OwlPack.Runtime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public class OwlPackIgnoreTypeAttribute : Attribute
{
}
