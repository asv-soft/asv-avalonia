using Asv.Avalonia.IO;
using Asv.IO;
using Xunit;
using IoRs = Asv.Avalonia.IO.RS;

namespace Asv.Avalonia.Test.IO;

public class PacketFieldDetailsTest
{
    [Fact]
    public void Markdown_ContainsVisitorDefinitionRangeFormatAndCapturedValue()
    {
        var type = new DoubleType(-20, 150, 0);
        var definition = new Field.Builder().Name("Temperature").Title("Sensor temperature")
            .Description("Measured **sensor** temperature.").Units("°C").FormatString("F1")
            .Metadata("readOnly", true).DataType(type).Build();
        var snapshot = new PacketFieldSnapshot("Temperature", type.Name, "21.5", definition.GetUnits(), definition.GetDescription(), [])
        {
            Definition = definition,
            DataType = type,
        };

        var markdown = snapshot.CreateDetailsMarkdown("Sample.Sensors.[1].Temperature");

        Assert.Contains("# Sensor temperature", markdown);
        Assert.Contains("Measured **sensor** temperature.", markdown);
        Assert.Contains("Sample.Sensors.\\[1\\].Temperature", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldType}:** double", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldValue}:** 21.5", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldUnits}:** °C", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldFormat}:** F1", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldMinimum}:** -20", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldMaximum}:** 150", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldReadOnly}:** {IoRs.PacketInspector_FieldYes}", markdown);
    }

    [Fact]
    public void Markdown_UsesActualElementTypeInsteadOfParentCollectionType()
    {
        var elementType = new UInt8Type(2, 99, 2);
        var definition = new Field.Builder().Name("Samples").DataType(new ListType(elementType, 0, 10)).Build();
        var snapshot = new PacketFieldSnapshot("[0]", elementType.Name, "42", null, null, [])
        {
            Definition = definition,
            DataType = elementType,
        };

        var markdown = snapshot.CreateDetailsMarkdown("Samples.[0]");

        Assert.Contains($"{IoRs.PacketInspector_FieldType}:** uint8", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldMinimum}:** 2", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldMaximum}:** 99", markdown);
        Assert.DoesNotContain(IoRs.PacketInspector_FieldMaximumSize, markdown);
    }

    [Fact]
    public void Markdown_ContainsEnumerationNamesValuesAndDescriptions()
    {
        var definition = new Field.Builder().Name("State").DataType(UInt8Type.Default)
            .Enum(new EnumValue<byte>(1, "Ready", "Receiver is ready"), new EnumValue<byte>(2, "Busy", "Processing data")).Build();
        var snapshot = new PacketFieldSnapshot("State", "uint8", "1", null, null, [])
        {
            Definition = definition,
            DataType = UInt8Type.Default,
        };

        var markdown = snapshot.CreateDetailsMarkdown("State");

        Assert.Contains("1 — Ready: Receiver is ready", markdown);
        Assert.Contains("2 — Busy: Processing data", markdown);
        Assert.DoesNotContain("ImmutableDictionary", markdown);
    }

    [Fact]
    public void Markdown_ContainsStringConstraintsAndEscapesCapturedText()
    {
        var type = new StringOptionalType(EncodingId.Ascii, 1, 12, "AB*");
        var snapshot = new PacketFieldSnapshot("Label", type.Name, "[icon=Close;] **text**", null, null, []) { DataType = type };

        var markdown = snapshot.CreateDetailsMarkdown("Label");

        Assert.Contains($"{IoRs.PacketInspector_FieldEncoding}:** Ascii", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldMinimumSize}:** 1", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldMaximumSize}:** 12", markdown);
        Assert.Contains("AB\\*", markdown);
        Assert.Contains("\\[icon=Close;\\] \\*\\*text\\*\\*", markdown);
    }

    [Fact]
    public void Markdown_CollectionIncludesElementTypeLengthAndMetadataWithoutDefinitionFallbackErrors()
    {
        var snapshot = new PacketFieldSnapshot("Readings", "array", string.Empty, null, null,
            [new PacketFieldSnapshot("[0]", "int16", "7", null, null, [])])
        {
            DataType = new ArrayType(Int16Type.Default, 1),
        };

        var markdown = snapshot.CreateDetailsMarkdown("Readings");

        Assert.Contains($"{IoRs.PacketInspector_FieldElementType}:** int16", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldSize}:** 1", markdown);
        Assert.Contains($"{IoRs.PacketInspector_FieldElements}:** 1", markdown);
    }
}
