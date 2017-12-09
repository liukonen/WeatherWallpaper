using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "wundergroundSRS")]
    class wundergroundSRS : wundergroundAPIBase, IsharedSunRiseSetInterface
    {
        public override ISharedResponse Invoke()
        {
            base.Call();
            return base.ReturnValue(SharedType.SRS);
        }

    }
}
