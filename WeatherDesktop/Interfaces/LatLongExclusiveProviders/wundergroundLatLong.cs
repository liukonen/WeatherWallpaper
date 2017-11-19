﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeatherDesktop.Interface
{
    class wundergroundLatLong : wundergroundAPIBase, ILatLongInterface
    {
        public wundergroundLatLong()
    {
            // base.Invoke();
            Call();
    }

    public double Latitude()
    {
        return ((LatLongResponse)base.ReturnValue(SharedType.LatLong)).Latitude;
    }

    public double Longitude()
    {
        return ((LatLongResponse)base.ReturnValue(SharedType.LatLong)).Longitude;
    }

    public bool worked()
    {
        return true;

    }
}
}
