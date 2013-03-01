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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Transactions;
using Dapper;

namespace FileZapper.Data
{
    class ZapperFile : IEquatable<ZapperFile>
    {
        private string Connection;

        public ZapperFolder Root { get; set; }
        public string FullPath { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Directory { get; set; }
        public long Size { get; set; }
        public DateTime? FileModified { get; set; }
        public string ContentHash { get; set; }
        public DateTime? Added { get; set; }
        public bool IsSystem { get; set; }
        public int Score { get; set; }

        public ZapperFile()
        {
            Id = Guid.NewGuid();
            Connection = ConfigurationManager.ConnectionStrings["zapper"].ConnectionString;
        }

        public ZapperFile(ZapperFolder root, string sFileName)
            : this()
        {
            Root = root;
            FileInfo f = new FileInfo(sFileName);
            IsSystem = f.Attributes.HasFlag(FileAttributes.System);
            FullPath = f.FullName;
            Name = f.Name;
            Directory = f.DirectoryName;
            Extension = f.Extension.ToLower();
            Size = f.Length;

            // A little hack to deal with value being stored in db as smalldatetime.
            // This way comparisons of the field will work between db/filesystem.
            // Using temp field so value isn't rewritten to the file system.
            int iMinuteBonus = f.LastWriteTime.Second < 30 ? 0 : 1;
            DateTime dt = f.LastWriteTime.AddMinutes(iMinuteBonus);
            FileModified = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
        }

        public bool Equals(ZapperFile other)
        {
            if (Object.ReferenceEquals(other, null)) { return false; }
            if (Object.ReferenceEquals(this, other)) { return true; }
            return
                FullPath == other.FullPath
                && Size == other.Size
                && Extension == other.Extension
                && Name == other.Name
                && Directory == other.Directory
                && FileModified == other.FileModified
                && ContentHash == ContentHash;
        }

        public bool EqualsIgnoreHash(ZapperFile other)
        {
            if (Object.ReferenceEquals(other, null)) { return false; }
            if (Object.ReferenceEquals(this, other)) { return true; }

            return
                FullPath == other.FullPath
                && Size == other.Size
                && Extension == other.Extension
                && Name == other.Name
                && Directory == other.Directory
                && FileModified == other.FileModified;
        }

        /// <summary>
        /// If force is true, it will always hash the file.
        /// Otherwise, it will only do so for existing hashed files if the metadata is different.
        /// </summary>
        public void Hashify(bool bForce = false)
        {
            try
            {
                if (!bForce)
                {
                    ZapperFile f = new ZapperFile(Root, FullPath);
                    if (!string.IsNullOrWhiteSpace(ContentHash)
                        && EqualsIgnoreHash(f))
                    {
                        return;
                    }
                }

                using (MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider())
                {
                    byte[] hashvalue;
                    using (var stream = new BufferedStream(File.OpenRead(FullPath), 1200000))
                    {
                        hashvalue = hasher.ComputeHash(stream);
                    }
                    ContentHash = System.BitConverter.ToString(hashvalue);
                }
                SaveToDB();
            }
            catch (Exception ex)
            {
                Exceptioneer.Log(ex, "File tagged with INVALID content hash: " + FullPath);
                ContentHash = "INVALID";
                SaveToDB();
            }
        }

        public void SaveToDB()
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (SqlConnection con = new SqlConnection(Connection))
                {
                    using (SqlCommand cmd = new SqlCommand("ins_zfile", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@zfile_id", Id);
                        cmd.Parameters.AddWithValue("@zfile_name", Name);
                        cmd.Parameters.AddWithValue("@zfile_ext", Extension);
                        cmd.Parameters.AddWithValue("@zfile_dir", Directory);
                        cmd.Parameters.AddWithValue("@zfile_path", FullPath);
                        cmd.Parameters.AddWithValue("@zfile_size", Size);
                        cmd.Parameters.AddWithValue("@zfile_hash", ContentHash);
                        cmd.Parameters.AddWithValue("@zfile_modified", FileModified);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeleteFromDB(Guid sessionid, bool bAllowUndelete = true)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (SqlConnection con = new SqlConnection(Connection))
                {
                    con.Open();
                    string sSql = "";
                    if (bAllowUndelete)
                    {
                        sSql += @"
                    insert zfile_deleted
	                    (FullPath, Deleted, SessionId, Id, Name, Extension, Directory, Size, FileModified, ContentHash, Added, Modified)
                    select
	                    FullPath, SYSDATETIME(), @SessionId, Id, Name, Extension, Directory, Size, FileModified, ContentHash, Added, Modified
                    from zfile
                    where FullPath = @FullPath
                    ";
                    }
                    sSql += "delete zfile where FullPath = @FullPath";
                    con.Execute(sSql, new { @SessionId = sessionid, FullPath = this.FullPath });
                }
            }
        }
    }
}
