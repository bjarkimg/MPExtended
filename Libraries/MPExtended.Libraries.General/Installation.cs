﻿#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Win32;

namespace MPExtended.Libraries.General
{
    public enum MPExtendedService
    {
        MediaAccessService,
        TVAccessService,
        StreamingService,
        UserSessionService,
        WifiRemote
    }

    public static class Installation
    {
        public static string GetRootDirectory()
        {
            // FIXME: It's not really a good idea that this function has a totally different function in Debug and
            // Release builds. 
#if DEBUG
            // It's a bit tricky to find the root source directory for Debug builds. The assembly might be in a different
            // directory then where it's source tree is (which happens with WebMP for example), so we can't use the Location
            // of the current executing assembly. The CodeBase points to it's original location on compilation, but this 
            // doesn't work if you send files to other people (so don't do it!).
            // Also, not everybody names the root directory MPExtended. Instead, we look for a directory with a child directory
            // named Config, which has a Debug child directory. It's not 100% foolproof but it works good enough.
            Uri originalPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(originalPath.LocalPath));
            do
            {
                if (Directory.Exists(Path.Combine(info.FullName, "Config")) && Directory.Exists(Path.Combine(info.FullName, "Config", "Debug")))
                {
                    return info.FullName;
                }
                info = info.Parent;
            } while (info != null);

            string curDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.GetFullPath(Path.Combine(curDir, "..", "..", "..", ".."));
#else
            // By default we try to read it from the registry, where the install location is set during installation. If
            // that fails, fall back to the parent directory of the currently executing assembly, which probably works good
            // for 99% of the cases. 
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\MPExtended");
            if (key != null)
            {
                object value = key.GetValue("InstallLocation");
                if (value != null)
                {
                    return value.ToString();
                }
            }

            DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            return info.Parent.FullName;
#endif
        }

        public static string GetConfigurationDirectory()
        {
#if DEBUG
            return Path.Combine(GetRootDirectory(), "Config", "Debug");
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended");
#endif
        }

        public static string GetLogDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs");
        }

        public static List<Service> GetInstalledServices()
        {
            List<Service> allServices = new List<Service>()
            {
                new Service(MPExtendedService.MediaAccessService, "MPExtended.Services.MediaAccessService", "MediaAccessService", "_mpextended-mas._tcp"),
                new Service(MPExtendedService.TVAccessService, "MPExtended.Services.TVAccessService", "TVAccessService", "_mpextended-tas._tcp"),
                new Service(MPExtendedService.StreamingService, "MPExtended.Services.StreamingService", "StreamingService", "_mpextended-wss._tcp"),
                new Service(MPExtendedService.UserSessionService, "MPExtended.Services.UserSessionService", "UserSessionProxyService", "_mpextended-uss._tcp"),
                new WifiRemoteService()
            };

            string[] disabled =
                XElement.Load(Configuration.GetPath("Services.xml"))
                .Element("disabledServices")
                .Elements("service")
                .Select(x => x.Value)
                .ToArray();

            return allServices.Where(x => x.IsInstalled && !disabled.Contains(x.Assembly)).ToList();
        }

        public static bool IsServiceInstalled(MPExtendedService srv)
        {
            return GetInstalledServices().Any(x => x.ServiceName == srv);
        }
    }
}
