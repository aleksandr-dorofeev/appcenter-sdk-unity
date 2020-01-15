// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AppCenter.Unity;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
using UnityEngine;
#endif

#if UNITY_2018_1_OR_NEWER
public class AppCenterPreBuild : IPreprocessBuildWithReport
#else
public class AppCenterPreBuild : IPreprocessBuild
#endif
{
    public int callbackOrder { get { return 0; } }

#if UNITY_2018_1_OR_NEWER
    public void OnPreprocessBuild(BuildReport report)
    {
        OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
    }
#endif

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.Android)
        {
            var settings = AppCenterSettingsContext.SettingsInstance;
            if (settings.UseAuth && AppCenter.Auth != null)
            {
                MsalDependency.SetupAuth();
            }
            if (settings.UsePush && AppCenter.Push != null)
            {
                FirebaseDependency.SetupPush();
            }
#if !APPCENTER_DONT_USE_NATIVE_STARTER
            var settingsMaker = new AppCenterSettingsMakerAndroid();
            AddStartupCode(settingsMaker);
            CreateManifest.Create(settings);
            AddSettingsFileToLoader(settingsMaker);
#endif
        }
        else if (target == BuildTarget.iOS)
        {
#if !APPCENTER_DONT_USE_NATIVE_STARTER
            AddStartupCode(new AppCenterSettingsMakerIos());
#endif
        }
    }

#if UNITY_ANDROID
    public static void AddSettingsFileToLoader(AppCenterSettingsMakerAndroid settingsMaker)
    {
        var loaderZipFile = AppCenterSettingsContext.AppCenterPath + "/Plugins/Android/appcenter-loader-release.aar";
        var loaderFolder = "appcenter-loader-release";
        var settingsFilePath = "appcenter-loader-release/res/values/appcenter-settings.xml";
        var settingsMetaFilePath = "appcenter-loader-release/res/values/appcenter-settings.xml.meta";

        if (!File.Exists(loaderZipFile))
        {
            Debug.LogWarning("Failed to load dependency file appcenter-loader-release.aar");
            return;
        }

        // Delete unzipeed directory if it already exists.
        if (Directory.Exists(loaderFolder))
        {
            Directory.Delete(loaderFolder, true);
        }

        AndroidLibraryHelper.UnzipFile(loaderZipFile, loaderFolder);
        if (!Directory.Exists(loaderFolder))
        {
            Debug.LogWarning("Unzipping loader folder failed.");
            return;
        }

        settingsMaker.CommitSettings(settingsFilePath);

        // Delete the AndroidManifest.xml.meta file if generated
        if (File.Exists(settingsMetaFilePath))
        {
            File.Delete(settingsMetaFilePath);
        }

        // Delete the original aar file and zipped the extracted folder to generate a new one.
        File.Delete(loaderZipFile);
        AndroidLibraryHelper.ZipFile(loaderFolder, loaderZipFile);
        Directory.Delete(loaderFolder, true);
    }
#endif

    private void AddStartupCode(IAppCenterSettingsMaker settingsMaker)
    {
        var settings = AppCenterSettingsContext.SettingsInstance;
        var advancedSettings = AppCenterSettingsContext.SettingsInstanceAdvanced;
        settingsMaker.SetAppSecret(settings);
        settingsMaker.SetLogLevel((int)settings.InitialLogLevel);
        if (settings.CustomLogUrl.UseCustomUrl)
        {
            settingsMaker.SetLogUrl(settings.CustomLogUrl.Url);
        }
        if (settings.MaxStorageSize.UseCustomMaxStorageSize && settings.MaxStorageSize.Size > 0)
        {
            settingsMaker.SetMaxStorageSize(settings.MaxStorageSize.Size);
        }
        if (settings.UsePush && settingsMaker.IsPushAvailable())
        {
            settingsMaker.StartPushClass();
            if (settings.EnableFirebaseAnalytics)
            {
                settingsMaker.EnableFirebaseAnalytics();
            }
        }
        if (settings.UseAnalytics && settingsMaker.IsAnalyticsAvailable())
        {
            settingsMaker.StartAnalyticsClass();
        }
        if (settings.UseAuth && settingsMaker.IsAuthAvailable())
        {
            if (settings.CustomAuthConfigUrl.UseCustomUrl)
            {
                settingsMaker.SetAuthConfigUrl(settings.CustomAuthConfigUrl.Url);
            }
            settingsMaker.StartAuthClass();
        }
        if (settings.UseCrashes && settingsMaker.IsCrashesAvailable())
        {
            settingsMaker.StartCrashesClass();
        }
        if (settings.UseDistribute && settingsMaker.IsDistributeAvailable())
        {
            if (settings.CustomApiUrl.UseCustomUrl)
            {
                settingsMaker.SetApiUrl(settings.CustomApiUrl.Url);
            }
            if (settings.CustomInstallUrl.UseCustomUrl)
            {
                settingsMaker.SetInstallUrl(settings.CustomInstallUrl.Url);
            }
            if (settings.EnableDistributeForDebuggableBuild)
            {
                settingsMaker.SetShouldEnableDistributeForDebuggableBuild();
            }
            settingsMaker.StartDistributeClass();
        }
        if (advancedSettings != null)
        {
            var startupType = settingsMaker.IsStartFromAppCenterBehavior(advancedSettings) ? StartupType.Skip : advancedSettings.GetStartupType();
            settingsMaker.SetStartupType((int)startupType);
            settingsMaker.SetTransmissionTargetToken(advancedSettings.TransmissionTargetToken);
        }
        else
        {
            settingsMaker.SetStartupType((int)StartupType.AppCenter);
        }
#if !UNITY_ANDROID
        settingsMaker.CommitSettings();
#endif
    }
}
