using System;
using System.Collections.Generic;
using System.Text;

namespace Taohua.Tests.Common
{
    public class ManyCtor
    {
        public Boo Boo { get; }

        public Foo Foo { get; }

        public ManyCtor(Foo foo)
        {
            Foo = foo;
        }

        public ManyCtor(Boo boo)
        {
            Boo = boo;
        }

        public ManyCtor(Foo foo, Boo boo)
        {
            Foo = foo;
            Boo = boo;
        }
    }

    public class ManyCtor2
    {
        public Foo Foo { get; }
        public Boo Boo { get; }
        public Goo Goo { get; }

        public ManyCtor2(Foo foo)
        {
            Foo = foo;
        }


        public ManyCtor2(Boo boo)
        {
            Boo = boo;
        }

        public ManyCtor2(Goo goo)
        {
            Goo = goo;
        }

        public ManyCtor2(Foo foo, Boo boo)
        {
            Foo = foo;
            Boo = boo;
        }
    }
}
