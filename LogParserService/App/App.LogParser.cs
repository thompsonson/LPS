using System;
using System.Collections.Generic;
using System.Text;
using MSUtil;
using System.Data;
using System.Text.RegularExpressions;

namespace LogParserService
{
    class LogParser
    {

        public static Type[] types = new Type[] 
        {  
            Type.GetType("System.Int32"), 
            Type.GetType("System.Single"),
            Type.GetType("System.String"), 
            Type.GetType("System.DateTime"),
            Type.GetType("System.Nullable")
        };

        public static DataTable ParseEventLog(string sql)
        {
            return Execute<COMEventLogInputContextClassClass>(sql);
        }

        public static DataTable ParseEventLog(string sql, string iCheckpoint)
        {
            COMEventLogInputContextClassClass Input = new COMEventLogInputContextClassClass();

            Input.iCheckpoint = iCheckpoint;

            return Execute(sql, Input);
        }


        public static void BatchEventLog(string sql)
        {
            ExectuteBatch<COMEventLogInputContextClassClass>(sql);
        }

        public static void BatchEventLog(string sql, string iCheckpoint)
        {
            COMEventLogInputContextClassClass Input = new COMEventLogInputContextClassClass();

            Input.iCheckpoint = iCheckpoint;

            ExectuteBatch(sql, Input);
        }
        

        public static DataTable ParseW3C(string sql)
        {
            return Execute<COMW3CInputContextClassClass>(sql);
        }
        
        public static DataTable ParseIISLog(string sql)
        {
            return Execute<COMIISW3CInputContextClassClass>(sql);
        }

        public static DataTable ParseIISLog(string sql, string iCheckpoint)
        {
            COMIISW3CInputContextClassClass Input = new COMIISW3CInputContextClassClass();
            
            Input.iCheckpoint = iCheckpoint;

            return Execute(sql, Input);
        }


        public static void BatchIISLog(string sql)
        {
            ExectuteBatch<COMIISW3CInputContextClassClass>(sql);
        }

        public static void BatchIISLog(string sql, string iCheckpoint)
        {
            COMIISW3CInputContextClassClass Input = new COMIISW3CInputContextClassClass();

            Input.iCheckpoint = iCheckpoint;

            ExectuteBatch(sql, Input);
        }


        //TODO: add methods for other types
        // W3C - COMW3CInputContextClassClass
        // NCSA - COMIISNCSAInputContextClassClass

        
        private static DataTable Execute<T>(string query) where T : new()
        {
            LogQueryClassClass log = new LogQueryClassClass();
            ILogRecordset recordset = log.Execute(query, new T());
            ILogRecord record = null;

            DataTable dt = new DataTable();
            Int32 columnCount = recordset.getColumnCount();

            for (int i = 0; i < columnCount; i++)
            {
                dt.Columns.Add(recordset.getColumnName(i), types[recordset.getColumnType(i) - 1]);
            }

            for (; !recordset.atEnd(); recordset.moveNext())
            {
                DataRow dr = dt.NewRow();

                record = recordset.getRecord();

                for (int i = 0; i < columnCount; i++)
                {
                    dr[i] = record.getValue(i);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable Execute(string query, object Input)
        {
            LogQueryClassClass log = new LogQueryClassClass();

            ILogRecordset recordset = log.Execute(query, Input);
            ILogRecord record = null;

            DataTable dt = new DataTable();
            Int32 columnCount = recordset.getColumnCount();

            for (int i = 0; i < columnCount; i++)
            {
                dt.Columns.Add(recordset.getColumnName(i), types[recordset.getColumnType(i) - 1]);
            }

            for (; !recordset.atEnd(); recordset.moveNext())
            {
                DataRow dr = dt.NewRow();

                record = recordset.getRecord();

                for (int i = 0; i < columnCount; i++)
                {
                    dr[i] = record.getValue(i);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        // todo: check for parse errors
        private static void ExectuteBatch<T>(string query) where T : new()
        {
            LogQueryClassClass log = new LogQueryClassClass();

            log.ExecuteBatch(query, new T());
        }

        private static void ExectuteBatch(string query, object Input)
        {
            LogQueryClassClass log = new LogQueryClassClass();

            log.ExecuteBatch(query, Input);
        }

        private static void ExecuteBatch(string query, object Input, object Output)
        {
            LogQueryClassClass log = new LogQueryClassClass();

            log.ExecuteBatch(query, Input, Output);
        }

    }
}
