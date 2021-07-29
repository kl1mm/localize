# localize
Simple package with which json files can be used to localize text via static source code access.

Implemented via [C# source generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)

## install the nuget package
Add the following Nuget package reference to the project file in which you want to localize:<br>

`<PackageReference Include="kli.Localize" Version="<version>" />`

## create your own *.json files where you want to put your localized texts.
```json
{
    "SampleText": "FooBar",
    "Other": "Text42"
}
```
Just give your default language a name **without** specifying the culture (e.g. Locale.json) all other languages follow the pattern `<Filename>_<CultureInfo.Name>.json` (e.g. Locale_en-US.json for American English or Locale_en.json for English)

![locale_files image][locale_files]

## add json files to csproj
Add an `ItemGroup` to your project file (csproj). Foreach json file add an `AdditionFiles`-Element with the `Include` attribute set to the path of the file.

![csproj image][csproj]

## use it
Now, if everythings works fine you should be able to locate the generated source code in you Solution-Explorer under Dependencies/Analysers.<br>
Of course you can also view and even debug the generated source code.<br>
![generated_2 image][generated_2]
![generated_1 image][generated_1]
<br>

Import the namespace where you put your *.json files an use the generated code to access your localizations.<br><br>
![useit image][useit]

[locale_files]: docs/locale_files.png
[csproj]: docs/csproj.png
[generated_1]: docs/generated_1.png
[generated_2]: docs/generated_2.png
[useit]: docs/useit.png