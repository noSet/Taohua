using System;
using System.Collections.Generic;
using System.Text;

namespace Taohua.Tests.Common
{
    public class MyConverter<TFrom, TTo>
    {
        public TFrom From { get; }

        public TTo To { get; }

        public MyConverter(TFrom from, TTo to)
        {
            From = from;
            To = to;
        }
    }
}
