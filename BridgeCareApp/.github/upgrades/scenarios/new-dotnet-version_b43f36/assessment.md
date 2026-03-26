# Assessment Update

## Accepted direction

The accepted direction for `AssetFox.CalculateEvaluate/AssetFox.CalculateEvaluate.csproj` is:

- change `TargetFramework` from `net6.0` to `net8.0`
- remove explicit package references to `System.Net.Http` and `System.Text.RegularExpressions` because their functionality is provided by the target framework on newer .NET
- keep `Antlr4.Runtime.Standard` under observation as the main package-level compatibility risk during validation

## Current assessment

`AssetFox.CalculateEvaluate` appears to be a low-risk candidate for a `.NET 8` upgrade.

### Evidence gathered

- Upgrade analysis for `AssetFox.CalculateEvaluate` reported:
  - `Project.0002` - target framework needs to be changed
  - `NuGet.0003` - package functionality is included with framework reference
  - `Api.0002` - source incompatible for selected .NET version
- The project currently references:
  - `Antlr4.Runtime.Standard` `4.8.0`
  - `System.Net.Http` `4.3.4`
  - `System.Text.RegularExpressions` `4.3.1`
- Referenced projects are already compatible consumers for a `net8.0` project:
  - `AssetFox.Choice` targets `netstandard2.0`
  - `AssetFox.Comparison` targets `netstandard2.0`
  - `AssetFox` targets `netstandard2.1`
- `AssetFox.CalculateEvaluate.Testing` already targets `net8.0` and references `AssetFox.CalculateEvaluate`
- Validation run completed successfully:
  - solution build succeeded
  - `AssetFox.CalculateEvaluate.Testing` passed `34/34` tests on `.NET 8`

## Risk notes

- The analyzer reported a possible source-level incompatibility for `.NET 8`, but the available tooling did not provide a specific source location.
- Given the passing `.NET 8` test run, any remaining compatibility issue is expected to be small and should be caught during the first rebuild after the target framework change.
- `Antlr4.Runtime.Standard` `4.8.0` is old. If runtime or compile issues appear after the target framework update, this package should be reviewed first.

## Recommended implementation order

1. Update `TargetFramework` to `net8.0`
2. Remove `System.Net.Http`
3. Remove `System.Text.RegularExpressions`
4. Rebuild the project and rerun `AssetFox.CalculateEvaluate.Testing`
5. Revisit `Antlr4.Runtime.Standard` only if validation shows issues
