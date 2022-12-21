using System;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Collections.Generic;


public class RecordSet
{

    private Dictionary<string, Dictionary<string, string>> default_db;

    private IDbConnection m_Connection = null;
    private string m_ConnectionString = null;
    private IDataReader m_Reader = null;
    private bool m_Eof = true;
    private string m_Mode = null;

    private string m_EditTable = "";
    private Dictionary<string, string> m_EditSet;
    private Int32 m_EditId = 0;
    private string m_IdField = "id";

    //Construtor
    public RecordSet()
    {
        this.setDatabases();
    }
    public RecordSet(string db) : this()
    {

        this.Connect(db);
    }

    private void setDatabases()
    {
        //create Connection strings for all connections
        //test
        /*
        Dictionary<string, string> infostore = new Dictionary<string, string>(){
            {"db_type",""},
            {"host",""},
            {"dbname",""},
            {"username",""},
            {"password",""}
        };
        */

        //prod
        Dictionary<string, string> infostore = new Dictionary<string, string>(){
            {"db_type",""},
            {"host",""},
            {"dbname",""},
            {"username",""},
            {"password",""}
        };

        default_db = new Dictionary<string, Dictionary<string, string>>()
            {
                {"default",  infostore},

            };
    }


    #region Get Functions

    public bool IsNull(string Field)
    {
        return m_Reader.IsDBNull(m_Reader.GetOrdinal(Field));
    }

    public string GetString(string Field)
    {
        if (m_Reader.IsDBNull(m_Reader.GetOrdinal(Field)))
            return null;
        return m_Reader.GetString(m_Reader.GetOrdinal(Field));
    }

    public Int32 GetInt32(string Field)
    {
        return m_Reader.GetInt32(m_Reader.GetOrdinal(Field));
    }

    public long GetLong(string Field)
    {
        if (m_Reader.IsDBNull(m_Reader.GetOrdinal(Field))) return 0;
        return m_Reader.GetInt64(m_Reader.GetOrdinal(Field));
    }

    public float GetFloat(string Field)
    {
        return m_Reader.GetFloat(m_Reader.GetOrdinal(Field));
    }
    public decimal GetDecimal(string Field)
    {
        return m_Reader.GetDecimal(m_Reader.GetOrdinal(Field));
    }


    public DateTime? GetDateTime(string Field)
    {
        if (m_Reader.IsDBNull(m_Reader.GetOrdinal(Field)))
        {
            return null;
        }
        else
            return m_Reader.GetDateTime(m_Reader.GetOrdinal(Field));
    }
    public bool GetBool(string Field)
    {
        return m_Reader.GetBoolean(m_Reader.GetOrdinal(Field));

    }

    //Now we can have none standard Id Fields! Yeahhhh 
    //useful when we are not the ones in control of the creation of the DB
    public string GetIdField()
    {
        return m_IdField;
    }
    #endregion

    #region Set Functions

    public void SetString(string Field, string str)
    {
        if (m_EditSet.ContainsKey(Field))
        {
            m_EditSet[Field] = str;
        }
        else
        {
            m_EditSet.Add(Field, str);
        }
    }

    public void SetLong(string Field, long l)
    {
        if (m_EditSet.ContainsKey(Field))
        {
            m_EditSet[Field] = l.ToString();
        }
        else
        {
            m_EditSet.Add(Field, l.ToString());
        }
    }

    //Now we can have none standard Id Fields! Yeahhhh 
    //useful when we are not the ones in control of the creation of the DB
    public void SetIdField(string id)
    {
        this.m_IdField = id;
    }

    #endregion

    public void Connect()
    {

        this.Connect("default");
    }

    public void Connect(string db)
    {

        try
        {
            switch (default_db[db]["db_type"])
            {
                case "odbc":
                    m_Mode = "odbc";

                    m_ConnectionString = "";
                    m_ConnectionString += "DSN=" + default_db[db]["dsn"] + ";";
                    m_ConnectionString += "uid=" + default_db[db]["uid"] + ";";
                    m_ConnectionString += "pwd=" + default_db[db]["pwd"] + ";";

                    m_Connection = new OdbcConnection(m_ConnectionString);
                    m_Connection.Open();

                    //Console.WriteLine(m_Mode);
                    //return true;
                    break;

                case "mssql":
                    m_Mode = "mssql";

                    m_ConnectionString = "";
                    m_ConnectionString += "Server=" + default_db[db]["host"] + ";";
                    m_ConnectionString += "Database=" + default_db[db]["dbname"] + ";";
                    m_ConnectionString += "User id=" + default_db[db]["username"] + ";";
                    m_ConnectionString += "Password=" + default_db[db]["password"] + ";";

                    m_Connection = new SqlConnection(m_ConnectionString);
                    m_Connection.Open();

                    //Console.WriteLine(m_Mode);
                    //return true;
                    break;
                default:
                    Error("Unknown DB type:" + default_db[db]["db_type"], "", "Connect");
                    break;
            }

        }
        catch (Exception ex)
        {
            Error(ex.Message, ex.StackTrace, "Connect");
            m_Connection = null;
            throw;
        }

        //return false;
    }

    public void Disconnect()
    {
        if (m_Connection != null)
        {
            m_Connection.Close();
            m_Connection = null;
        }
    }

    public void Open(string Sql)
    {
        Open(Sql, "Query");
    }

    public void Open(string Sql, string QueryMode)
    {
        Open(Sql, "Query", false);
    }

    public void Open(string Sql, string QueryMode, bool isStoredProcedure)
    {
        //Query Mode can be Query or NonQuery
        //The NonQuery SHouldn't be used if one can help it.
        //The NonQuery is used on sql statements that don't return results 
        //For the most part you should be able to use Update()-in conjunction with AddNee() or Edit()- and Delete()
        //for any functions that don't return results
        //I added this incase a need arised for this feature.

        //also added isStoredProcedure because I there is a change to be made when calling stored Procedure
        //with store procedure you only have to call the name of the function.
        try
        {
            IDbCommand Command = null;

            switch (m_Mode)
            {
                case "odbc":
                    Command = new OdbcCommand(Sql, m_Connection as OdbcConnection);
                    break;
                case "mssql":
                    Command = new SqlCommand(Sql, m_Connection as SqlConnection);
                    break;
                default:
                    Error("Unknown DB mode:" + m_Mode, "", "Open");
                    break;
            }
            if (isStoredProcedure)
                Command.CommandType = CommandType.StoredProcedure;


            if (QueryMode == "Query")
                m_Reader = Command.ExecuteReader();
            else if (QueryMode == "NonQuery")
                Command.ExecuteNonQuery();
            else
                Error("Unknown QueryMode.", "", "Open");

        }
        catch (Exception ex)
        {
            Error(ex.Message, ex.StackTrace, "Open");
            throw;
        }

        MoveNext();
    }

    public void Close()
    {
        if (m_Reader != null)
        {
            m_Reader.Close();
            m_Reader = null;
        }
    }

    public void MoveNext()
    {
        if (m_Reader != null)
        {
            m_Eof = !m_Reader.Read();
        }
        else
        {
            m_Eof = true;
        }
    }

    public bool IsEof()
    {
        return m_Eof;
    }

    public void AddNew(string Table)
    {
        m_EditTable = Table;
        m_EditSet = new Dictionary<string, string>();
        m_EditId = 0;
    }

    public void Edit(string Table)
    {
        m_EditTable = Table;
        m_EditSet = new Dictionary<string, string>();

        m_EditId = GetInt32(m_IdField);
    }


    /////////////////

    public void Update()
    {
        string Comma;

        if (m_EditId == 0)
        {
            try
            {

                IDbCommand Command = null;
                switch (m_Mode)
                {
                    case "odbc":
                        Command = new OdbcCommand();
                        break;
                    case "mssql":
                        Command = new SqlCommand();
                        break;
                    default:
                        Error("Unknown DB mode:" + m_Mode, "", "Update");
                        break;
                }

                Command.Connection = this.m_Connection;
                Command.CommandText = "";

                Command.CommandText += "INSERT INTO ";
                Command.CommandText += m_EditTable;
                Command.CommandText += " (";

                Comma = "";
                foreach (KeyValuePair<string, string> kvp in m_EditSet)
                {
                    Command.CommandText += Comma;
                    Command.CommandText += kvp.Key;
                    Comma = ",";
                }

                Command.CommandText += ") VALUES (";

                Comma = "";
                foreach (KeyValuePair<string, string> kvp in m_EditSet)
                {
                    Command.CommandText += Comma;
                    Command.CommandText += "'";
                    Command.CommandText += kvp.Value.Replace("'", "''");
                    Command.CommandText += "'";
                    Comma = ",";
                }

                Command.CommandText += ")";

                //compatibility for Npgsql - doesn't like an open datareader when calling ExecuteNonQuery()
                //So we close the datareader.
                //DataReader is opened in the Open Function.
                this.m_Reader.Close();

                Command.ExecuteNonQuery();

                this.m_Connection.Close();
                //return true;
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.StackTrace, "Update");
                throw;
            }
        }
        else
        {
            try
            {

                IDbCommand Command = null;
                switch (m_Mode)
                {
                    case "odbc":
                        Command = new OdbcCommand();
                        break;
                    case "mssql":
                        Command = new SqlCommand();
                        break;
                    default:
                        Error("Unknown DB mode:" + m_Mode, "", "m_Mode");
                        break;
                }

                Command.Connection = this.m_Connection;
                Command.CommandText = "";

                Command.CommandText += "UPDATE ";
                Command.CommandText += m_EditTable;
                Command.CommandText += " SET ";

                Comma = "";
                foreach (KeyValuePair<string, string> kvp in m_EditSet)
                {
                    Command.CommandText += Comma;
                    Command.CommandText += kvp.Key;
                    Command.CommandText += "='";
                    Command.CommandText += kvp.Value.Replace("'", "''");
                    Command.CommandText += "'";
                    Comma = ",";
                }

                Command.CommandText += " WHERE " + m_IdField + " = '";
                Command.CommandText += m_EditId.ToString();
                Command.CommandText += "'";

                //compatibility for Npgsql - doesn't like an open datareader when calling ExecuteNonQuery()
                //So we close the datareader.
                //DataReader is opened in the Open Function.
                this.m_Reader.Close();

                Command.ExecuteNonQuery();

                this.m_Connection.Close();
                //return true;
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.StackTrace, "Update");
                throw;
            }

        }
        //return false;
    }
    ///////////////////
    //not sure how to use this
    public void Delete(string Table)
    {
        try
        {
            IDbCommand Command = null;
            switch (m_Mode)
            {
                case "odbc":
                    Command = new OdbcCommand("DELETE FROM " + Table + " WHERE " + m_IdField + " = '" + GetInt32(m_IdField).ToString() + "'", this.m_Connection as OdbcConnection);
                    break;

                case "mssql":
                    Command = new SqlCommand("DELETE FROM " + Table + " WHERE " + m_IdField + " = '" + GetInt32(m_IdField).ToString() + "'", this.m_Connection as SqlConnection);
                    break;
                default:
                    Error("Unknown DB mode:" + m_Mode, "", "Delete");
                    break;

            }
            //compatibility for Npgsql - doesn't like an open datareader when calling ExecuteNonQuery()
            //So we close the datareader.
            //DataReader is opened in the Open Function.
            this.m_Reader.Close();

            Command.ExecuteNonQuery();

            this.m_Connection.Close();
            //return true;
        }
        catch (Exception ex)
        {
            Error(ex.Message, ex.StackTrace, "Delete");
            throw;
        }
        //return false;
    }
    //
    private void Error(string Message, string Stacktrace, string func)
    {
        string errorMessage = "";
        errorMessage += "RecordSet::" + func + "():";
        errorMessage += "Exception: " + Message;
        //errorMessage += "StackTrace: " + Stacktrace;
        Console.Write(errorMessage);

    }

    public string Escape(string value)
    {
        return value.Replace("'", "''");
    }
}
