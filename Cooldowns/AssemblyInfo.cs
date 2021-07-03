using System.Runtime.Versioning;
using System.Windows;

// This is to prevent CA1416 warnings/errors as described here : https://github.com/dotnet/sdk/issues/14502
[assembly: SupportedOSPlatform("windows7.0")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page,
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page,
    // app, or any theme specific resource dictionaries)
)]