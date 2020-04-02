# localize

Simple tool to enable localization with json files as source.

## To start using kli.Localize.Tool

#### First install the nuget package 
`<PackageReference Include="kli.Localize.Tool" Version="<version>" />`

#### Next create your *.json files where u put your localizations
```json
{
    "OnlyInGerman": "Wert",
    "Name": "German"
}
```
Just give your default language a name **without** specifying the culture (e.g. Resources.json) all other languages follow the pattern `<Filename>_<CultureInfo.Name>.json` (e.g. Resources_en-US.json for American or Resources_en.json for English)

![alt text][tree]
![alt text][jsonfiles]


#### Last but not least
Insert an ItemGroup with `<Localizable>Relative path to the json file</Localizable>` in the project file (csproj) into which you have inserted your * .json file.

![alt text][csproj]

Now, if everythings works fine and you can compile your solution you should get a nested `<YourJsonFile>.g.cs` which you now can use in you code.

![alt text][generated]

#### Use it and enjoy ;)

Now import the Namespace where you put your *.json files an use the generated code to access your localizations.

![alt text][using]



[csproj]:docs/csproj.png "Project file"
[tree]:docs/tree.png "Filetree"
[jsonfiles]: docs/jsonfiles.png "Json files with resources"
[generated]: docs/generated.png "Generated source"
[using]: docs/using.png "Use it"

