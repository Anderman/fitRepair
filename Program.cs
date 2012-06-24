using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
//using System.Data.OleDb;
//using System.Data.OracleClient;
//using System.Data.Odbc;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
namespace myFit
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                string filename = args[0];
                if (filename.ToLower().EndsWith(".fit"))
                {
                    FileStream file = (new FileStream(filename, FileMode.Open));
                    StreamWriter outFile = (new StreamWriter(filename.ToLower().Replace(".fit", ".csv"), false));
                    new fitFileReader(file, outFile);
                    file.Close();
                    outFile.Close();
                }
                else
                {
                    StreamReader file = (new StreamReader(filename));
                    FileStream outFile = (new FileStream(filename.ToLower().Replace(".csv", "_fixed.fit"), FileMode.Create));
                    new fitFileWrite(file, outFile);
                    file.Close();
                    outFile.Close();
                }
            }
            else
            {
                //OdbcConnection conn = new OdbcConnection();
                OracleConnection conn1 = new OracleConnection("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle4)(PORT=1521))(CONNECT_DATA=(SID=tddb1)));User Id=TDMICRO_LEES;Password=welcome01;");
                //conn.ConnectionString = "Driver={Microsoft ODBC for Oracle};server=oraclev4;Uid=TDMICRO_LEES;Pwd=welcom01;";
                //conn.ConnectionString = "Driver={Microsoft ODBC for Oracle};Server=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oraclev4)(PORT=1521))(CONNECT_DATA=(SID=tddb1)));Uid=TDMICRO_LEES;Pwd=welcom01;";
                //conn.ConnectionString = "Provider=OraOLEDB.Oracle;Data Source=oraclev4;User Id=TDMICRO_LEES;Password=welcome01;OLEDB.NET=True;";
                conn1.Open();
                OracleCommand cmd = new OracleCommand("SELECT REQUESTS.REQUESTID, REQUESTS.COLLECTIONDATE, PATIENTS.BIRTHDATE, PATIENTS.PATID, PATIENTS.PATNUMBER, HOSPITALIZATIONS.HOSPITNUMBER, PATIENTS.BENNUMBER, PATIENTS.NAME, PATIENTS.FIRSTNAME, PATIENTS.SEX, PATIENTS.ETHNICORIGIN, PATIENTS.RELIGION, DICT_SERVICES.SHORTTEXT HOSPSERVICE, REQUESTS.ACCESSNUMBER, DICT_LOCATIONS.LOCID, DICT_LOCATIONS.LOCCODE, DICT_DOCTORS.DOCID, DICT_DOCTORS.DOCCODE, DICT_TESTS.TESTCODE, DICT_TESTS.RESPRECISION, TESTS.RESTYPE, TESTS.RESVALUE, DICT_TEXTS.TEXTCODE, DICT_TEXTS.SHORTTEXT FROM tdmicro_main.PATIENTS, tdmicro_main.REQUESTS, tdmicro_main.HOSPITALIZATIONS, tdmicro_main.DICT_SERVICES, tdmicro_main.LOCATIONS, tdmicro_main.DOCTORS, tdmicro_main.DICT_LOCATIONS, tdmicro_main.DICT_DOCTORS, tdmicro_main.TESTS, tdmicro_main.DICT_TESTS, tdmicro_main.DICT_TEXTS WHERE (REQUESTS.COLLECTIONDATE >= to_date('2012-06-05','yyyy-mm-dd'))     AND (REQUESTS.COLLECTIONDATE <= to_date('2012-06-14','yyyy-mm-dd'))             AND ((TESTS.TESTID IN (3375)))             AND (PATIENTS.PATID = REQUESTS.PATID)             AND (REQUESTS.HOSPITID = HOSPITALIZATIONS.HOSPITID (+))             AND (HOSPITALIZATIONS.SERVICEID = DICT_SERVICES.SERVICEID (+))             AND (REQUESTS.REQUESTID = LOCATIONS.REQUESTID (+))             AND (LOCATIONS.LOCID = DICT_LOCATIONS.LOCID (+))             AND (REQUESTS.REQUESTID = DOCTORS.REQUESTID (+))             AND (DOCTORS.DOCID = DICT_DOCTORS.DOCID (+)             AND DOCTORS.PRESCRIBER (+) = 1)             AND (REQUESTS.REQUESTID = TESTS.REQUESTID)             AND (TESTS.TESTID = DICT_TESTS.TESTID)             AND (TESTS.CODEDRESULTID = DICT_TEXTS.TEXTID (+)) ORDER BY REQUESTS.COLLECTIONDATE, REQUESTS.REQUESTID");
                cmd.Connection = conn1;
                OracleDataReader od= cmd.ExecuteReader();
                while (od.Read())
                {
                    Console.WriteLine(od.GetValue(0).ToString());
                }
            }
        }
    }
    public static class con
    {
    }
}
