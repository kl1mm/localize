# Version Changes

### From version 2.0

#### Added `GetString` on nested sections
`GetString(key, cultureInfo)` is now also generated on nested section classes and
resolves the section-prefixed key, so lookups work at every hierarchy level:

```csharp
MyLocale.Sub.GetString("SubText");
MyLocale.Sub.DoublyNested.GetString("Something");
MyLocale.Sub.GetString("DoublyNested::Something");
```

#### BREAKING - New `Localize` element

The json files are no longer specified via `AdditionalFiles` but rather via `Localize` element.
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <PackageReference Include="kli.Localize" Version="2.0.*" />

        <Localize Include="Localizations\Locale_*.json" 
                         NamespaceName="Namespace.Of.Your.Choice"
                         ClassName="MyClassName"
                         NeutralCulture="de" />
    </ItemGroup>
</Project>
```

#### Added new diagnostic
- SGL0004 : Localize element is missing NeutralCulture attribute for files: "\<Localize file that is missing the attribute\>".

**Neutral culture** is no longer assumed but needs to be specified via the `NeutralCulture` attribute.
This also means from now on, every file needs a culture postfix.
So the "neutral culture" file has to be postfixed accordingly (`_<your_neutralculture>.json`).


### From version 1.0

#### BREAKING - Ignore none JSON-String/Object values
All properties that are not string or object will be ignored. 
```json
{
    "Number": 4.2,
    "Bool": true,
    "Null": null,
    "Array": [1,2,3]
}
```

#### [Add Support for Nested Classes #8](https://github.com/kl1mm/localize/issues/8)
It is now possible to use JSON objects in the localization files.
During generation, the structure is mapped as a nested class for access

```json
{
    "SomeText": "some text",
    "Sub": 
    {
        "FileNotFound": "Not found",
        "DivideByZero": "x / zero"
    },
    "UI":{
        "LabelOne": "One",
        "LabelTwo": "Two",
        "Login": {
            "LabelUserName": "User",
            "LabelPassword": "Pass"
        }
    }
}
```
#### Improved Diagnostics
 - SGL0001: InvalidJsonFileFormat - `<JsonReaderException.Message>`
 - SGL0002: InvalidJsonPropertyName - `Json property key must be a valid C# identifier`
 - SGL0003: InvalidJsonTokenType - `Json property value must be an object or a string`

All diagnostics came with LinePostion (linenumber & column)

### From version 0.8

It is now possible to override the namespace and the class/file name that will be generated:
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <PackageReference Include="kli.Localize" Version="0.8.*" />

        <AdditionalFiles Include="Localizations\Locale.json" 
                         NamespaceName="Namespace.Of.Your.Choice"
                         ClassName="MyClassName" />
    </ItemGroup>
</Project>
```
From which the following is generated:
```csharp
namespace Namespace.Of.Your.Choice
{
    ...
    public sealed class MyClassName {
    ...
```
