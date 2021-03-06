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
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace MPExtended.Libraries.General
{
    public class Configuration
    {
        private static ServicesConfiguration serviceConfig = null;
        private static MediaAccessConfiguration mediaConfig = null;
        private static StreamingConfiguration streamConfig = null;

        public static ServicesConfiguration Services 
        {
            get 
            {
                if (serviceConfig == null)
                    serviceConfig = new ServicesConfiguration();

                return serviceConfig;
            }
        }

        public static MediaAccessConfiguration Media
        {
            get
            {
                if (mediaConfig == null)
                    mediaConfig = new MediaAccessConfiguration();

                return mediaConfig;
            }
        }

        public static StreamingConfiguration Streaming
        {
            get
            {
                if (streamConfig == null)
                    streamConfig = new StreamingConfiguration();

                return streamConfig;
            }
        }

        public static string GetPath(string filename)
        {
            return Path.Combine(Installation.GetConfigurationDirectory(), filename);
        }
    }
}
