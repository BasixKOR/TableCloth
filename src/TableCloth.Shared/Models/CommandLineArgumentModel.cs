﻿using System.Collections.Generic;
using System.Linq;
using TableCloth.Resources;

namespace TableCloth.Models
{
    public sealed class CommandLineArgumentModel
    {
#pragma warning disable IDE0290 // Use primary constructor
        public CommandLineArgumentModel(
#pragma warning restore IDE0290 // Use primary constructor
            string[] rawArguments,
            string[]
#if !NETFX
            ?
#endif
            selectedServices = default,
            bool? enableMicrophone = default,
            bool? enableWebCam = default,
            bool? enablePrinters = default,

            string
#if !NETFX
            ?
#endif
            certPrivateKeyPath = default,

            string
#if !NETFX
            ?
#endif
            certPublicKeyPath = default,

            bool? installEveryonesPrinter = default,
            bool? installAdobeReader = default,
            bool? installHancomOfficeViewer = default,
            bool? installRaiDrive = default,
            bool? enableInternetExplorerMode = default,
            bool showCommandLineHelp = default,
            bool showVersionHelp = default,
            bool dryRun = default,
            bool simulateFailure = false)
        {
            RawArguments = rawArguments;
            SelectedServices = selectedServices ?? Enumerable.Empty<string>();
            EnableMicrophone = enableMicrophone;
            EnableWebCam = enableWebCam;
            EnablePrinters = enablePrinters;
            CertPrivateKeyPath = certPrivateKeyPath;
            CertPublicKeyPath = certPublicKeyPath;
            InstallEveryonesPrinter = installEveryonesPrinter;
            InstallAdobeReader = installAdobeReader;
            InstallHancomOfficeViewer = installHancomOfficeViewer;
            InstallRaiDrive = installRaiDrive;
            EnableInternetExplorerMode = enableInternetExplorerMode;
            ShowCommandLineHelp = showCommandLineHelp;
            ShowVersionHelp = showVersionHelp;
            DryRun = dryRun;
            SimulateFailure = simulateFailure;
        }

        public string[] RawArguments { get; private set; }

        public bool? EnableMicrophone { get; private set; }

        public bool? EnableWebCam { get; private set; }

        public bool? EnablePrinters { get; private set; }

        public string
#if !NETFX
            ?
#endif
            CertPrivateKeyPath
        { get; private set; }

        public string
#if !NETFX
            ?
#endif
            CertPublicKeyPath
        { get; private set; }

        public bool? InstallEveryonesPrinter { get; private set; }

        public bool? InstallAdobeReader { get; private set; }

        public bool? InstallHancomOfficeViewer { get; private set; }

        public bool? InstallRaiDrive { get; private set; }

        public bool? EnableInternetExplorerMode { get; private set; }

        public bool ShowCommandLineHelp { get; private set; }

        public bool ShowVersionHelp { get; private set; }

        public IEnumerable<string> SelectedServices { get; private set; } = new List<string>();

        public bool DryRun { get; private set; }

        public bool SimulateFailure { get; private set; }

        public override string ToString()
        {
            var options = new List<string>();

            if (ShowCommandLineHelp)
                options.Add(ConstantStrings.TableCloth_Switch_Help);
            else if (ShowVersionHelp)
                options.Add(ConstantStrings.TableCloth_Switch_Version);
            else
            {
                if (EnableMicrophone.HasValue && EnableMicrophone.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_EnableMicrophone);
                if (EnableWebCam.HasValue && EnableWebCam.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_EnableCamera);
                if (EnablePrinters.HasValue && EnablePrinters.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_EnablePrinter);

                if (!string.IsNullOrWhiteSpace(CertPublicKeyPath))
                {
                    options.Add(ConstantStrings.TableCloth_Switch_CertPublicKey);
                    options.Add(CertPublicKeyPath);
                }

                if (!string.IsNullOrWhiteSpace(CertPrivateKeyPath))
                {
                    options.Add(ConstantStrings.TableCloth_Switch_CertPrivateKey);
                    options.Add(CertPrivateKeyPath);
                }

                if (InstallEveryonesPrinter.HasValue && InstallEveryonesPrinter.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_InstallEveryonesPrinter);
                if (InstallAdobeReader.HasValue && InstallAdobeReader.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_InstallAdobeReader);
                if (InstallHancomOfficeViewer.HasValue && InstallHancomOfficeViewer.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_InstallHancomOfficeViewer);
                if (InstallRaiDrive.HasValue && InstallRaiDrive.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_InstallRaiDrive);
                if (EnableInternetExplorerMode.HasValue && EnableInternetExplorerMode.Value)
                    options.Add(ConstantStrings.TableCloth_Switch_EnableIEMode);

                if (DryRun)
                    options.Add(ConstantStrings.TableCloth_Switch_DryRun);
                if (SimulateFailure)
                    options.Add(ConstantStrings.TableCloth_Switch_SimulateFailure);

                foreach (var eachSite in SelectedServices)
                    options.Add(eachSite);
            }

#pragma warning disable IDE0305 // Simplify collection initialization
            return string.Join(" ", options.ToArray());
#pragma warning restore IDE0305 // Simplify collection initialization
        }
    }
}
