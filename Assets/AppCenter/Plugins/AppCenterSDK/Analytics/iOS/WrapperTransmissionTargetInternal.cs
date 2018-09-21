// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

#if UNITY_IOS && !UNITY_EDITOR
using AOT;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.AppCenter.Unity.Analytics.Internal
{
    public class WrapperTransmissionTargetInternal
    {

        public static void TrackEvent(IntPtr transmissionTarget, string eventName)
        {
            appcenter_unity_transmission_target_track_event(transmissionTarget, eventName);
        }

        public static void TrackEventWithProperties(IntPtr transmissionTarget, string eventName, IDictionary<string, string> properties)
        {
            appcenter_unity_transmission_target_track_event_with_props(transmissionTarget, eventName, properties.Keys.ToArray(), properties.Values.ToArray(), properties.Count);
        }

        public static AppCenterTask SetEnabledAsync(IntPtr transmissionTarget, bool enabled)
        {
            appcenter_unity_transmission_target_set_enabled(transmissionTarget, enabled);
            return AppCenterTask.FromCompleted();
        }

        public static AppCenterTask<bool> IsEnabledAsync(IntPtr transmissionTarget)
        {
            bool isEnabled = appcenter_unity_transmission_target_is_enabled(transmissionTarget);
            return AppCenterTask<bool>.FromCompleted(isEnabled);
        }

        public static IntPtr GetTransmissionTarget (IntPtr transmissionTargetParent, string transmissionTargetToken) 
        {
            return appcenter_unity_transmission_transmission_target_for_token(transmissionTargetParent, transmissionTargetToken);
        }

#region External

        [DllImport("__Internal")]
        private static extern void appcenter_unity_transmission_target_track_event(IntPtr transmissionTarget, string eventName);

        [DllImport("__Internal")]
        private static extern void appcenter_unity_transmission_target_track_event_with_props(IntPtr transmissionTarget, string eventName, string[] keys, string[] values, int count);
        
        [DllImport("__Internal")]
        private static extern void appcenter_unity_transmission_target_set_enabled(IntPtr transmissionTarget, bool enabled);

        [DllImport("__Internal")]
        private static extern bool appcenter_unity_transmission_target_is_enabled(IntPtr transmissionTarget);

        [DllImport("__Internal")]
        private static extern IntPtr appcenter_unity_transmission_transmission_target_for_token(IntPtr transmissionTargetParent, string transmissionTargetToken);

#endregion
    }
}
#endif