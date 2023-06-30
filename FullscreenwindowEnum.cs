using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1
{
    public static class FullscreenwindowEnum
    {
        public static Fullscreenwindow? GetStatusFromString(string fullscreenwindowString)
        {
            switch (fullscreenwindowString.ToLower())
            {
                case "approved":
                    return Fullscreenwindow.Approved;
                case "rejected":
                    return Fullscreenwindow.Rejected;
                case "canceled":
                    return Fullscreenwindow.Canceled;
                
            }

            return null;
        }
    }

    public enum Fullscreenwindow
    {
        Approved = 1,
        Rejected = 2,
        Canceled = 3
    }
}
