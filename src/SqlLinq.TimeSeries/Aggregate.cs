using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlLinq.TimeSeries
{
    delegate object Aggregate<T>(IReadOnlyList<(TimeSpan Time, T Item)> rows, TimeSpan window);
}
