using SepeCam.Core;
using SepeCam.Core.Native;
using Xunit;

namespace SepeCam.Core.Tests;

public sealed class CameraPropertyCatalogTests
{
    [Fact]
    public void Catalog_Has18Properties()
    {
        Assert.Equal(18, CameraPropertyInfo.Catalog.Length);
    }

    [Fact]
    public void Catalog_HasUniqueKindIdPairs()
    {
        var seen = new HashSet<(PropertyKind, int)>();
        foreach (var (kind, id, _) in CameraPropertyInfo.Catalog)
        {
            Assert.True(seen.Add((kind, id)), $"Duplicate ({kind}, {id})");
        }
    }

    [Fact]
    public void Catalog_HasUniqueDisplayNames()
    {
        var names = CameraPropertyInfo.Catalog.Select(c => c.name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Theory]
    [InlineData(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness, "Brightness")]
    [InlineData(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Contrast, "Contrast")]
    [InlineData(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.WhiteBalance, "White Balance")]
    [InlineData(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.BacklightCompensation, "Backlight")]
    [InlineData(PropertyKind.CameraControl, (int)CameraControlProperty.Exposure, "Exposure")]
    [InlineData(PropertyKind.CameraControl, (int)CameraControlProperty.Zoom, "Zoom")]
    [InlineData(PropertyKind.CameraControl, (int)CameraControlProperty.Focus, "Focus")]
    public void Catalog_ContainsExpectedEntry(PropertyKind kind, int id, string expectedName)
    {
        var match = CameraPropertyInfo.Catalog.FirstOrDefault(c => c.kind == kind && c.id == id);

        Assert.NotEqual(default, match);
        Assert.Equal(expectedName, match.name);
    }
}
