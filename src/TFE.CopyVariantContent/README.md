# TFE.CopyVariantContent (Umbraco 17 port)

Create language variants from existing content, without copy/pasting. This is a port of
[jkulker/TFE.CopyVariantContent](https://github.com/jkulker/TFE.CopyVariantContent) (Umbraco 10-13,
AngularJS) to the Umbraco 17 backoffice.

The editor right-clicks a content node (or uses the `...` actions menu), chooses **Create variants**,
optionally toggles **Include all items below**, and confirms. Every language variant that does not
exist yet is created by copying the default-culture values.

## What changed from the original

| Concern | Original (v10-13) | This port (v17) |
|---|---|---|
| Target framework | `net6.0` | `net10.0` |
| UI | AngularJS controller + view + `package.manifest` | Lit web component (entity action + modal) + `umbraco-package.json` |
| Menu hook | `MenuRenderingNotification` handler | `entityAction` for the `document` entity |
| API | `UmbracoAuthorizedApiController` | Backoffice Management API controller (`BackOfficeRoute` + dedicated Swagger doc) |
| Language listing | `ILocalizationService` (obsolete in v17) | `ILanguageService` |
| Copy logic | First published value into all cultures | Default-culture value into each missing, culture-varying property |
| "Include children" scope | Direct children only (`GetPagedChildren`) | All descendants (`GetPagedDescendants`), matching the label |

The copy logic reads from the default culture, never overwrites the default culture, skips
already-created variants, skips invariant content and invariant properties, copies all missing
cultures for a node in a single save (checking the result), and logs (instead of swallowing)
per-property failures so one bad property cannot abort the node.

## Project layout

```
TFE.CopyVariantContent.csproj   Razor class library (net10.0), the deployable package
Constants.cs                    API name -> route prefix + Swagger doc
Controllers/                    Backoffice Management API controller
Composers/                      DI registration + dedicated Swagger document
Models/                         Request/response DTOs
Services/                       VariantContentCopier (IContentService + ILanguageService)
wwwroot/App_Plugins/CopyVariantContent/   Built client bundle (output, do not edit)
Client/                         TypeScript/Lit source (entity action, modal, data source)
```

## Build

Server:

```sh
dotnet build
```

Client (outputs to `wwwroot/App_Plugins/CopyVariantContent`):

```sh
cd Client
npm install
npm run build      # or: npm run watch  (rebuild on change)
```

Both are verified building against Umbraco 17.4.2 / .NET 10 / Node 22.

## Tests

The `../TFE.CopyVariantContent.Tests` project (xUnit + NSubstitute) unit-tests the copier:

```sh
dotnet test ../TFE.CopyVariantContent.Tests
```

It covers content-not-found, invariant content types, missing vs existing cultures, the default
culture never being overwritten, invariant and null-valued properties, a property setter throwing,
the include-children recursion, no additional languages, and a failed save. The behaviour was also
verified end to end on a live Umbraco 17 site (entity action, modal, HTTP round-trip returning 200,
unauthenticated calls rejected with 401, multi-language and descendant copying, and idempotency).

## Run / test against an Umbraco 17 site

To click through it locally, reference it from any v17 site, for example a throwaway one:

```sh
dotnet new install Umbraco.Templates@17.*
dotnet new umbraco -n CopyVariantTestSite
dotnet add CopyVariantTestSite reference ../TFE.CopyVariantContent/TFE.CopyVariantContent.csproj
dotnet run --project CopyVariantTestSite
```

Build the client first so `wwwroot/App_Plugins/CopyVariantContent` exists; the project reference
copies the static assets into the host. Create a culture-varying document type with a couple of
languages to exercise it.
