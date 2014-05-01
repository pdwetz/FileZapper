/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2014 Peter Wetzel

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Configuration;

namespace FileZapper.Core.Configuration
{
    [ConfigurationCollection(typeof(ZapperFolderConfigElement))]
    public class ZapperFolderConfigCollection : ConfigurationElementCollection
    {
        public ZapperFolderConfigElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as ZapperFolderConfigElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ZapperFolderConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ZapperFolderConfigElement)element).FullPath;
        }
    }
}