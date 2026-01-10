# ConfigWJb — Multi‑Variant Configuration Demo

This project demonstrates **four ways** to configure WJb using different input sources. You can switch between configuration variants using an MSBuild property `ConfigVariant`.

The project uses the following file naming pattern:

- `Program.Config1WJb.cs` — Actions embedded in `appsettings.json`
- `Program.Config2WJb.cs` — Actions loaded from `actions.json`
- `Program.Config3WJb.cs` — Code‑first actions (no JSON)
- `Program.Config4WJb.cs` — WJb settings from `appsettings.json`, actions from `actions.json`

Only **one** of these files is compiled at a time.

---

## 🔧 Switching Between Variants (MSBuild)

The project `.csproj` contains this block:

```Xml
<!-- Default variant (you can change to Config2WJb, Config3WJb, or Config4WJb) -->
<PropertyGroup>
  <ConfigVariant>Config1WJb</ConfigVariant>
</PropertyGroup>

<!-- Exclude all Program.Config*.cs, then include only the selected variant -->
<ItemGroup>
  <Compile Remove="Program.Config*.cs" />
  <Compile Include="Program.$(ConfigVariant).cs" />
</ItemGroup>

<ItemGroup>
  <Content Include="actions.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### ▶️ Running a specific config

You can override the variant at runtime:

```Shell
dotnet run -p:ConfigVariant=Config1WJb
dotnet run -p:ConfigVariant=Config2WJb
dotnet run -p:ConfigVariant=Config3WJb
dotnet run -p:ConfigVariant=Config4WJb
```

If you run without parameters:

```Shell
dotnet run
```

it uses the default value defined in the `.csproj`:

```Xml
<ConfigVariant>Config4WJb</ConfigVariant>
```

---

## 📂 Included Files

- **appsettings.json** — For Config1 and Config4 (Config4 loads WJb settings).
- **actions.json** — For Config2 and Config4.
- `Program.Config*.cs` — Each contains a fully self‑contained minimal host.

---

## 🧪 What Each Variant Demonstrates

### Config1WJb — Actions in appsettings.json
- Loads `WJb:Actions` directly from `appsettings.json`.
- No WJb settings are used in this variant.

### Config2WJb — Actions from actions.json
- Loads and deserializes actions from external file.
- No WJb settings.

### Config3WJb — Code‑first configuration
- Builds the action dictionary entirely in C#.
- No settings or external JSON.

### Config4WJb — Settings + Actions
- Loads **WJb settings** from `appsettings.json`.
- Loads **actions** from `actions.json`.
- Uses the `configureSettings` callback of `AddWJbBase` to populate runtime settings.

---

## 📌 Expected Output Example

Example console output from any config running `MyAction`:

```
Hello Oleksandr!
Hello Viktor!
```
