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
using System.Reflection;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal class LazyLibraryList<T> : ILibraryList<T> where T : ILibrary
    {
        private IDictionary<int, Lazy<T, IDictionary<string, object>>> items = new Dictionary<int, Lazy<T, IDictionary<string, object>>>();
        private ProviderType type;

        public LazyLibraryList(IDictionary<int, Lazy<T, IDictionary<string, object>>> dict, ProviderType type) 
        {
            items = dict;
            this.type = type;
        }

        public void Add(int key, Lazy<T, IDictionary<string, object>> value) 
        {
            items[key] = value;
        }

        public T this[int? key]
        {
            get
            {
                return GetValue(key);
            }
        }

        public ICollection<int> Keys
        {
            get
            {
                return items.Where(x => x.Value.Value.Supported).Select(x => x.Key).ToList();
            }
        }

        public T GetValue(int? passedId)
        {
            int key = passedId.HasValue ? passedId.Value : ProviderHandler.GetDefaultProvider(type);

            if (!items.ContainsKey(key) || !items[key].Value.Supported)
            {
                Log.Error("Tried to get library for unknown id {0}", key);
                return default(T);
            }

            return items[key].Value;
        }

        public int Count()
        {
            return items.Count(x => x.Value.Value.Supported);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.Where(x => x.Value.Value.Supported).Select(x => GetValue(x.Key)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // more specific methods below
        public int GetKeyByName(string name)
        {
            var list = items.Where(x => x.Value.Value.Supported && (string)x.Value.Metadata["Name"] == name);
            if (list.Count() > 0)
            {
                return list.First().Key;
            }

            return 0;
        }

        public List<WebBackendProvider> GetAllAsBackendProvider()
        {
            return items.Values
                .Where(x => x.Value.Supported)
                .Select(x => new WebBackendProvider()
                {
                    Name = (string)x.Metadata["Name"],
                    Id = (int)x.Metadata["Id"],
                    Version = VersionUtil.GetBuildVersion(x.Value.GetType().Assembly).ToString()
                })
                .ToList();
        }

        public IEnumerable<WebSearchResult> SearchAll(string text)
        {
            return items
                .ToDictionary(x => x.Key, x => x.Value.Value)
                .Where(x => x.Value.Supported)
                .SelectMany(x => x.Value.Search(text).Finalize((int)items[x.Key].Metadata["Id"], type));
        }
    }
}
