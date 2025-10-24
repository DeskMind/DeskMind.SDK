# DeskMind.SDK

Overview

DeskMind.SDK is the core library used by the DeskMind solution. It contains core types and interfaces used by the Studio and plugin projects, including plugin infrastructure, tool registry, security policy abstractions, and UI integration points.

Included projects

- `DeskMind.Core` - Core SDK library used by plugins and host applications.

Requirements

- .NET SDK 9.0 (for WPF host projects)
- Projects targeting `.NET Standard 2.0` are used for cross-targeted plugin libraries
- Visual Studio 2022+ or `dotnet` CLI

Building

From the workspace root run:

- `dotnet build` to build all projects in the repository

If you only need the SDK library, build the project at:

- `DeskMind.SDK/src/DeskMind.Core/DeskMind.Core.csproj`

Using the SDK

- Add a `ProjectReference` to `DeskMind.Core` in your plugin or host project. Example in a plugin `.csproj`:

  `<ProjectReference Include="..\..\..\DeskMind.SDK\src\DeskMind.Core\DeskMind.Core.csproj" />`

Core concepts

- Plugin system
  - `IPluginFactory`, `PluginConfig`, `PluginMetadata`, and `SecurePluginFactoryBase` define how plugins are discovered, configured and instantiated.
  - Plugins are expected to produce capabilities that are registered in the `ToolRegistry`.

- Tool registry
  - `ToolRegistry` is the central registry used by the host to enumerate and access registered tools provided by plugins.

- Security
  - `ISecurityPolicyProvider` and `SecurityPolicy` provide a pluggable security model for controlling access to tools and operations.

- Host integration points
  - UI services such as `IMessageBoxService` and `IPluginUIProvider` define abstractions for UI interactions the host provides to plugins so the SDK remains UI-agnostic.

Examples and existing plugins (in this workspace)

- `DeskMind.Plugins.WebScraper` - web-scraping plugin using `HtmlAgilityPack` and `Playwright`.
- `DeskMind.Plugin.PythonRunner` - Python runner plugin (Pro).

Contributing

- Follow the repository conventions and create feature branches off `master`.
- Add unit tests for new public behaviors when applicable.

License

- Add a `LICENSE` file to the repository root to indicate licensing terms.

Contact

- For more information about the SDK APIs, inspect the `DeskMind.SDK/src/DeskMind.Core` source tree and the interfaces such as `IPluginFactory`, `ToolRegistry`, and `SecurityPolicy`.
