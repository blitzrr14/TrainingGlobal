using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
using System.Web;
using log4net;
using System.Data;

/// <summary>
/// Summary description for Responses
/// </summary>
public class Responses
{

}


[DataContract]
public class DBConnection
{
    private MySqlConnection connection;
    private Boolean pool = false;
    String path;
    private static readonly ILog kplog = LogManager.GetLogger(typeof(DBConnection));
    //Constructor
    public DBConnection(String Serv, String DB, String UID, String Password, String pooling, Int32 maxcon, Int32 mincon, Int32 tout)
    {
        Initialize(Serv, DB, UID, Password, pooling, maxcon, mincon, tout);
    }

    //Initialize values
    private void Initialize(String Serv, String DB, String UID, String Password, String pooling, Int32 maxcon, Int32 mincon, Int32 tout)
    {
        try
        {
            if (pooling.Equals("1"))
            {
                pool = true;
            }

            string myconstring = "server = " + Serv + "; database = " + DB + "; uid = " + UID + ";password= " + Password + "; pooling=" + pool + ";min pool size=" + mincon + ";max pool size=" + maxcon + "; Connection Lifetime=0 ;Command Timeout=28800; connection timeout=" + tout + ";Allow Zero Datetime=true";
            connection = new MySqlConnection(myconstring);
        }
        catch (Exception ex)
        {
            kplog.Fatal("Unable to connect", ex);
            throw new Exception(ex.Message);
        }

    }

    public String Path
    {
        get { return path; }
        set { path = value; }
    }
    //open connection to database
    public bool OpenConnection()
    {
        try
        {
            connection.Open();
            return true;
        }
        catch (MySqlException)
        {
            //When handling errors, you can your application's response based 
            //on the error number.
            //The two most common error numbers when connecting are as follows:
            //0: Cannot connect to server.
            //1045: Invalid user name and/or password.
            return false;
        }
    }

    //Close connection
    public bool CloseConnection()
    {
        try
        {
            connection.Close();
            return true;
        }
        catch (MySqlException)
        {
            return false;
        }
    }

    public MySqlConnection getConnection()
    {
        return connection;
    }

    public void dispose()
    {
        connection.Dispose();
    }

}

[DataContract]
public class IniFile
{
    public string path;

    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section,
             string key, string def, StringBuilder retVal,
        int size, string filePath);


    public IniFile(string INIPath)
    {
        path = INIPath;
    }

    public void IniWriteValue(string Section, string Key, string Value)
    {
        WritePrivateProfileString(Section, Key, Value, this.path);
    }


    public string IniReadValue(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(255);
        int i = GetPrivateProfileString(Section, Key, "", temp,
                                        255, this.path);
        return temp.ToString();

    }
}


[DataContract]
public class SeccomResponse
{
    [DataMember]
    public int respcode { get; set; }
    [DataMember]
    public Int32 RowsCount { get; set; }
    [DataMember]
    public String message { get; set; }
    [DataMember]
    public String ErrorDetail { get; set; }
    [DataMember]
    public Boolean isFound { get; set; }
    [DataMember]
    public Boolean isSaved { get; set; }
    [DataMember]
    public String remarks { get; set; }
    [DataMember]
    public string status { get; set; }
    [DataMember]
    public string reason { get; set; }
    [DataMember]
    public String remarksrname { get; set; }
    [DataMember]
    public string reasonrname { get; set; }
    [DataMember]
    public Int32 percentage { get; set; }
    [DataMember]
    public List<TransList> data { get; set; }

}

public class TransList
{
    public String kptnno { get; set; }
    public String POAmount { get; set; }
    public String ZipCode { get; set; }
    public String City { get; set; }
    public String DateClaimed { get; set; }
    public String TransDate { get; set; }
    public String Principal { get; set; }
    public String Charge { get; set; }
    public String Othercharge { get; set; }
    public String Total { get; set; }
    public String ExchangeRate { get; set; }
    public String SenderFName { get; set; }
    public String SenderMName { get; set; }
    public String SenderLName { get; set; }
    public String ReceiverFName { get; set; }
    public String ReceiverMName { get; set; }
    public String ReceiverLName { get; set; }
    public String ReceiverStreet { get; set; }
    public String ReceiverProvinceCity { get; set; }
    public String ReceiverCountry { get; set; }
    public String SenderStreet { get; set; }
    public String SenderProvinceCity { get; set; }
    public String SenderCountry { get; set; }
    public String SenderBirthDate { get; set; }
    public String occupation { get; set; }
    public String IDType { get; set; }
    public String IDNo { get; set; }
    public String ExpiryDate { get; set; }
    public String SSN { get; set; }
    public String SenderContactNo { get; set; }
    public String ReceiverContactNo { get; set; }
    public String PaymentType { get; set; }
    public String TransType { get; set; }
    public String CancelledDate { get; set; }
    public String CancelReason { get; set; }
    public String Purpose { get; set; }
    public String Status { get; set; }
    //public String Email { get; set; }
}

public class FirstList
{

    public String kptn { get; set; }
    public String TableOriginated { get; set; }
    public String SenderFName { get; set; }
    public String SenderLName { get; set; }
    public String SenderMName { get; set; }
    public String TransDate { get; set; }
}

[DataContract]
public class TransReviewResponse
{
    [DataMember]
    public Int32 respcode { get; set; }
    [DataMember]
    public String respmsg { get; set; }
    [DataMember]
    public String errordetail { get; set; }
    [DataMember]
    public List<CTRList> data { get; set; }
}

[DataContract]
public class FTPResponse 
{
    [DataMember]
    public Int32 respcode { get; set; }
    [DataMember]
    public String respmsg { get; set; }
    [DataMember]
    public String errordetail { get; set; }
    [DataMember]
    public String Server { get; set; }
    [DataMember]
    public String Username { get; set; }
    [DataMember]
    public String Password { get; set; }

}


public class CTRList
{

    public String TransDate { get; set; }
    public String Name { get; set; }
    public String CustomerType { get; set; }
    public Double Amount { get; set; }
    public String DateFiled { get; set; }
    public String Resource { get; set; }
    public String ReviewedBy { get; set; }
    public String DateReviewed { get; set; }
    public String Status { get; set; }
    public String CompletedDate { get; set; }
    public String Reason { get; set; }
    public String KPTN { get; set; }
    public String SenderFullName { get; set; }
    public String ReceiverFullName { get; set; }
    public String SOBranch { get; set; }
    public String FileName { get; set;}
    public Boolean FileStat { get; set; }
    public String Email { get; set; }
    public String ReviewStatus { get; set; }
    public String Remark { get; set; }
   

}
