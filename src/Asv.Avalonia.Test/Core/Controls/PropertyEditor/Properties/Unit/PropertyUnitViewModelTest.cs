using Asv.Cfg;
using Asv.Modeling;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class PropertyUnitViewModelTest
{
    [Fact]
    public void Constructor_WithDisplayFormat_KeepsUnroundedEditableText()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1234.567);

        // Act
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");

        // Assert
        Assert.Equal("1234.567", property.Text.Value);
        Assert.Equal("1234.57", property.DisplayText);
        Assert.True(property.IsSync);
    }

    [Fact]
    public void ChangeUnit_WithPendingValue_PreservesNumericPrecision()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1234.567);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");
        property.IsInEditMode = true;
        property.Text.Value = "3704.123456";

        // Act
        property.ChangeUnitCommand.Execute(unit.AvailableUnits[DistanceNauticalMileUnitItem.Id]);

        // Assert
        Assert.Equal(3704.123456, unit.CurrentUnitItem.Value.ParseToSi(property.Text.Value), 8);
        Assert.Equal(1234.567, model.Value);
        Assert.False(property.IsSync);
    }

    [Fact]
    public async Task ApplyFromUser_WithUnchangedText_PreservesModelValue()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1234.567);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");
        property.IsInEditMode = true;

        // Act
        await property.ApplyFromUser(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1234.567, model.Value);
        Assert.Equal("1234.567", property.Text.Value);
        Assert.False(property.IsInEditMode);
        Assert.True(property.IsSync);
    }

    [Fact]
    public void Cancel_WithPendingChanges_RestoresTextAndDisplay()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1234.567);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");
        property.IsInEditMode = true;
        property.Text.Value = "9876.54321";

        // Act
        property.Cancel();

        // Assert
        Assert.Equal("1234.567", property.Text.Value);
        Assert.Equal("1234.57", property.DisplayText);
        Assert.Equal(1234.567, model.Value);
        Assert.True(property.IsSync);
    }

    [Theory]
    [InlineData("-", true)]
    [InlineData("", true)]
    [InlineData("invalid", true)]
    [InlineData("1.234567", false)]
    public async Task ApplyFromUser_WithDraftText_ValidatesWithoutFormattingInput(
        string text,
        bool hasErrors
    )
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1234.567);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");
        property.IsInEditMode = true;
        property.Text.Value = text;

        // Act
        await property.ApplyFromUser(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(text, property.Text.Value);
        Assert.Equal(hasErrors, property.HasValidationErrors);
        Assert.Equal(hasErrors, property.IsInEditMode);
        Assert.Equal(hasErrors ? 1234.567 : 1.234567, model.Value);
        Assert.Equal(hasErrors ? text : "1.23", property.DisplayText);
    }

    [Fact]
    public void ApplyValueFromModel_WhileEditing_PreservesDraft()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1234.567);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");
        property.IsInEditMode = true;
        property.Text.Value = "9876.54321";

        // Act
        model.Value = 3456.789;

        // Assert
        Assert.Equal("9876.54321", property.Text.Value);
        Assert.Equal("9876.54", property.DisplayText);
        Assert.False(property.IsSync);
    }

    [Fact]
    public void ApplyValueFromModel_OutsideEditing_RefreshesDisplayAndNotifiesBinding()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1234.567);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");
        var notified = false;
        property.PropertyChanged += (_, args) =>
            notified |= args.PropertyName == nameof(property.DisplayText);

        // Act
        model.Value = 3456.789;

        // Assert
        Assert.Equal("3456.789", property.Text.Value);
        Assert.Equal("3456.79", property.DisplayText);
        Assert.True(notified);
        Assert.True(property.IsSync);
    }

    [Fact]
    public void ChangeUnit_FromExternalSource_RefreshesDisplayWithoutChangingModel()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(3704.123456);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");

        // Act
        unit.CurrentUnitItem.Value = unit.AvailableUnits[DistanceNauticalMileUnitItem.Id];

        // Assert
        Assert.Equal("2.00", property.DisplayText);
        Assert.Equal(3704.123456, unit.CurrentUnitItem.Value.ParseToSi(property.Text.Value), 8);
        Assert.Equal(3704.123456, model.Value);
        Assert.True(property.IsSync);
    }

    [Fact]
    public async Task ApplyFromUser_CallbackWithoutNotification_ShowsFormattedAppliedValue()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var update = new Subject<double>();
        var savedValue = 1234.567;
        using var property = new PropertyUnitCallback(
            "distance",
            unit,
            _ => ValueTask.FromResult(savedValue),
            (value, _) =>
            {
                savedValue = value;
                return ValueTask.CompletedTask;
            },
            update,
            format: "F2"
        );
        await property.Refresh(TestContext.Current.CancellationToken);
        property.IsInEditMode = true;
        property.Text.Value = "9876.54321";

        // Act
        await property.ApplyFromUser(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(9876.54321, savedValue);
        Assert.Equal("9876.54321", property.Text.Value);
        Assert.Equal("9876.54", property.DisplayText);
        Assert.True(property.IsSync);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ApplyFromUser_WithUnsuccessfulWrite_DoesNotPublishUndo(bool cancelled)
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var update = new Subject<double>();
        var writeCompletion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var property = new PropertyUnitCallback(
            "distance",
            unit,
            _ => ValueTask.FromResult(1.0),
            (_, _) => new ValueTask(writeCompletion.Task),
            update
        );
        await property.Refresh(TestContext.Current.CancellationToken);
        var changes = new List<UndoEvent<IViewModel>>();
        using var subscription = property.Events.Catch<UndoEvent<IViewModel>>(changes.Add);
        property.IsInEditMode = true;
        property.Text.Value = "2";

        // Act
        var apply = property.ApplyFromUser(TestContext.Current.CancellationToken);
        if (cancelled)
        {
            writeCompletion.SetCanceled();
        }
        else
        {
            writeCompletion.SetException(new InvalidOperationException("Write failed."));
        }
        await apply;

        // Assert
        Assert.Empty(changes);
    }

    [Fact]
    public async Task ApplyFromUser_WithPendingWrite_PublishesUndoAfterCompletion()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var update = new Subject<double>();
        var writeCompletion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var property = new PropertyUnitCallback(
            "distance",
            unit,
            _ => ValueTask.FromResult(1.0),
            (_, _) => new ValueTask(writeCompletion.Task),
            update
        );
        await property.Refresh(TestContext.Current.CancellationToken);
        var changes = new List<UndoEvent<IViewModel>>();
        using var subscription = property.Events.Catch<UndoEvent<IViewModel>>(changes.Add);
        property.IsInEditMode = true;
        property.Text.Value = "2";

        // Act
        var apply = property.ApplyFromUser(TestContext.Current.CancellationToken);
        var pendingChangeCount = changes.Count;
        writeCompletion.SetResult();
        await apply;

        // Assert
        Assert.Equal(0, pendingChangeCount);
        var undoEvent = Assert.Single(changes);
        Assert.Equal("Value", undoEvent.ChangeId);
        var change = Assert.IsType<ValueUndoChange<double>>(undoEvent.UndoChange);
        Assert.Equal(1.0, change.OldValue);
        Assert.Equal(2.0, change.NewValue);
    }

    [Fact]
    public async Task ApplyFromUser_WithSynchronousModelUpdate_PreservesOriginalUndoValue()
    {
        // Arrange
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(1.0);
        using var property = new PropertyUnitReactive("distance", unit, model);
        var changes = new List<UndoEvent<IViewModel>>();
        using var subscription = property.Events.Catch<UndoEvent<IViewModel>>(changes.Add);
        property.IsInEditMode = true;
        property.Text.Value = "2";

        // Act
        await property.ApplyFromUser(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2.0, model.Value);
        var undoEvent = Assert.Single(changes);
        var change = Assert.IsType<ValueUndoChange<double>>(undoEvent.UndoChange);
        Assert.Equal(1.0, change.OldValue);
        Assert.Equal(2.0, change.NewValue);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UndoRedo_WithDisplayFormat_PreservesNumericPrecision(bool redo)
    {
        // Arrange
        var change = new ValueUndoChange<double> { OldValue = 1.234567, NewValue = 2.345678 };
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var model = new ReactiveProperty<double>(redo ? change.OldValue : change.NewValue);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");

        // Act
        if (redo)
        {
            await property.Undo["Value"].Redo(change, TestContext.Current.CancellationToken);
        }
        else
        {
            await property.Undo["Value"].Undo(change, TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(redo ? 2.345678 : 1.234567, model.Value);
        Assert.Equal(redo ? "2.35" : "1.23", property.DisplayText);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UndoRedo_WithUnitChange_RestoresUnitAndPreservesValue(bool redo)
    {
        // Arrange
        var change = new ValueUndoChange<string>
        {
            OldValue = DistanceMeterUnitItem.Id,
            NewValue = DistanceNauticalMileUnitItem.Id,
        };
        using var unit = DistanceUnitTestSetup.CreateUnit();
        unit.CurrentUnitItem.Value = unit.AvailableUnits[redo ? change.OldValue : change.NewValue];
        using var model = new ReactiveProperty<double>(3704.123456);
        using var property = new PropertyUnitReactive("distance", unit, model, format: "F2");

        // Act
        if (redo)
        {
            await property.Undo["Unit"].Redo(change, TestContext.Current.CancellationToken);
        }
        else
        {
            await property.Undo["Unit"].Undo(change, TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(
            redo ? change.NewValue : change.OldValue,
            unit.CurrentUnitItem.Value.UnitItemId
        );
        Assert.Equal(3704.123456, model.Value);
        Assert.Equal(3704.123456, unit.CurrentUnitItem.Value.ParseToSi(property.Text.Value), 8);
        Assert.Equal(redo ? "2.00" : "3704.12", property.DisplayText);
        Assert.True(property.IsSync);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UndoRedo_WithCancelledToken_ForwardsCancellationToWrite(bool redo)
    {
        // Arrange
        var change = new ValueUndoChange<double> { OldValue = 1.234567, NewValue = 2.345678 };
        using var unit = DistanceUnitTestSetup.CreateUnit();
        using var update = new Subject<double>();
        var writeWasCancelled = false;
        using var property = new PropertyUnitCallback(
            "distance",
            unit,
            _ => ValueTask.FromResult(redo ? change.OldValue : change.NewValue),
            (_, cancel) =>
            {
                writeWasCancelled = cancel.IsCancellationRequested;
                return ValueTask.CompletedTask;
            },
            update,
            format: "F2"
        );
        await property.Refresh(TestContext.Current.CancellationToken);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken
        );
        cancellation.Cancel();

        // Act
        if (redo)
        {
            await property.Undo["Value"].Redo(change, cancellation.Token);
        }
        else
        {
            await property.Undo["Value"].Undo(change, cancellation.Token);
        }

        // Assert
        Assert.True(writeWasCancelled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_WithTimeUnit_PreservesPrecisionBeyondDefaultFormat(bool composite)
    {
        // Arrange
        using var unit = new TimeSpanUnit(
            new InMemoryConfiguration(),
            [new TimeSpanSecondUnitItem(), new TimeSpanHourMinuteSecondUnitItem()]
        );
        if (composite)
        {
            unit.CurrentUnitItem.Value = unit.AvailableUnits[TimeSpanHourMinuteSecondUnitItem.Id];
        }
        using var model = new ReactiveProperty<double>(composite ? 61.23456789 : 1.23456789);

        // Act
        using var property = new PropertyUnitReactive("time", unit, model, format: "F8");

        // Assert
        Assert.Equal(model.Value, unit.CurrentUnitItem.Value.ParseToSi(property.Text.Value));
        Assert.Equal(composite ? "0:01:1.23456789" : "1.23456789", property.DisplayText);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UndoRedo_WithTimeUnit_PreservesPrecisionBeyondDefaultFormat(bool redo)
    {
        // Arrange
        var change = new ValueUndoChange<double> { OldValue = 1.23456789, NewValue = 2.34567891 };
        using var unit = new TimeSpanUnit(
            new InMemoryConfiguration(),
            [new TimeSpanSecondUnitItem()]
        );
        using var model = new ReactiveProperty<double>(redo ? change.OldValue : change.NewValue);
        using var property = new PropertyUnitReactive("time", unit, model, format: "F8");

        // Act
        if (redo)
        {
            await property.Undo["Value"].Redo(change, TestContext.Current.CancellationToken);
        }
        else
        {
            await property.Undo["Value"].Undo(change, TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Equal(redo ? 2.34567891 : 1.23456789, model.Value);
    }
}
