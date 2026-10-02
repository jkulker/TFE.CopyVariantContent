using FluentAssertions;
using NSubstitute;
using TFE.CopyVariantContent.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Xunit;
using static TFE.CopyVariantContent.Tests.TestDoubles;

namespace TFE.CopyVariantContent.Tests;

public class VariantContentCopierTests
{
    private const string DefaultIso = "en-US";
    private const string DanishIso = "da-DK";

    private readonly IContentService _contentService = Substitute.For<IContentService>();
    private readonly ILanguageService _languageService = Substitute.For<ILanguageService>();
    private readonly VariantContentCopier _sut;

    public VariantContentCopierTests()
    {
        // Default world: English (default) + Danish, and saves succeed.
        // Build doubles up front: configuring a substitute inside another .Returns() confuses NSubstitute.
        ILanguage[] languages = [Language(DefaultIso), Language(DanishIso)];
        OperationResult saveResult = SuccessSave();
        _languageService.GetDefaultIsoCodeAsync().Returns(Task.FromResult(DefaultIso));
        _languageService.GetAllAsync().Returns(Task.FromResult<IEnumerable<ILanguage>>(languages));
        _contentService.Save(Arg.Any<IContent>(), Arg.Any<int?>(), Arg.Any<ContentScheduleCollection?>())
            .Returns(saveResult);

        _sut = new VariantContentCopier(_contentService, _languageService, Logger<VariantContentCopier>());
    }

    [Fact]
    public async Task Returns_failure_when_content_not_found()
    {
        // Arrange
        var key = Guid.NewGuid();
        _contentService.GetById(key).Returns((IContent?)null);

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.Success.Should().BeFalse();
        result.VariantsCreated.Should().Be(0);
        result.Error.Should().Be("Content not found");
    }

    [Fact]
    public async Task Invariant_content_type_creates_nothing_and_does_not_save()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Nothing, [DefaultIso], "Home",
            new PropertySpec("title", VariesByCulture: false, "Hello"));
        _contentService.GetById(key).Returns(content);

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.Success.Should().BeTrue();
        result.VariantsCreated.Should().Be(0);
        content.DidNotReceive().SetCultureName(Arg.Any<string>(), Arg.Any<string>());
        _contentService.DidNotReceive().Save(Arg.Any<IContent>(), Arg.Any<int?>(), Arg.Any<ContentScheduleCollection?>());
    }

    [Fact]
    public async Task Missing_culture_creates_variant_and_copies_varying_property()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso], "Home",
            new PropertySpec("title", VariesByCulture: true, "Hello world"));
        _contentService.GetById(key).Returns(content);

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.VariantsCreated.Should().Be(1);
        content.Received(1).SetCultureName("Home", DanishIso);
        content.Received(1).SetValue("title", "Hello world", DanishIso);
        _contentService.Received(1).Save(content, Arg.Any<int?>(), Arg.Any<ContentScheduleCollection?>());
    }

    [Fact]
    public async Task Default_culture_is_never_overwritten()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso], "Home",
            new PropertySpec("title", VariesByCulture: true, "Hello world"));
        _contentService.GetById(key).Returns(content);

        // Act
        await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        content.DidNotReceive().SetCultureName(Arg.Any<string>(), DefaultIso);
        content.DidNotReceive().SetValue("title", Arg.Any<object?>(), DefaultIso);
    }

    [Fact]
    public async Task Existing_culture_is_skipped()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso, DanishIso], "Home",
            new PropertySpec("title", VariesByCulture: true, "Hello world"));
        _contentService.GetById(key).Returns(content);

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.VariantsCreated.Should().Be(0);
        content.DidNotReceive().SetCultureName(Arg.Any<string>(), DanishIso);
        _contentService.DidNotReceive().Save(Arg.Any<IContent>(), Arg.Any<int?>(), Arg.Any<ContentScheduleCollection?>());
    }

    [Fact]
    public async Task Invariant_property_is_not_copied()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso], "Home",
            new PropertySpec("title", VariesByCulture: true, "Varies"),
            new PropertySpec("sku", VariesByCulture: false, "INVARIANT"));
        _contentService.GetById(key).Returns(content);

        // Act
        await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        content.Received(1).SetValue("title", "Varies", DanishIso);
        content.DidNotReceive().SetValue("sku", Arg.Any<object?>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task Null_default_value_is_skipped_but_variant_is_still_created()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso], "Home",
            new PropertySpec("title", VariesByCulture: true, DefaultValue: null));
        _contentService.GetById(key).Returns(content);

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.VariantsCreated.Should().Be(1);
        content.Received(1).SetCultureName("Home", DanishIso);
        content.DidNotReceive().SetValue("title", Arg.Any<object?>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task Property_set_throwing_does_not_abort_the_node()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso], "Home",
            new PropertySpec("bad", VariesByCulture: true, "boom"),
            new PropertySpec("good", VariesByCulture: true, "ok"));
        _contentService.GetById(key).Returns(content);
        content.When(c => c.SetValue("bad", Arg.Any<object?>(), DanishIso))
            .Do(_ => throw new InvalidOperationException("editor blew up"));

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.VariantsCreated.Should().Be(1);
        content.Received(1).SetValue("good", "ok", DanishIso);
        _contentService.Received(1).Save(content, Arg.Any<int?>(), Arg.Any<ContentScheduleCollection?>());
    }

    [Fact]
    public async Task Include_children_false_does_not_query_descendants()
    {
        // Arrange
        var key = Guid.NewGuid();
        IContent content = Content(key, 5, ContentVariation.Culture, [DefaultIso], "Root",
            new PropertySpec("title", VariesByCulture: true, "v"));
        _contentService.GetById(key).Returns(content);

        // Act
        await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        long ignored;
        _contentService.DidNotReceiveWithAnyArgs().GetPagedDescendants(0, 0, 0, out ignored);
    }

    [Fact]
    public async Task Include_children_true_processes_all_descendants()
    {
        // Arrange
        var rootKey = Guid.NewGuid();
        IContent root = Content(rootKey, 5, ContentVariation.Culture, [DefaultIso], "Root",
            new PropertySpec("title", VariesByCulture: true, "RootVal"));
        var childKey = Guid.NewGuid();
        IContent child = Content(childKey, 6, ContentVariation.Culture, [DefaultIso], "Child",
            new PropertySpec("title", VariesByCulture: true, "ChildVal"));
        _contentService.GetById(rootKey).Returns(root);
        long ignored;
        _contentService.GetPagedDescendants(0, 0, 0, out ignored)
            .ReturnsForAnyArgs(call =>
            {
                call[3] = 1L;
                return new[] { child };
            });

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(rootKey, includeChildren: true);

        // Assert
        result.VariantsCreated.Should().Be(2);
        root.Received(1).SetValue("title", "RootVal", DanishIso);
        child.Received(1).SetValue("title", "ChildVal", DanishIso);
    }

    [Fact]
    public async Task No_additional_languages_creates_nothing()
    {
        // Arrange
        ILanguage[] onlyDefault = [Language(DefaultIso)];
        _languageService.GetAllAsync().Returns(Task.FromResult<IEnumerable<ILanguage>>(onlyDefault));
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso], "Home",
            new PropertySpec("title", VariesByCulture: true, "v"));
        _contentService.GetById(key).Returns(content);

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.VariantsCreated.Should().Be(0);
        _contentService.DidNotReceive().Save(Arg.Any<IContent>(), Arg.Any<int?>(), Arg.Any<ContentScheduleCollection?>());
    }

    [Fact]
    public async Task Variants_are_not_counted_when_save_fails()
    {
        // Arrange
        _contentService.Save(Arg.Any<IContent>(), Arg.Any<int?>(), Arg.Any<ContentScheduleCollection?>())
            .Returns(FailedSave());
        var key = Guid.NewGuid();
        IContent content = Content(key, 1, ContentVariation.Culture, [DefaultIso], "Home",
            new PropertySpec("title", VariesByCulture: true, "v"));
        _contentService.GetById(key).Returns(content);

        // Act
        VariantCopyResult result = await _sut.CopyToMissingVariantsAsync(key, includeChildren: false);

        // Assert
        result.Success.Should().BeTrue();
        result.VariantsCreated.Should().Be(0);
    }
}
