using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeatherDesktop.Interface
{
    class wundergroundSRS : wundergroundAPIBase, IsharedSunRiseSetInterface
    {
        public override ISharedResponse Invoke()
        {
            base.Call();
            return base.ReturnValue(SharedType.SRS);
        }

    }
}
