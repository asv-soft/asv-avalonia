﻿using Asv.Avalonia.Routable;

namespace Asv.Avalonia;

public interface ISupportRefresh : IRoutable
{
    void Refresh();
}