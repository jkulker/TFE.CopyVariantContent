using Microsoft.Extensions.Logging;
using NSubstitute;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace TFE.CopyVariantContent.Tests;

/// <summary>
/// Factory helpers that build NSubstitute doubles for the Umbraco models the copier touches.
/// </summary>
internal static class TestDoubles
{
    public sealed record PropertySpec(string Alias, bool VariesByCulture, object? DefaultValue);

    public static ILogger<T> Logger<T>() => Substitute.For<ILogger<T>>();

    public static ILanguage Language(string isoCode)
    {
        var language = Substitute.For<ILanguage>();
        language.IsoCode.Returns(isoCode);
        return language;
    }

    public static OperationResult SuccessSave() => new(OperationResultType.Success, new EventMessages());

    public static OperationResult FailedSave() => new(OperationResultType.Failed, new EventMessages());

    public static IContent Content(
        Guid key,
        int id,
        ContentVariation typeVariation,
        string[] availableCultures,
        string? defaultCultureName,
        params PropertySpec[] properties)
    {
        var contentType = Substitute.For<ISimpleContentType>();
        contentType.Variations.Returns(typeVariation);

        var content = Substitute.For<IContent>();
        content.Key.Returns(key);
        content.Id.Returns(id);
        content.Name.Returns(defaultCultureName);
        content.ContentType.Returns(contentType);
        content.AvailableCultures.Returns(availableCultures);
        content.GetCultureName(Arg.Any<string>()).Returns(defaultCultureName);

        var propertyList = new List<IProperty>();
        foreach (PropertySpec spec in properties)
        {
            var propertyType = Substitute.For<IPropertyType>();
            propertyType.Variations.Returns(spec.VariesByCulture ? ContentVariation.Culture : ContentVariation.Nothing);

            var property = Substitute.For<IProperty>();
            property.PropertyType.Returns(propertyType);
            property.Alias.Returns(spec.Alias);
            property.GetValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(spec.DefaultValue);

            propertyList.Add(property);
        }

        var propertyCollection = Substitute.For<IPropertyCollection>();
        propertyCollection.GetEnumerator().Returns(_ => propertyList.GetEnumerator());
        content.Properties.Returns(propertyCollection);

        return content;
    }
}
