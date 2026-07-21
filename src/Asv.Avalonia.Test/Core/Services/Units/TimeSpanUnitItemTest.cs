using Asv.Common;
using Xunit;

namespace Asv.Avalonia.Test;

public class TimeSpanUnitItemTest
{
    [Fact]
    public void PrintFromSi_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), 3.456, "3456"),
            (new TimeSpanMillisecondUnitItem(), 1.2345, "1235"),
            (new TimeSpanSecondUnitItem(), 3723.456, "3723.456"),
            (new TimeSpanMinuteUnitItem(), 3720, "62"),
            (new TimeSpanHourUnitItem(), 3600, "1"),
            (new TimeSpanMinuteSecondUnitItem(), 3723.456, "62:03.456"),
            (new TimeSpanHourMinuteUnitItem(), 3723.456, "1:02.0576"),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.456, "1:02:03.456"),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.PrintFromSi(testCase.Value))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void PrintFromSi_EdgeValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Expected)[]
        {
            (new TimeSpanHourMinuteSecondUnitItem(), 0, "0:00:00"),
            (new TimeSpanMinuteSecondUnitItem(), 3.4, "0:03.4"),
            (new TimeSpanMinuteSecondUnitItem(), 62.3, "1:02.3"),
            (new TimeSpanMinuteSecondUnitItem(), 62.999, "1:02.999"),
            (new TimeSpanMinuteSecondUnitItem(), 119.9996, "1:59.9996"),
            (new TimeSpanHourMinuteUnitItem(), -3723.456, "-1:02.0576"),
            (new TimeSpanHourMinuteSecondUnitItem(), -3723.456, "-1:02:03.456"),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.PrintFromSi(testCase.Value))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void ParseToSi_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string Value, double Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), "1234", 1.234),
            (new TimeSpanMillisecondUnitItem(), "1.5k", 1.5),
            (new TimeSpanMillisecondUnitItem(), "1.234k", 1.234),
            (new TimeSpanMillisecondUnitItem(), "0.001k", 0.001),
            (new TimeSpanSecondUnitItem(), "3723.456", 3723.456),
            (new TimeSpanMinuteUnitItem(), "62", 3720),
            (new TimeSpanHourUnitItem(), "1", 3600),
            (new TimeSpanMinuteSecondUnitItem(), "62:03.456", 3723.456),
            (new TimeSpanMinuteSecondUnitItem(), "1.5:30", 120),
            (new TimeSpanHourMinuteUnitItem(), "1.5:30", 7200),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:1.5:30", 3720),
            (new TimeSpanHourMinuteSecondUnitItem(), "1k:30:00", 3_601_800),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:03.456", 3723.456),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.ParseToSi(testCase.Value))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i], 3);
        }
    }

    [Fact]
    public void ParseToSi_EdgeValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string Value, double Expected)[]
        {
            (new TimeSpanMinuteSecondUnitItem(), "-62:03.456", -3723.456),
            (new TimeSpanHourMinuteUnitItem(), "-1:02.0576", -3723.456),
            (new TimeSpanHourMinuteSecondUnitItem(), "-1:02:03.456", -3723.456),
            (new TimeSpanMinuteSecondUnitItem(), " 62 : 03.456 ", 3723.456),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.ParseToSi(testCase.Value))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i], 3);
        }
    }

    [Fact]
    public void Print_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), 1234, "1234"),
            (new TimeSpanMillisecondUnitItem(), 1234.5, "1235"),
            (new TimeSpanSecondUnitItem(), 3723.456, "3723.456"),
            (new TimeSpanMinuteUnitItem(), 62, "62"),
            (new TimeSpanHourUnitItem(), 1, "1"),
            (new TimeSpanMinuteSecondUnitItem(), 3723.456, "62:03.456"),
            (new TimeSpanHourMinuteUnitItem(), 3723.456, "1:02.0576"),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.456, "1:02:03.456"),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.Print(testCase.Value)).ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void PrintWithUnits_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), 1234, $"1234 {RS.Millisecond_UnitItem_Symbol}"),
            (new TimeSpanMillisecondUnitItem(), 1234.5, $"1235 {RS.Millisecond_UnitItem_Symbol}"),
            (new TimeSpanSecondUnitItem(), 3723.456, $"3723.456 {RS.Second_UnitItem_Symbol}"),
            (new TimeSpanMinuteUnitItem(), 62, $"62 {RS.Minute_UnitItem_Symbol}"),
            (new TimeSpanHourUnitItem(), 1, $"1 {RS.Hour_UnitItem_Symbol}"),
            (
                new TimeSpanMinuteSecondUnitItem(),
                3723.456,
                $"62:03.456 {RS.MinuteSecond_UnitItem_Symbol}"
            ),
            (
                new TimeSpanHourMinuteUnitItem(),
                3723.456,
                $"1:02.0576 {RS.HourMinute_UnitItem_Symbol}"
            ),
            (
                new TimeSpanHourMinuteSecondUnitItem(),
                3723.456,
                $"1:02:03.456 {RS.HourMinuteSecond_UnitItem_Symbol}"
            ),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.PrintWithUnits(testCase.Value))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void PrintFromSiWithUnits_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), 3.456, $"3456 {RS.Millisecond_UnitItem_Symbol}"),
            (new TimeSpanMillisecondUnitItem(), 1.2345, $"1235 {RS.Millisecond_UnitItem_Symbol}"),
            (new TimeSpanSecondUnitItem(), 3723.456, $"3723.456 {RS.Second_UnitItem_Symbol}"),
            (new TimeSpanMinuteUnitItem(), 3720, $"62 {RS.Minute_UnitItem_Symbol}"),
            (new TimeSpanHourUnitItem(), 3600, $"1 {RS.Hour_UnitItem_Symbol}"),
            (
                new TimeSpanMinuteSecondUnitItem(),
                3723.456,
                $"62:03.456 {RS.MinuteSecond_UnitItem_Symbol}"
            ),
            (
                new TimeSpanHourMinuteUnitItem(),
                3723.456,
                $"1:02.0576 {RS.HourMinute_UnitItem_Symbol}"
            ),
            (
                new TimeSpanHourMinuteSecondUnitItem(),
                3723.456,
                $"1:02:03.456 {RS.HourMinuteSecond_UnitItem_Symbol}"
            ),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.PrintFromSiWithUnits(testCase.Value))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void Parse_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string Value, double Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), "1234", 1234),
            (new TimeSpanMillisecondUnitItem(), "1.5k", 1500),
            (new TimeSpanSecondUnitItem(), "3723.456", 3723.456),
            (new TimeSpanMinuteUnitItem(), "62", 62),
            (new TimeSpanHourUnitItem(), "1", 1),
            (new TimeSpanMinuteSecondUnitItem(), "62:03.456", 3723.456),
            (new TimeSpanHourMinuteUnitItem(), "1:02.0576", 3723.456),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:03.456", 3723.456),
            (new TimeSpanMinuteSecondUnitItem(), "2166666666:40", 130_000_000_000),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.Parse(testCase.Value)).ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i], 3);
        }
    }

    [Fact]
    public void IsValid_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string? Value)[]
        {
            (new TimeSpanMillisecondUnitItem(), "1234"),
            (new TimeSpanMillisecondUnitItem(), "-1234"),
            (new TimeSpanMillisecondUnitItem(), "1234.0"),
            (new TimeSpanMillisecondUnitItem(), "1.5k"),
            (new TimeSpanMillisecondUnitItem(), "1.234k"),
            (new TimeSpanMillisecondUnitItem(), "0.001k"),
            (new TimeSpanMinuteSecondUnitItem(), "62:03.456"),
            (new TimeSpanMinuteSecondUnitItem(), "1.5:30"),
            (new TimeSpanHourMinuteUnitItem(), "1.5:02"),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:03.456"),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.IsValid(testCase.Value)).ToArray();

        // Assert
        foreach (var isValid in actual)
        {
            Assert.True(isValid);
        }
    }

    [Fact]
    public void IsValid_InvalidValue_Fail()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string? Value)[]
        {
            (new TimeSpanMillisecondUnitItem(), "text"),
            (new TimeSpanMillisecondUnitItem(), null),
            (new TimeSpanMillisecondUnitItem(), "1234.5"),
            (new TimeSpanMillisecondUnitItem(), "1.2345k"),
            (new TimeSpanMinuteSecondUnitItem(), "62"),
            (new TimeSpanMinuteSecondUnitItem(), "62:"),
            (new TimeSpanMinuteSecondUnitItem(), "62:text"),
            (new TimeSpanMinuteSecondUnitItem(), "1:-30"),
            (new TimeSpanMinuteSecondUnitItem(), "1:+30"),
            (new TimeSpanHourMinuteSecondUnitItem(), null),
            (new TimeSpanHourMinuteSecondUnitItem(), "1::03"),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:03:456"),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.IsValid(testCase.Value)).ToArray();

        // Assert
        foreach (var isValid in actual)
        {
            Assert.False(isValid);
        }
    }

    [Fact]
    public void ValidateValue_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string? Value)[]
        {
            (new TimeSpanMillisecondUnitItem(), "1234"),
            (new TimeSpanMillisecondUnitItem(), "1.5k"),
            (new TimeSpanMillisecondUnitItem(), "1.234k"),
            (new TimeSpanMillisecondUnitItem(), "0.001k"),
            (new TimeSpanMinuteSecondUnitItem(), "62:03.456"),
            (new TimeSpanHourMinuteUnitItem(), "1.5:02"),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:03.456"),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.ValidateValue(testCase.Value).IsSuccess)
            .ToArray();

        // Assert
        foreach (var isSuccess in actual)
        {
            Assert.True(isSuccess);
        }
    }

    [Fact]
    public void ValidateValue_InvalidValue_Fail()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string? Value, Type ExpectedExceptionType)[]
        {
            (new TimeSpanMillisecondUnitItem(), "text", typeof(NotNumberValidationException)),
            (
                new TimeSpanMillisecondUnitItem(),
                null,
                typeof(IsNullOrWhiteSpaceValidationException)
            ),
            (new TimeSpanMillisecondUnitItem(), "1234.5", typeof(ValidationException)),
            (new TimeSpanMinuteSecondUnitItem(), "62", typeof(ValidationException)),
            (new TimeSpanMinuteSecondUnitItem(), "62:", typeof(ValidationException)),
            (new TimeSpanMinuteSecondUnitItem(), "62:text", typeof(NotNumberValidationException)),
            (new TimeSpanMinuteSecondUnitItem(), "1:-30", typeof(ValidationException)),
            (
                new TimeSpanHourMinuteSecondUnitItem(),
                null,
                typeof(IsNullOrWhiteSpaceValidationException)
            ),
            (new TimeSpanHourMinuteSecondUnitItem(), "1::03", typeof(ValidationException)),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:03:456", typeof(ValidationException)),
        };

        foreach (var testCase in testCases)
        {
            // Act
            var actual = testCase.Unit.ValidateValue(testCase.Value);

            // Assert
            Assert.False(actual.IsSuccess);
            var exception = Assert.IsType<UnitException>(actual.ValidationException);
            Assert.NotNull(exception.InnerException);
            Assert.Equal(testCase.ExpectedExceptionType, exception.InnerException.GetType());
        }
    }

    [Fact]
    public void FromSi_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, double Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), 1.234, 1234),
            (new TimeSpanSecondUnitItem(), 3723.456, 3723.456),
            (new TimeSpanMinuteUnitItem(), 3720, 62),
            (new TimeSpanHourUnitItem(), 3600, 1),
            (new TimeSpanMinuteSecondUnitItem(), 3723.456, 3723.456),
            (new TimeSpanHourMinuteUnitItem(), 3723.456, 3723.456),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.456, 3723.456),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.FromSi(testCase.Value)).ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i], 3);
        }
    }

    [Fact]
    public void ToSi_ValidValue_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, double Expected)[]
        {
            (new TimeSpanMillisecondUnitItem(), 1234, 1.234),
            (new TimeSpanSecondUnitItem(), 3723.456, 3723.456),
            (new TimeSpanMinuteUnitItem(), 62, 3720),
            (new TimeSpanHourUnitItem(), 1, 3600),
            (new TimeSpanMinuteSecondUnitItem(), 3723.456, 3723.456),
            (new TimeSpanHourMinuteUnitItem(), 3723.456, 3723.456),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.456, 3723.456),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.ToSi(testCase.Value)).ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i], 3);
        }
    }

    [Fact]
    public void PrintParse_RoundTrip_Success()
    {
        // Arrange
        var units = new IUnitItem[]
        {
            new TimeSpanMinuteSecondUnitItem(),
            new TimeSpanHourMinuteUnitItem(),
            new TimeSpanHourMinuteSecondUnitItem(),
        };
        var values = new[] { 0d, 0.5, 1, 3.456, 62, 62.3, 3723.456, 86400, -3723.456, -0.5 };

        foreach (var unit in units)
        {
            foreach (var value in values)
            {
                // Act
                var printed = unit.PrintFromSi(value);
                var parsed = unit.ParseToSi(printed);
                var reprinted = unit.PrintFromSi(parsed);

                // Assert
                Assert.Equal(printed, reprinted);
                Assert.Equal(value, parsed, 3);
            }
        }
    }

    [Fact]
    public void IsValid_SignedNonFirstComponent_Fail()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string Value)[]
        {
            (new TimeSpanMinuteSecondUnitItem(), "1:-30"),
            (new TimeSpanMinuteSecondUnitItem(), "1:+30"),
            (new TimeSpanHourMinuteUnitItem(), "1:-30"),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:-03"),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:+03.456"),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.IsValid(testCase.Value)).ToArray();

        // Assert
        foreach (var isValid in actual)
        {
            Assert.False(isValid);
        }
    }

    [Fact]
    public void ValidateValue_SignedNonFirstComponent_Fail()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string Value)[]
        {
            (new TimeSpanMinuteSecondUnitItem(), "1:-30"),
            (new TimeSpanMinuteSecondUnitItem(), "1:+30"),
            (new TimeSpanHourMinuteUnitItem(), "1:-30"),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:-03"),
            (new TimeSpanHourMinuteSecondUnitItem(), "1:02:+03.456"),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.ValidateValue(testCase.Value).IsSuccess)
            .ToArray();

        // Assert
        foreach (var isSuccess in actual)
        {
            Assert.False(isSuccess);
        }
    }

    [Fact]
    public void Parse_NegativeSignOnlyInFirstComponent_Success()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, string Value, double Expected)[]
        {
            (new TimeSpanMinuteSecondUnitItem(), "-1:30.500", -90.5),
            (new TimeSpanHourMinuteUnitItem(), "-1:30.5", -5430),
            (new TimeSpanHourMinuteSecondUnitItem(), "-1:02:03.456", -3723.456),
        };

        // Act
        var actual = testCases.Select(testCase => testCase.Unit.Parse(testCase.Value)).ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i], 3);
        }
    }

    [Fact]
    public void Print_Format_AppliesToTimeSpanSecondUnitItem()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Format, string Expected)[]
        {
            (new TimeSpanSecondUnitItem(), 3723.456, "0", "3723"),
            (new TimeSpanSecondUnitItem(), 3723.6, "0", "3724"),
            (new TimeSpanSecondUnitItem(), 0, "0", "0"),
            (new TimeSpanSecondUnitItem(), 3.456, "0.00", "3.46"),
            (new TimeSpanSecondUnitItem(), 3.4, "0.000", "3.400"),
            (new TimeSpanSecondUnitItem(), 3.456, "0000.0", "0003.5"),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.Print(testCase.Value, testCase.Format))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void Print_Format_AppliesToLastCompositeComponent()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Format, string Expected)[]
        {
            (new TimeSpanMinuteSecondUnitItem(), 62.3, "00", "1:02"),
            (new TimeSpanMinuteSecondUnitItem(), 62.3456, "00.00", "1:02.35"),
            (new TimeSpanMinuteSecondUnitItem(), 62.3, "00.000", "1:02.300"),
            (new TimeSpanHourMinuteUnitItem(), 3723.456, "00", "1:02"),
            (new TimeSpanHourMinuteUnitItem(), 3723.456, "00.00", "1:02.06"),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.4, "00", "1:02:03"),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.6, "00", "1:02:04"),
            (new TimeSpanHourMinuteSecondUnitItem(), -3723.4, "00", "-1:02:03"),
            (new TimeSpanHourMinuteSecondUnitItem(), 0, "00", "0:00:00"),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.456, "00.00", "1:02:03.46"),
            (new TimeSpanHourMinuteSecondUnitItem(), 3723.4, "00.000", "1:02:03.400"),
            (new TimeSpanHourMinuteSecondUnitItem(), -62.345, "00.00", "-0:01:02.35"),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.Print(testCase.Value, testCase.Format))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void PrintWithUnits_Format_AppliesToPrintedValue()
    {
        // Arrange
        var testCases = new (IUnitItem Unit, double Value, string Format, string Expected)[]
        {
            (new TimeSpanSecondUnitItem(), 3.4, "0.000", $"3.400 {RS.Second_UnitItem_Symbol}"),
            (
                new TimeSpanMinuteSecondUnitItem(),
                62.3,
                "00.000",
                $"1:02.300 {RS.MinuteSecond_UnitItem_Symbol}"
            ),
            (
                new TimeSpanHourMinuteSecondUnitItem(),
                3723.456,
                "00.00",
                $"1:02:03.46 {RS.HourMinuteSecond_UnitItem_Symbol}"
            ),
        };

        // Act
        var actual = testCases
            .Select(testCase => testCase.Unit.PrintWithUnits(testCase.Value, testCase.Format))
            .ToArray();

        // Assert
        for (var i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Expected, actual[i]);
        }
    }

    [Fact]
    public void Constructor_DuplicateComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => new DuplicateComponentsUnit());
    }

    private sealed class DuplicateComponentsUnit : TimeSpanCompositeUnitItemBase
    {
        public DuplicateComponentsUnit()
            : base(TimeSpanComponent.Hours, TimeSpanComponent.Hours) { }

        public override string UnitItemId => "test.dup";
        public override string Name => "dup";
        public override string Description => "dup";
        public override string Symbol => "dup";
        public override bool IsInternationalSystemUnit => false;
    }
}
