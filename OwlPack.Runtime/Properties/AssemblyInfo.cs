using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using OwlPack.Runtime;

[assembly: OwlPackExternalSerializer(typeof(DateTime), typeof(DateTimeSerializer))]
[assembly: OwlPackExternalSerializer(typeof(TimeSpan), typeof(TimeSpanSerializer))]
[assembly: OwlPackExternalSerializer(typeof(Type), typeof(TypeSerializer))]
[assembly: OwlPackExternalSerializer(typeof(Guid), typeof(GuidSerializer))]
[assembly: OwlPackExternalSerializer(typeof(List<>), typeof(ListSerializer<, >))]
[assembly: OwlPackExternalSerializer(typeof(Queue<>), typeof(QueueSerializer<, >))]
[assembly: OwlPackExternalSerializer(typeof(HashSet<>), typeof(HashSetSerializer<, >))]
[assembly: OwlPackExternalSerializer(typeof(Dictionary<, >), typeof(DictionarySerializer<, , >))]
[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("Example_SchemaChanges")]
[assembly: AssemblyCompany("OwlPack.Runtime")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+5a7145d94b3207b720a8e4fb59b7fb684c13d35c")]
[assembly: AssemblyProduct("OwlPack.Runtime")]
[assembly: AssemblyTitle("OwlPack.Runtime")]
[assembly: AssemblyVersion("1.0.0.0")]
