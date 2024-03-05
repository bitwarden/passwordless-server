namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

public class Export
{
    public ExportCsv Csv { get; } = new();

    public ExportSvg Svg { get; } = new();

    public ExportPng Png { get; } = new();


    /// <summary>
    /// Sets the file name for all export types.
    /// </summary>
    /// <param name="filename">File name without file extension.</param>
    public void SetFilename(string filename)
    {
        Csv.Filename = filename;
        Svg.Filename = filename;
        Png.Filename = filename;
    }
}

public abstract class BaseExport
{
    public string? Filename { get; set; }
}

public class ExportCsv : BaseExport;

public class ExportSvg : BaseExport;

public class ExportPng : BaseExport;