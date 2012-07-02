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
using System.Data.SqlClient;
using System.Transactions;
using Dapper;

namespace FileZapper
{
    /// <summary>
    /// Lightweight helper class for logging exceptions to database
    /// </summary>
    public class Exceptioneer
    {
        public int Id { get; set; }
        public DateTime AddDate { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionType { get; set; }
        public string AssemblyName { get; set; }
        public string Thread { get; set; }
        public string InnerException { get; set; }
        public string StackTrace { get; set; }
        public string ExceptionNotes { get; set; }

        public Exceptioneer(Exception ex, string sNotes)
        {
            AssemblyName = ex.Source;
            ExceptionMessage = ex.Message;
            ExceptionType = ex.GetType().Name;
            StackTrace = ex.StackTrace;
            InnerException = ex.InnerException == null ? null : ex.InnerException.ToString();
            ExceptionNotes = sNotes;
            Thread = string.IsNullOrWhiteSpace(System.Threading.Thread.CurrentThread.Name) ? System.Threading.Thread.CurrentThread.ManagedThreadId.ToString()
                : string.Format("{0} - {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, System.Threading.Thread.CurrentThread.Name);
        }

        /// <summary>
        /// Log exception to database and to user console
        /// </summary>
        static public void Log(Exception ex, string sNotes = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}: ERROR ******", DateTime.Now.ToString("HH:mm:ss.fff"));
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;

            Exceptioneer e = new Exceptioneer(ex, sNotes);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                string sCon = ConfigurationManager.ConnectionStrings["zapper"].ConnectionString;
                using (var con = new SqlConnection(sCon))
                {
                    con.Open();
                    con.Execute(@"insert Exception (AddDate, ExceptionMessage, ExceptionType, AssemblyName, Thread, InnerException, StackTrace, ExceptionNotes)
                    values (SYSDATETIME(), @ExceptionMessage, @ExceptionType, @AssemblyName, @Thread, @InnerException, @StackTrace, @ExceptionNotes)",
                        e);
                }
            }
        }
    }
}
