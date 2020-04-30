using System.Linq;
using UnityEngine;

public static class WebCamUtil
{
    public static string FindName()
    {
        if (Application.isMobilePlatform)
        {
            return WebCamTexture.devices.Where(d => d.isFrontFacing).Last().name;
        }
        return WebCamTexture.devices.Last().name;
    }
}
