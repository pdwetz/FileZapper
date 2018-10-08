/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2018 Peter Wetzel

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
namespace FileZapper.Core.Utilities
{
    public class ByteCompare
    {
        public static bool SafeCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
            {
                return false;
            }
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }
            return true;
        }

        // Copyright (c) 2008-2013 Hafthor Stefansson
        // Distributed under the MIT/X11 software license
        // Ref: http://www.opensource.org/licenses/mit-license.php.
        //public static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        //{
        //    if (a1 == null || a2 == null || a1.Length != a2.Length)
        //        return false;
        //    fixed (byte* p1 = a1, p2 = a2)
        //    {
        //        byte* x1 = p1, x2 = p2;
        //        int l = a1.Length;
        //        for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
        //            if (*((long*)x1) != *((long*)x2)) return false;
        //        if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
        //        if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
        //        if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
        //        return true;
        //    }
        //}
    }
}