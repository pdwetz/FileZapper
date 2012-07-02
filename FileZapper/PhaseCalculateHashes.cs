/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2012 Peter Wetzel

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
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace FileZapper
{
    /// <summary>
    /// Calculate hashes
    /// </summary>
    class PhaseCalculateHashes : IZapperPhase
    {
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; }
        public bool IsInitialPhase { get; set; }

        public PhaseCalculateHashes()
        {
            Name = "Calculate hashes (this may take awhile)";
        }

        public void Process()
        {
            var files = GetPossibleDupes();
            // Parallelism explicitly limited, as default setting was causing hundreds of threads to be created and killed machine performance.
            // Instead, we're simply using the logical processor count (including physical cores and hyperthreading)
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, hashfile =>
            {
                // Note: hashify will determine if it needs to calc hash again; it metadata is unchanged it will skip
                hashfile.Hashify();
            });
        }

        /// <summary>
        /// Retrieve all files from database that could be duplicates (have more than one match for extension and file size)
        /// </summary>
        public IEnumerable<ZapperFile> GetPossibleDupes()
        {
            using (var con = new SqlConnection(ZapperProcessor.Connection))
            {
                con.Open();
                string sSql = @"
                    with Dupes (Extension, Size)
                    as
                    (
	                    select Extension, Size
	                    from zfile
	                    group by Extension, Size
	                    having count(*) > 1
                    )
                    select f.*
                    from zfile f, Dupes d
                    where f.Extension = d.Extension
	                    and f.Size = d.Size
                    ";

                return con.Query<ZapperFile>(sSql);
            }
        }
    }
}
