﻿using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(ProgressBase.Id)]
[Shared]
[method: ImportingConstructor]
public class PercentProgressUnit() : UnitItemBase(100)
{
    public const string Id = $"{ProgressBase.Id}.percent";
    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_Progress_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => true;
}
