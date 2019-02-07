using System;
using System.Collections.Generic;

namespace NuciWeb.Processors
{
    public interface IWebProcessor : IDisposable
    {
        string Name { get; }

        IList<string> Tabs { get; }

        IEnumerable<string> DriverWindowTabs { get; }
    }
}