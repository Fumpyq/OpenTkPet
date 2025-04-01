using PostSharp.Extensibility;
using Profiling;

[assembly: AutoProfileAllMethods(
    AttributeTargetTypes = "ConsoleApp1_Pet.*",
    AttributeTargetMemberAttributes = MulticastAttributes.AnyVisibility |
                                    MulticastAttributes.NonAbstract,
    AttributePriority = 1)]
