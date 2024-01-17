﻿using Hostess.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TableCloth;
using TableCloth.Resources;

namespace Hostess.Components.Implementations
{
    public sealed class StepsComposer : IStepsComposer
    {
        public StepsComposer(
            ICommandLineArguments commandLineArguments,
            IResourceCacheManager resourceCacheManager,
            ISharedLocations sharedLocations,
            ICriticalServiceProtector criticalServiceProtector,
            IAppMessageBox appMessageBox)
        {
            _commandLineArguments = commandLineArguments;
            _resourceCacheManager = resourceCacheManager;
            _sharedLocations = sharedLocations;
            _criticalServiceProtector = criticalServiceProtector;
            _appMessageBox = appMessageBox;
        }

        private readonly ICommandLineArguments _commandLineArguments;
        private readonly IResourceCacheManager _resourceCacheManager;
        private readonly ISharedLocations _sharedLocations;
        private readonly ICriticalServiceProtector _criticalServiceProtector;
        private readonly IAppMessageBox _appMessageBox;

        private readonly string[] _validAccountNames = new string[]
        {
            "WDAGUtilityAccount",
        };

        public IEnumerable<InstallItemViewModel> ComposeSteps()
        {
            var parsedArgs = _commandLineArguments.Current;
            var catalog = _resourceCacheManager.CatalogDocument;
            var targets = parsedArgs.SelectedServices;

            var packages = new List<InstallItemViewModel>
            {
                new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.CustomAction,
                    TargetSiteName = UIStringResources.Option_Prerequisites,
                    PackageName = UIStringResources.Install_VerifyEnvironment,
                    CustomAction = VerifyWindowsContainerEnvironmentAsync,
                },
                new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.CustomAction,
                    TargetSiteName = UIStringResources.Option_Prerequisites,
                    PackageName = UIStringResources.Install_PrepareEnvironment,
                    CustomAction = PrepareDirectoriesAsync,
                },
                new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.CustomAction,
                    TargetSiteName = UIStringResources.Option_Prerequisites,
                    PackageName = UIStringResources.Install_TryProtectCriticalServices,
                    CustomAction = TryProtectCriticalServicesAsync,
                },
                new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.CustomAction,
                    TargetSiteName = UIStringResources.Option_Prerequisites,
                    PackageName = UIStringResources.Install_SetDesktopWallpaper,
                    CustomAction = SetDesktopWallpaperAsync,
                },
            };

            if (parsedArgs.EnableInternetExplorerMode.HasValue &&
                parsedArgs.EnableInternetExplorerMode.Value)
            {
                packages.Add(new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.CustomAction,
                    TargetSiteName = UIStringResources.Option_Prerequisites,
                    PackageName = UIStringResources.Install_EnableIEMode,
                    CustomAction = EnableIEModeAsync,
                });
            }

            foreach (var eachTargetName in targets)
            {
                var targetService = catalog.Services.FirstOrDefault(x => string.Equals(eachTargetName, x.Id, StringComparison.Ordinal));

                if (targetService == null)
                    continue;

                packages.AddRange(targetService.Packages.Select(eachPackage => new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.DownloadAndInstall,
                    TargetSiteName = targetService.DisplayName,
                    TargetSiteUrl = targetService.Url,
                    PackageName = eachPackage.Name,
                    PackageUrl = eachPackage.Url,
                    Arguments = eachPackage.Arguments,
                    Installed = null,
                }));

                var bootstrapData = targetService.CustomBootstrap;

                if (!string.IsNullOrWhiteSpace(bootstrapData))
                {
                    packages.Add(new InstallItemViewModel()
                    {
                        InstallItemType = InstallItemType.PowerShellScript,
                        TargetSiteName = targetService.DisplayName,
                        TargetSiteUrl = targetService.Url,
                        PackageName = UIStringResources.Hostess_CustomScript_Title,
                        ScriptContent = bootstrapData,
                    });
                }
            }

            if (parsedArgs.InstallAdobeReader.HasValue &&
                parsedArgs.InstallAdobeReader.Value)
            {
                packages.Add(new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.OpenWebSite,
                    TargetSiteName = UIStringResources.Option_Addin,
                    TargetSiteUrl = CommonStrings.AppInfoUrl,
                    PackageName = UIStringResources.Option_InstallAdobeReader,
                    PackageUrl = CommonStrings.AdobeReaderUrl,
                    Arguments = string.Empty,
                    ScriptContent = string.Empty,
                });
            }

            if (parsedArgs.InstallEveryonesPrinter.HasValue &&
                parsedArgs.InstallEveryonesPrinter.Value)
            {
                packages.Add(new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.OpenWebSite,
                    TargetSiteName = UIStringResources.Option_Addin,
                    PackageName = UIStringResources.Option_InstallEveryonesPrinter,
                    PackageUrl = CommonStrings.EveryonesPrinterUrl,
                });
            }

            if (parsedArgs.InstallHancomOfficeViewer.HasValue &&
                parsedArgs.InstallHancomOfficeViewer.Value)
            {
                packages.Add(new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.OpenWebSite,
                    TargetSiteName = UIStringResources.Option_Addin,
                    PackageName = UIStringResources.Option_InstallHancomOfficeViewer,
                    PackageUrl = CommonStrings.HancomOfficeViewerUrl,
                });
            }

            if (parsedArgs.InstallRaiDrive.HasValue &&
                parsedArgs.InstallRaiDrive.Value)
            {
                packages.Add(new InstallItemViewModel()
                {
                    InstallItemType = InstallItemType.OpenWebSite,
                    TargetSiteName = UIStringResources.Option_Addin,
                    PackageName = UIStringResources.Option_InstallRaiDrive,
                    PackageUrl = CommonStrings.RaiDriveUrl,
                });
            }

            return packages;
        }

        private async Task PrepareDirectoriesAsync(InstallItemViewModel viewModel)
        {
            var parsedArgs = _commandLineArguments.Current;

            if (parsedArgs.DryRun)
            {
                await Task.Delay(TimeSpan.FromSeconds(1d)).ConfigureAwait(false);
                return;
            }

            var downloadFolderPath = _sharedLocations.GetDownloadDirectoryPath();

            if (!Directory.Exists(downloadFolderPath))
                Directory.CreateDirectory(downloadFolderPath);
        }

        private async Task EnableIEModeAsync(InstallItemViewModel viewModel)
        {
            var parsedArgs = _commandLineArguments.Current;

            if (parsedArgs.DryRun)
            {
                await Task.Delay(TimeSpan.FromSeconds(1d)).ConfigureAwait(false);
                return;
            }

            if (parsedArgs.EnableInternetExplorerMode ?? false)
            {
                // HKLM\SOFTWARE\Policies\Microsoft\Edge > InternetExplorerIntegrationLevel (REG_DWORD) with value 1, InternetExplorerIntegrationSiteList (REG_SZ)
                using (var ieModeKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge", true))
                {
                    ieModeKey.SetValue("InternetExplorerIntegrationLevel", 1, RegistryValueKind.DWord);
                    ieModeKey.SetValue("InternetExplorerIntegrationSiteList", ConstantStrings.IEModePolicyXmlUrl, RegistryValueKind.String);
                }

                // msedge.exe 파일 경로를 유추하고, Policy를 반영하기 위해 잠시 실행했다가 종료하는 동작을 추가
                if (!_sharedLocations.TryGetMicrosoftEdgeExecutableFilePath(out var msedgePath))
                    msedgePath = _sharedLocations.GetDefaultX86MicrosoftEdgeExecutableFilePath();

                if (File.Exists(msedgePath))
                {
                    var msedgePsi = new ProcessStartInfo(msedgePath, "about:blank")
                    {
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Minimized,
                    };

                    using (var msedgeProcess = Process.Start(msedgePsi))
                    {
                        var tcs = new TaskCompletionSource<int>();
                        msedgeProcess.EnableRaisingEvents = true;
                        msedgeProcess.Exited += (_sender, _e) =>
                        {
                            tcs.SetResult(msedgeProcess.ExitCode);
                        };
                        await Task.Delay(TimeSpan.FromSeconds(1.5d)).ConfigureAwait(false);
                        msedgeProcess.CloseMainWindow();
                        await tcs.Task.ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task VerifyWindowsContainerEnvironmentAsync(InstallItemViewModel viewModel)
        {
            var parsedArgs = _commandLineArguments.Current;

            if (parsedArgs.DryRun)
            {
                await Task.Delay(TimeSpan.FromSeconds(1d)).ConfigureAwait(false);
                return;
            }

            if (!_validAccountNames.Contains(Environment.UserName, StringComparer.Ordinal))
            {
                var response = _appMessageBox.DisplayQuestion(
                    AskStrings.Ask_WarningForNonSandboxEnvironment,
                    defaultAnswer: MessageBoxResult.No);

                if (response != MessageBoxResult.Yes)
                    Environment.Exit(1);
            }
        }

        private async Task TryProtectCriticalServicesAsync(InstallItemViewModel viewModel)
        {
            var parsedArgs = _commandLineArguments.Current;

            if (parsedArgs.DryRun)
            {
                await Task.Delay(TimeSpan.FromSeconds(1d)).ConfigureAwait(false);
                return;
            }

            try { _criticalServiceProtector.PreventServiceProcessTermination("TermService"); }
            catch (AggregateException aex) { _appMessageBox.DisplayError(aex.InnerException, false); }
            catch (Exception ex) { _appMessageBox.DisplayError(ex, false); }

            try { _criticalServiceProtector.PreventServiceStop("TermService", Environment.UserName); }
            catch (AggregateException aex) { _appMessageBox.DisplayError(aex.InnerException, false); }
            catch (Exception ex) { _appMessageBox.DisplayError(ex, false); }
        }

        private async Task SetDesktopWallpaperAsync(InstallItemViewModel viewModel)
        {
            var parsedArgs = _commandLineArguments.Current;

            if (parsedArgs.DryRun)
            {
                await Task.Delay(TimeSpan.FromSeconds(1d)).ConfigureAwait(false);
                return;
            }

            var picturesDirectoryPath = _sharedLocations.GetPicturesDirectoryPath();

            if (!Directory.Exists(picturesDirectoryPath))
                Directory.CreateDirectory(picturesDirectoryPath);

            var wallpaperPath = Path.Combine(picturesDirectoryPath, "Signature.jpg");
            Properties.Resources.Signature.Save(wallpaperPath, ImageFormat.Jpeg);

            _ = NativeMethods.SystemParametersInfoW(
                NativeMethods.SetDesktopWallpaper, 0, wallpaperPath,
                NativeMethods.UpdateIniFile | NativeMethods.SendWinIniChange);

            NativeMethods.UpdatePerUserSystemParameters(
                IntPtr.Zero, IntPtr.Zero, "1, True", 0);
        }
    }
}