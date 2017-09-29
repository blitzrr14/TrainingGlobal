using System;
using System.Collections.Generic;
using System.Linq; 
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Security;
using System.IO;
using HttpUtils;
using Newtonsoft.Json;
using log4net;
using log4net.Config;


// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
    private String path, pathKiosk, pathBillspay;
    private DBConnection dbconnetwork,dbconCTR,dbconKiosk,dbconBillspay;
    private MySqlCommand command;
    private MySqlConnection connection;
    private static readonly ILog kplog = LogManager.GetLogger(typeof(Service));
    private String loginuser = "boswebserviceusr";
    private String loginpass = "boyursa805";
    private Int32 Counter = 0;
    private DateTime dt;
    private Int32 isUse365Global = 1;
    private String Brader = string.Empty;

    public Service() 
    { 

        path = "C:\\kpconfig\\globalconf.ini";
        pathKiosk = "C:\\kpconfig\\kpAPIGlobalConf.ini";
        pathBillspay = "C:\\kpconfig\\billspayGlobalConf.ini";


        IniFile ini = new IniFile(path);
        IniFile iniKiosk = new IniFile(pathKiosk);
        IniFile iniBillspay = new IniFile(pathBillspay);

        String Serv = ini.IniReadValue("DBConfig Transaction B", "server");
        String DB = ini.IniReadValue("DBConfig Transaction B", "database");
        String UID = ini.IniReadValue("DBConfig Transaction B", "uid");
        String Password = ini.IniReadValue("DBConfig Transaction B", "password");
        String pool = ini.IniReadValue("DBConfig Transaction B", "pool");
        Int32 maxcon = Convert.ToInt32(ini.IniReadValue("DBConfig Transaction B", "maxcon"));
        Int32 mincon = Convert.ToInt32(ini.IniReadValue("DBConfig Transaction B", "mincon"));
        Int32 tout = Convert.ToInt32(ini.IniReadValue("DBConfig Transaction B", "tout"));
        dbconnetwork = new DBConnection(Serv, DB, UID, Password, pool, maxcon, mincon, tout);

        String Serv2 = ini.IniReadValue("DBConfig Transaction", "server");
        String DB2 = ini.IniReadValue("DBConfig Transaction", "database");
        String UID2 = ini.IniReadValue("DBConfig Transaction", "uid");
        String Password2 = ini.IniReadValue("DBConfig Transaction", "password");
        String pool2 = ini.IniReadValue("DBConfig Transaction", "pool");
        Int32 maxcon2 = Convert.ToInt32(ini.IniReadValue("DBConfig Transaction", "maxcon"));
        Int32 mincon2 = Convert.ToInt32(ini.IniReadValue("DBConfig Transaction", "mincon"));
        Int32 tout2 = Convert.ToInt32(ini.IniReadValue("DBConfig Transaction", "tout"));
        dbconCTR = new DBConnection(Serv2, DB2, UID2, Password2, pool2, maxcon2, mincon2, tout2);

        String Serv3 = iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "server");
        String DB3 = iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "database");
        String UID3 = iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "uid");
        String Password3 = iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "password");
        String pool3 = iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "pool");
        Int32 maxcon3 = Convert.ToInt32(iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "maxcon"));
        Int32 mincon3 = Convert.ToInt32(iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "mincon"));
        Int32 tout3 = Convert.ToInt32(iniKiosk.IniReadValue("DBConfig KioskAPIGlobal B", "tout"));
        dbconKiosk = new DBConnection(Serv3, DB3, UID3, Password3, pool3, maxcon3, mincon3, tout3);

        String Serv4 = iniBillspay.IniReadValue("DBConfig Transaction", "server");
        String DB4 = iniBillspay.IniReadValue("DBConfig Transaction", "database");
        String UID4 = iniBillspay.IniReadValue("DBConfig Transaction", "uid");
        String Password4 = iniBillspay.IniReadValue("DBConfig Transaction", "password");
        String pool4 = iniBillspay.IniReadValue("DBConfig Transaction", "pool");
        Int32 maxcon4 = Convert.ToInt32(iniBillspay.IniReadValue("DBConfig Transaction", "maxcon"));
        Int32 mincon4 = Convert.ToInt32(iniBillspay.IniReadValue("DBConfig Transaction", "mincon"));
        Int32 tout4 = Convert.ToInt32(iniBillspay.IniReadValue("DBConfig Transaction", "tout"));
        dbconBillspay = new DBConnection(Serv4, DB4, UID4, Password4, pool4, maxcon4, mincon4, tout4);

        log4net.Config.XmlConfigurator.Configure();
    }



    #region Transactional Review


    //Modified by: Rr
    //Date : 07-20-2016
    //Description: Added Kiosk Transactions
    //Done : 07-20-2016

    //Modified by: Rr
    //Date : 10-12-16
    //Description: Added Kiosk Transactions
    //Done : 10-12-16
    public SeccomResponse getTransReviewByTotalAmount(String Username, String Password, String DateTo, String DateFrom, String AmountMin, String AmountMax, Boolean WOCancel)
     {
        try
        {

            if (Username != loginuser || Password != loginpass)
            {
                return new SeccomResponse { respcode = 7, message = getRespMessage(7) };
            }

            List<TransList> tl = new List<TransList>();
            String HomeCity = string.Empty;
            String ZipCode = string.Empty;
            String Occupation = string.Empty;
            String SSN = string.Empty;
            String Status = string.Empty;
            String KPTNNO = string.Empty;
            String TransDate = string.Empty;
            String AmountPO = string.Empty;
            String DateClaimed = string.Empty;
            String Principal = string.Empty;
            String Charge = string.Empty;
            String OtherCharge = string.Empty;
            String Total = string.Empty;
            String ExchangeRate = string.Empty;
            String SenderFName = string.Empty;
            String SenderMName = string.Empty;
            String SenderLName = string.Empty;
            String ReceiverFName = string.Empty;
            String ReceiverMName = string.Empty;
            String ReceiverLName = string.Empty;
            String ReceiverStreet = string.Empty;
            String ReceiverProvinceCity = string.Empty;
            String ReceiverCountry = string.Empty;
            String SenderStreet = string.Empty;
            String SenderProvinceCity = string.Empty;
            String SenderCountry = string.Empty;
            String SenderBirthDate = string.Empty;
            String IDType = string.Empty;
            String IDNo = string.Empty;
            String ExpiryDate = string.Empty;
            String SenderContactNo = string.Empty;
            String ReceiverContactNo = string.Empty;
            String PaymentType = string.Empty;
            String TransType = string.Empty;
            String CancelledDate = string.Empty;
            String CancelReason = string.Empty;
            String Purpose = string.Empty;
            String xTableName = string.Empty;
            String xTableNameD2B = string.Empty;
            Boolean translogs = false;
            Boolean in365 = false;
            Int32 xcounter = 0;
      

            using (MySqlConnection con = dbconnetwork.getConnection())
            {
                DateTime DateNow = getServerDateGlobal(false);
                con.Open();
                using (MySqlCommand command = con.CreateCommand())
                {
                    try
                    {

                        if (AmountMin == null || AmountMax == null)
                        {

                            return new SeccomResponse { respcode = 0, message = "AmountMin and AmountMax must have Value!" };
                        }
                        DataTable dt = new DataTable();
                        DataSet ds = new DataSet();
                        List<FirstList> listKptn = new List<FirstList>();
                        String[] custArr = new String[7];
                      
                        String from = (DateFrom == "" || DateFrom == null ? DateNow.AddMonths(-6).ToString("yyyy-MM-dd 00:00:00") : (Convert.ToDateTime(DateFrom)).ToString("yyyy-MM-dd 00:00:00"));
                        String to = (DateTo == "" || DateTo == null ? DateNow.ToString("yyyy-MM-dd 23:59:59") : (Convert.ToDateTime(DateTo)).ToString("yyyy-MM-dd 23:59:59"));
                      
                     //   String dto = Convert.ToDateTime(DateTo).ToString("MM");
                        String yrdateto = to.Substring(0, 4);
                        String yrdatefrom = from.Substring(0, 4);
                        String df = Convert.ToDateTime(to).AddMonths(-6).ToString("MM");
                  

                        int rr = Convert.ToInt32(df);
                        
                        for (int z = 0; z <= 6; z++) 
                        {
                            String xrr = rr.ToString();
                            if (rr < 10)
                                 xrr = "0" + xrr;


                            custArr[z] = xrr;
                            
                             if (rr % 12 == 0) 
                            {
                                rr = 1;
                                rr--;
                            }

                             rr++;
                        
                        }


                       

                            for (int i = 0; i < custArr.Length; i++)
                            {



                                command.Parameters.Clear();
                              // command.CommandText = "select kptnno,TableOriginated from kptransactionsglobal.sendout" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "')  and (year(TransDate) between  '"+ yrdatefrom + "' and '" + yrdateto + "') GROUP BY kptnno ORDER BY TransDate;";

                               command.CommandText = "select kptnno,TableOriginated from kptransactionsglobal.sendout" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "')  and (TransDate between  '"+ from + "' and '" + to + "') GROUP BY kptnno ORDER BY TransDate;";
                                MySqlDataReader rdr2 = command.ExecuteReader();

                                while (rdr2.Read())
                                {
                                    listKptn.Add(new FirstList
                                    {
                                        kptn = rdr2["KPTNNO"].ToString(),
                                        TableOriginated = rdr2["TableOriginated"].ToString(),
                                       
                                    });


                                }


                                rdr2.Close();
                            }

                            for (int i = 0; i < custArr.Length; i++)
                            {



                                command.Parameters.Clear();
                               // command.CommandText = "select kptnno,TableOriginated from kptransactionsglobal.sendoutd2b" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "') and (year(TransDate) between '" + yrdatefrom + "' and '" + yrdateto + "')  GROUP BY kptnno ORDER BY TransDate;";

                                command.CommandText = "select kptnno,TableOriginated from kptransactionsglobal.sendoutd2b" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "') and (TransDate between '" + from + "' and '" + to + "')  GROUP BY kptnno ORDER BY TransDate;";
                                MySqlDataReader rdr2 = command.ExecuteReader();

                                while (rdr2.Read())
                                {
                                    listKptn.Add(new FirstList
                                    {
                                        kptn = rdr2["KPTNNO"].ToString(),
                                        TableOriginated = rdr2["TableOriginated"].ToString(),

                                    });


                                }


                                rdr2.Close();
                            }

                            con.Close();
                            for (int i = 0; i < custArr.Length; i++)
                            {

                                using(MySqlConnection conK = dbconKiosk.getConnection())
                                {
                                    conK.Open();
                                    using(MySqlCommand cmd = conK.CreateCommand())
                                    {
                                        cmd.Parameters.Clear();
                                        //command.CommandText = "select kptnno,TableOriginated from kptransactionsglobal.sendoutd2b" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "') and (year(TransDate) between '" + yrdatefrom + "' and '" + yrdateto + "')  GROUP BY kptnno ORDER BY TransDate;";

                                        cmd.CommandText = "select kptnno,TableOriginated from kpkiosktransactionsglobal.sendout" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "') and (TransDate between '" + from + "' and '" + to + "')  GROUP BY kptnno ORDER BY TransDate;";
                                        MySqlDataReader rdr2 = cmd.ExecuteReader();

                                        while (rdr2.Read())
                                        {
                                            listKptn.Add(new FirstList
                                            {
                                                kptn = rdr2["KPTNNO"].ToString(),
                                                TableOriginated = rdr2["TableOriginated"].ToString(),

                                            });


                                        }


                                        rdr2.Close();
                                    }
                                }

                               
                            }

                            for (int i = 0; i < custArr.Length; i++)
                            {

                                using (MySqlConnection conB = dbconBillspay.getConnection())
                                {
                                    conB.Open();
                                    using (MySqlCommand cmd = conB.CreateCommand())
                                    {
                                        cmd.Parameters.Clear();
                                        //command.CommandText = "select kptnno,TableOriginated from kptransactionsglobal.sendoutd2b" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "') and (year(TransDate) between '" + yrdatefrom + "' and '" + yrdateto + "')  GROUP BY kptnno ORDER BY TransDate;";

                                        cmd.CommandText = "select kptn as kptnno,TableOriginated from kptransactionsglobal.sendoutbillspay" + custArr[i] + " where (Total Between '" + AmountMin + "' and '" + AmountMax + "') and (TransDate between '" + from + "' and '" + to + "')  GROUP BY kptnno ORDER BY TransDate;";
                                        MySqlDataReader rdr2 = cmd.ExecuteReader();

                                        while (rdr2.Read())
                                        {
                                            listKptn.Add(new FirstList
                                            {
                                                kptn = rdr2["KPTNNO"].ToString(),
                                                TableOriginated = rdr2["TableOriginated"].ToString(),

                                            });


                                        }

                                        
                                        rdr2.Close();
                                    }
                                    conB.Close();
                                }


                            }
                            con.Open();

                           
                            for (int x = 0; x < listKptn.Count; x++)
                            {




                                if (listKptn[x].kptn.Substring(0, 3) == "BPG")
                                {
                                    TransList response = new TransList();
                                    response = getBillspay(listKptn[x].kptn, AmountMin, AmountMax, WOCancel);

                                    if (response.kptnno != null)
                                    {
                                        tl.Add(response);

                                    }
                                    continue;
                                }



                                translogs = false;
                                if (listKptn[x].kptn == "" && listKptn[x].TableOriginated == "")
                                {
                                    continue;
                                }

                                command.Parameters.Clear();
                                command.CommandText = "select `action` from kpadminlogsglobal.transactionlogs where kptnno='" + listKptn[x].kptn + "' and `action` NOT IN ('PEEP','PO REPRINT','SO REPRINT') order by `timestamp` desc limit 1";
                                MySqlDataReader statusRdr = command.ExecuteReader();
                                if (!statusRdr.HasRows)
                                {
                                   
                                    translogs = true;
                                }
                                else
                                {
                                    statusRdr.Read();
                                    Status = statusRdr["action"].ToString();
                                  
                                   
                                }
                                statusRdr.Close();

                                Boolean isd2b = false; 

                                using (MySqlCommand cmd2 = con.CreateCommand())
                                {
                                    cmd2.Parameters.Clear();

                                  //  xTableName = decodeKPTNGlobal(0, listKptn[x].kptn);

                                    string tablesendout = listKptn[x].TableOriginated.Substring(7, 3);
                                    if (tablesendout != "d2b")
                                    {
                                        cmd2.Parameters.Clear();
                                        cmd2.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                              "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                               "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                               "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                               "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                               "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                               "FROM kpglobal." + listKptn[x].TableOriginated + " where KPTNNO = '" + listKptn[x].kptn + "' AND (TransDate BETWEEN '" + from + "' AND '" + to + "'); ";
                                    }
                                    else
                                    {
                                        cmd2.Parameters.Clear();
                                        cmd2.CommandText = "select RejectedDate, KPTNNO, TransDate, AmountPO, " +
                                                          "CompletedDate as DateClaimed, Principal, Charge, " +
                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                           "SenderContactNo,ReceiverContactNo,PaymentType,'INTERNATIONAL' as TransType,CancelledDate,CancelReason,Purpose " +
                                                           "FROM " + listKptn[x].TableOriginated + " where KPTNNO = '" + listKptn[x].kptn + "' AND (TransDate BETWEEN '" + from + "' AND '" + to + "');";

                                        isd2b = true;
                                    }
                                  
                                        MySqlDataReader transRdr = cmd2.ExecuteReader();

                                        if (transRdr.HasRows && translogs == false && isd2b==false)
                                        {
                                            transRdr.Read();
                                            xcounter++;
                                            Int32 isCancelled = Convert.ToInt32(transRdr["isCancelled"]);

                                            if (WOCancel == true && isCancelled == 1) { transRdr.Close(); continue; }


                                            KPTNNO = transRdr["KPTNNO"].ToString();
                                            TransDate = transRdr["TransDate"].ToString();
                                            AmountPO = transRdr["AmountPO"].ToString();
                                            DateClaimed = transRdr["DateClaimed"].ToString();
                                            Principal = transRdr["Principal"].ToString();
                                            Charge = transRdr["Charge"].ToString();
                                            OtherCharge = transRdr["OtherCharge"].ToString();
                                            Total = transRdr["Total"].ToString();
                                            ExchangeRate = transRdr["ExchangeRate"].ToString();
                                            SenderFName = transRdr["SenderFName"].ToString();
                                            SenderMName = transRdr["SenderMName"].ToString();
                                            SenderLName = transRdr["SenderLName"].ToString();
                                            ReceiverFName = transRdr["ReceiverFName"].ToString();
                                            ReceiverMName = transRdr["ReceiverMName"].ToString();
                                            ReceiverLName = transRdr["ReceiverLName"].ToString();
                                            ReceiverStreet = transRdr["ReceiverStreet"].ToString();
                                            ReceiverProvinceCity = transRdr["ReceiverProvinceCity"].ToString();
                                            ReceiverCountry = transRdr["ReceiverCountry"].ToString();
                                            SenderStreet = transRdr["SenderStreet"].ToString();
                                            SenderProvinceCity = transRdr["SenderProvinceCity"].ToString();
                                            SenderCountry = transRdr["SenderCountry"].ToString();
                                            SenderBirthDate = transRdr["SenderBirthDate"].ToString();
                                            IDType = transRdr["IDType"].ToString();
                                            IDNo = transRdr["IDNo"].ToString();
                                            ExpiryDate = transRdr["ExpiryDate"].ToString();
                                            SenderContactNo = transRdr["SenderContactNo"].ToString();
                                            ReceiverContactNo = transRdr["ReceiverContactNo"].ToString();
                                            PaymentType = transRdr["PaymentType"].ToString();
                                            TransType = transRdr["TransType"].ToString();
                                            CancelledDate = transRdr["CancelledDate"].ToString();
                                            CancelReason = transRdr["CancelReason"].ToString();
                                            Purpose = transRdr["Purpose"].ToString();


                                            if (SenderBirthDate == "0000-00-00" || SenderBirthDate == "00/00/0000" || SenderBirthDate == "00-00-0000" || SenderBirthDate == "0/0/0000") 
                                            {
                                                SenderBirthDate = "";
                                            }

                                            if (ExpiryDate == "0000-00-00" || ExpiryDate == "00/00/0000" || ExpiryDate == "00-00-0000" || ExpiryDate == "0/0/0000") 
                                            {
                                                ExpiryDate = "";
                                            }

                                            transRdr.Close();
                                            String BDate = SenderBirthDate;
                                            command.Parameters.Clear();
                                            command.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN,c.Mobile from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.FirstName = @FName and c.LastName = @LName and c.MiddleName = @MName and DATE_FORMAT(c.Birthdate,'%Y-%m-%d') = @BDate;";
                                            command.Parameters.AddWithValue("FName", SenderFName);
                                            command.Parameters.AddWithValue("LName", SenderLName);
                                            command.Parameters.AddWithValue("MName", SenderMName);
                                            command.Parameters.AddWithValue("BDate",BDate);
                                            MySqlDataReader custRdr = command.ExecuteReader();

                                            if (!custRdr.HasRows)
                                            {
                                                HomeCity = "";
                                                ZipCode = "";
                                                Occupation = "";
                                                SSN = "";
                                            }
                                            else
                                            {
                                                custRdr.Read();
                                                HomeCity = custRdr["HomeCity"].ToString();
                                                ZipCode = custRdr["ZipCode"].ToString();
                                                Occupation = custRdr["Occupation"].ToString();
                                                SSN = custRdr["SSN"].ToString();
                                                SenderContactNo = custRdr["Mobile"].ToString();
                                            }
                                            custRdr.Close();

                                            tl.Add(new TransList
                                            {

                                                kptnno = KPTNNO,
                                                TransDate = TransDate,
                                                POAmount = AmountPO,
                                                DateClaimed = DateClaimed,
                                                Principal = Principal,
                                                Charge = Charge,
                                                Othercharge = OtherCharge,
                                                Total = Total,
                                                ExchangeRate = ExchangeRate,
                                                SenderFName = SenderFName,
                                                SenderMName = SenderMName,
                                                SenderLName = SenderLName,
                                                ReceiverFName = ReceiverFName,
                                                ReceiverMName = ReceiverMName,
                                                ReceiverLName = ReceiverLName,
                                                ReceiverStreet = ReceiverStreet,
                                                ReceiverProvinceCity = ReceiverProvinceCity,
                                                ReceiverCountry = ReceiverCountry,
                                                SenderStreet = SenderStreet,
                                                SenderProvinceCity = SenderProvinceCity,
                                                SenderCountry = SenderCountry,
                                                SenderBirthDate = SenderBirthDate,
                                                IDType = IDType,
                                                IDNo = IDNo,
                                                ExpiryDate = ExpiryDate,
                                                SenderContactNo = SenderContactNo,
                                                ReceiverContactNo = ReceiverContactNo,
                                                PaymentType = PaymentType,
                                                TransType = TransType,
                                                CancelledDate = CancelledDate,
                                                CancelReason = CancelReason,
                                                Purpose = Purpose,
                                                City = HomeCity,
                                                occupation = Occupation,
                                                SSN = SSN,
                                                Status = Status,
                                                ZipCode = ZipCode

                                            });

                                        }
                                    

                                    else
                                    {
                                        transRdr.Close();
                                        // xTableNameD2B = decodeKPTNGlobald2b(0, listKptn[x].kptn);
                                            
                                      
                                        MySqlDataReader d2brdr = cmd2.ExecuteReader();


                                        if (d2brdr.HasRows && translogs == false)
                                        {
                                            d2brdr.Read();


                                            xcounter++;

                                            String RejectedDate = d2brdr["RejectedDate"].ToString();
                                            CancelledDate = d2brdr["CancelledDate"].ToString();

                                            if (WOCancel == true && (RejectedDate != "" || CancelledDate != ""))
                                            {
                                                d2brdr.Close(); continue;
                                            }

                                            KPTNNO = d2brdr["KPTNNO"].ToString();
                                            TransDate = d2brdr["TransDate"].ToString();
                                            AmountPO = d2brdr["AmountPO"].ToString();
                                            DateClaimed = d2brdr["DateClaimed"].ToString();
                                            Principal = d2brdr["Principal"].ToString();
                                            Charge = d2brdr["Charge"].ToString();
                                            OtherCharge = d2brdr["OtherCharge"].ToString();
                                            Total = d2brdr["Total"].ToString();
                                            ExchangeRate = d2brdr["ExchangeRate"].ToString();
                                            SenderFName = d2brdr["SenderFName"].ToString();
                                            SenderMName = d2brdr["SenderMName"].ToString();
                                            SenderLName = d2brdr["SenderLName"].ToString();
                                            ReceiverFName = d2brdr["ReceiverFName"].ToString();
                                            ReceiverMName = d2brdr["ReceiverMName"].ToString();
                                            ReceiverLName = d2brdr["ReceiverLName"].ToString();
                                            ReceiverStreet = d2brdr["ReceiverStreet"].ToString();
                                            ReceiverProvinceCity = d2brdr["ReceiverProvinceCity"].ToString();
                                            ReceiverCountry = d2brdr["ReceiverCountry"].ToString();
                                            SenderStreet = d2brdr["SenderStreet"].ToString();
                                            SenderProvinceCity = d2brdr["SenderProvinceCity"].ToString();
                                            SenderCountry = d2brdr["SenderCountry"].ToString();
                                            SenderBirthDate = d2brdr["SenderBirthDate"].ToString();
                                            IDType = d2brdr["IDType"].ToString();
                                            IDNo = d2brdr["IDNo"].ToString();
                                            ExpiryDate = d2brdr["ExpiryDate"].ToString();
                                            SenderContactNo = d2brdr["SenderContactNo"].ToString();
                                            ReceiverContactNo = d2brdr["ReceiverContactNo"].ToString();
                                            PaymentType = d2brdr["PaymentType"].ToString();
                                            TransType = d2brdr["TransType"].ToString();
                                         //   CancelledDate = d2brdr["CancelledDate"].ToString();
                                            CancelReason = d2brdr["CancelReason"].ToString();
                                            Purpose = d2brdr["Purpose"].ToString();

                                            d2brdr.Close();

                                        }
                                        else
                                        {
                                            d2brdr.Close();
                                            con.Close();
                                            using (MySqlConnection conkiosk = dbconKiosk.getConnection())
                                            {
                                                conkiosk.Open();
                                                using(MySqlCommand cmd = conkiosk.CreateCommand())
                                                {

                                                    String xTableKiosk = decodeKPTNGlobalKiosk(listKptn[x].kptn);
                                                    cmd.Parameters.Clear();
                                                    cmd.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                                          "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                                           "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                                           "FROM " + xTableKiosk + " where KPTNNO = '" + listKptn[x].kptn + "' AND (TransDate BETWEEN '" + from + "' AND '" + to + "'); ";

                                                    MySqlDataReader rdrK = cmd.ExecuteReader();

                                                    if (rdrK.HasRows && translogs == false)
                                                    {

                                                        rdrK.Read();
                                                        xcounter++;
                                                        Int32 isCancelled = Convert.ToInt32(rdrK["isCancelled"]);

                                                        if (WOCancel == true && isCancelled == 1) { rdrK.Close(); conkiosk.Close(); con.Open(); continue; }


                                                        KPTNNO = rdrK["KPTNNO"].ToString();
                                                        TransDate = rdrK["TransDate"].ToString();
                                                        AmountPO = rdrK["AmountPO"].ToString();
                                                        DateClaimed = rdrK["DateClaimed"].ToString();
                                                        Principal = rdrK["Principal"].ToString();
                                                        Charge = rdrK["Charge"].ToString();
                                                        OtherCharge = rdrK["OtherCharge"].ToString();
                                                        Total = rdrK["Total"].ToString();
                                                        ExchangeRate = rdrK["ExchangeRate"].ToString();
                                                        SenderFName = rdrK["SenderFName"].ToString();
                                                        SenderMName = rdrK["SenderMName"].ToString();
                                                        SenderLName = rdrK["SenderLName"].ToString();
                                                        ReceiverFName = rdrK["ReceiverFName"].ToString();
                                                        ReceiverMName = rdrK["ReceiverMName"].ToString();
                                                        ReceiverLName = rdrK["ReceiverLName"].ToString();
                                                        ReceiverStreet = rdrK["ReceiverStreet"].ToString();
                                                        ReceiverProvinceCity = rdrK["ReceiverProvinceCity"].ToString();
                                                        ReceiverCountry = rdrK["ReceiverCountry"].ToString();
                                                        SenderStreet = rdrK["SenderStreet"].ToString();
                                                        SenderProvinceCity = rdrK["SenderProvinceCity"].ToString();
                                                        SenderCountry = rdrK["SenderCountry"].ToString();
                                                        SenderBirthDate = rdrK["SenderBirthDate"].ToString();
                                                        IDType = rdrK["IDType"].ToString();
                                                        IDNo = rdrK["IDNo"].ToString();
                                                        ExpiryDate = rdrK["ExpiryDate"].ToString();
                                                        SenderContactNo = rdrK["SenderContactNo"].ToString();
                                                        ReceiverContactNo = rdrK["ReceiverContactNo"].ToString();
                                                        PaymentType = rdrK["PaymentType"].ToString();
                                                        TransType = rdrK["TransType"].ToString();
                                                        CancelledDate = rdrK["CancelledDate"].ToString();
                                                        CancelReason = rdrK["CancelReason"].ToString();
                                                        Purpose = rdrK["Purpose"].ToString();


                                                    }
                                                    else 
                                                    {
                                                        rdrK.Close(); conkiosk.Close(); con.Open(); continue;
                                                    }
                                                }
                                            
                                            }
                                            con.Open();

                                            
                                        }

                                        d2brdr.Close();

                                        String BDate = SenderBirthDate == "" ? String.Empty : Convert.ToDateTime(SenderBirthDate).ToString("yyyy-MM-dd");
                                        command.Parameters.Clear();
                                        command.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN,c.Mobile from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.FirstName = @FName and c.LastName = @LName and c.MiddleName = @MName and DATE_FORMAT(c.Birthdate,'%Y-%m-%d') = @BDate;";
                                        command.Parameters.AddWithValue("FName", SenderFName);
                                        command.Parameters.AddWithValue("LName", SenderLName);
                                        command.Parameters.AddWithValue("MName", SenderMName);
                                        command.Parameters.AddWithValue("BDate", BDate);
                                        MySqlDataReader custRdr = command.ExecuteReader();

                                        if (!custRdr.HasRows)
                                        {
                                            HomeCity = "";
                                            ZipCode = "";
                                            Occupation = "";
                                            SSN = "";
                                        }
                                        else
                                        {
                                            custRdr.Read();
                                            HomeCity = custRdr["HomeCity"].ToString();
                                            ZipCode = custRdr["ZipCode"].ToString();
                                            Occupation = custRdr["Occupation"].ToString();
                                            SSN = custRdr["SSN"].ToString();
                                            SenderContactNo = custRdr["Mobile"].ToString();
                                        }
                                        custRdr.Close();

                                        tl.Add(new TransList
                                        {

                                            kptnno = KPTNNO,
                                            TransDate = TransDate,
                                            POAmount = AmountPO,
                                            DateClaimed = DateClaimed,
                                            Principal = Principal,
                                            Charge = Charge,
                                            Othercharge = OtherCharge,
                                            Total = Total,
                                            ExchangeRate = ExchangeRate,
                                            SenderFName = SenderFName,
                                            SenderMName = SenderMName,
                                            SenderLName = SenderLName,
                                            ReceiverFName = ReceiverFName,
                                            ReceiverMName = ReceiverMName,
                                            ReceiverLName = ReceiverLName,
                                            ReceiverStreet = ReceiverStreet,
                                            ReceiverProvinceCity = ReceiverProvinceCity,
                                            ReceiverCountry = ReceiverCountry,
                                            SenderStreet = SenderStreet,
                                            SenderProvinceCity = SenderProvinceCity,
                                            SenderCountry = SenderCountry,
                                            SenderBirthDate = SenderBirthDate,
                                            IDType = IDType,
                                            IDNo = IDNo,
                                            ExpiryDate = ExpiryDate,
                                            SenderContactNo = SenderContactNo,
                                            ReceiverContactNo = ReceiverContactNo,
                                            PaymentType = PaymentType,
                                            TransType = TransType,
                                            CancelledDate = CancelledDate,
                                            CancelReason = CancelReason,
                                            Purpose = Purpose,
                                            City = HomeCity,
                                            occupation = Occupation,
                                            SSN = SSN,
                                            Status = Status,
                                            ZipCode = ZipCode

                                        });

                                    }
                                  
                                }
                            }



                    if(listKptn.Count == 0)
                    {
                        return new SeccomResponse { respcode = 0, message = "No Data Found!", data = null };
                    }

                  

                        con.Close();
                        return new SeccomResponse { respcode = 1, message = "Success!", data = new List<TransList>(tl), RowsCount = tl.Count };

                        
                      
                    

                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
                    }


                }

            }

        }
        catch (Exception ex)
        {
            dbconnetwork.CloseConnection();
            return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
        }

    }

    //Modified by: Rr
    //Date : 07-19-2016
    //Description: Added Kiosk Transactions
    //Done : 07-19-2016

    //Modified by: Rr
    //Date : 10-08-2016
    //Description: Added Billspay Transactions
    //Done : Ongoing


    public SeccomResponse getTransReviewByName(String Username, String Password, String FirstName, String MiddleName, String LastName, String DateFrom, String DateTo, String AmountMin, String AmountMax, String CustType, Boolean WOCancel)
    {
        try
        {

            if (Username != loginuser || Password != loginpass)
            {
                return new SeccomResponse { respcode = 7, message = getRespMessage(7) };
            }

            List<TransList> tl = new List<TransList>();
            String HomeCity = string.Empty;
            String ZipCode = string.Empty;
            String Occupation = string.Empty;
            String SSN = string.Empty;
            String Status = string.Empty;
            String KPTNNO = string.Empty;
            String TransDate = string.Empty;
            String AmountPO = string.Empty;
            String DateClaimed = string.Empty;
            String Principal = string.Empty;
            String Charge = string.Empty;
            String OtherCharge = string.Empty;
            String Total = string.Empty;
            String ExchangeRate = string.Empty;
            String SenderFName = string.Empty;
            String SenderMName = string.Empty;
            String SenderLName = string.Empty;
            String ReceiverFName = string.Empty;
            String ReceiverMName = string.Empty;
            String ReceiverLName = string.Empty;
            String ReceiverStreet = string.Empty;
            String ReceiverProvinceCity = string.Empty;
            String ReceiverCountry = string.Empty;
            String SenderStreet = string.Empty;
            String SenderProvinceCity = string.Empty;
            String SenderCountry = string.Empty;
            String SenderBirthDate = string.Empty;
            String IDType = string.Empty;
            String IDNo = string.Empty;
            String ExpiryDate = string.Empty;
            String SenderContactNo = string.Empty;
            String ReceiverContactNo = string.Empty;
            String PaymentType = string.Empty;
            String TransType = string.Empty;
            String CancelledDate = string.Empty;
            String CancelReason = string.Empty;
            String Purpose = string.Empty;
            String xTableName = string.Empty;
            String xTableNameD2B = string.Empty;
            Boolean translogs = false;
            Boolean in365 = false;
            Int32 xcounter = 0;

            using (MySqlConnection con = dbconnetwork.getConnection())
            {
                DateTime DateNow = getServerDateGlobal(false);
             
                using (MySqlCommand command = con.CreateCommand())
                {
                    try
                    {

                       
                        DataTable dt = new DataTable();
                        DataSet ds = new DataSet();
                        List<FirstList> listKptn = new List<FirstList>();
                        String[] custArr = new String[9];
                        custArr[0] = "customerAtoC";
                        custArr[1] = "customerDtoF";
                        custArr[2] = "customerGtoI";
                        custArr[3] = "customerJtoL";
                        custArr[4] = "customerMtoO";
                        custArr[5] = "customerPtoR";
                        custArr[6] = "customerStoU";
                        custArr[7] = "customerVtoX";
                        custArr[8] = "customerYtoZ";
                        String from = (DateFrom == "" || DateFrom == null ? DateNow.AddMonths(-6).ToString("yyyy-MM-dd 00:00:00") : (Convert.ToDateTime(DateFrom)).ToString("yyyy-MM-dd 00:00:00"));
                        String to = (DateTo == "" || DateTo == null ? DateNow.ToString("yyyy-MM-dd 23:59:59") : (Convert.ToDateTime(DateTo)).ToString("yyyy-MM-dd 23:59:59"));
                        String query;
                        String queryPhoneNo = string.Empty;
                        Int32 i = 0;
                        if (MiddleName == null) MiddleName = "";
                        if(AmountMin == null) AmountMin = "";

                        if(MiddleName != "")
                        {

                           query = "where FirstName = @FirstName and LastName = @LastName And MiddleName = @MiddleName  AND CustomerType = '"+CustType+"' AND (TransDate between '"+from+"' and '"+to+"') GROUP BY kptn ORDER BY TransDate DESC;";
                           queryPhoneNo = "select Mobile From kpcustomersglobal.customers where FirstName = @FirstName and LastName = @LastName AND MiddleName = @MiddleName;";
                        }
                        else
                        {
                            query = "where FirstName = @FirstName and LastName = @LastName AND CustomerType = '" + CustType + "' AND (TransDate between '" + from + "' and '" + to + "') GROUP BY kptn ORDER BY TransDate DESC;";
                            queryPhoneNo = "select Mobile From kpcustomersglobal.customers where FirstName = @FirstName and LastName = @LastName;";
                        }

                     
                            String custAtoZ = getcustomertable(LastName);

                            con.Open();

                            command.Parameters.Clear();
                            command.CommandText = "Select TransDate, kptn from kpadminlogsglobal.customer" + custAtoZ + " " + query;
                            command.Parameters.AddWithValue("AmountMin", AmountMin);
                            command.Parameters.AddWithValue("AmountMax", AmountMax);
                            command.Parameters.AddWithValue("DateFrom", from);
                            command.Parameters.AddWithValue("DateTo", to);
                            command.Parameters.AddWithValue("FirstName", FirstName);
                            command.Parameters.AddWithValue("MiddleName", MiddleName);
                            command.Parameters.AddWithValue("LastName", LastName);
                            MySqlDataReader rdr2 = command.ExecuteReader();

                            while (rdr2.Read())
                            {
                                listKptn.Add(new FirstList
                                {
                                    kptn = rdr2["KPTN"].ToString(),
                                    TransDate = rdr2["TransDate"].ToString()
                                });


                            }

                            con.Close();
                            rdr2.Close();
                        


                        //for ( i = 0; i < custArr.Length; i++)
                        //{

                        //    String custAtoZ = getcustomertable(LastName);

                        //    con.Open();

                        //    command.Parameters.Clear();
                        //    command.CommandText = "Select TransDate, kptn from kpadminlogsglobal." + custArr[i] + " " + query;
                        //    command.Parameters.AddWithValue("AmountMin", AmountMin);
                        //    command.Parameters.AddWithValue("AmountMax", AmountMax);
                        //    command.Parameters.AddWithValue("DateFrom", from);
                        //    command.Parameters.AddWithValue("DateTo", to);
                        //    command.Parameters.AddWithValue("FirstName",FirstName);
                        //    command.Parameters.AddWithValue("MiddleName", MiddleName);
                        //    command.Parameters.AddWithValue("LastName", LastName);
                        //    MySqlDataReader rdr2 = command.ExecuteReader();

                        //    while (rdr2.Read())
                        //    {
                        //        listKptn.Add(new FirstList
                        //        {
                        //            kptn = rdr2["KPTN"].ToString(),
                        //            TransDate = rdr2["TransDate"].ToString()
                        //        });


                        //    }

                        //    con.Close();
                        //    rdr2.Close();
                        //}
                        
                       
                        for (int x = 0; x < listKptn.Count; x++)
                        {


                            if (listKptn[x].kptn.Substring(0, 3) == "BPG") 
                            {
                                TransList response = new TransList();
                                response = getBillspay(listKptn[x].kptn, AmountMin, AmountMax, WOCancel);

                                if (response != null)
                                {
                                    tl.Add(response);
                               
                                }
                                continue;
                            }


                            translogs = false;
                            if (con.State == ConnectionState.Closed)
                            {
                                con.Open();
                            }

                            command.Parameters.Clear();
                            command.CommandText = "select `action` from kpadminlogsglobal.transactionlogs where kptnno='" + listKptn[x].kptn + "' and `action` NOT IN ('PEEP','PO REPRINT','SO REPRINT') order by `timestamp` desc limit 1";
                            MySqlDataReader statusRdr = command.ExecuteReader();
                        
                            if (!statusRdr.HasRows)
                            {

                                translogs = true;
                            }
                            else
                            {
                                statusRdr.Read();
                                Status = statusRdr["action"].ToString();


                            }
                            statusRdr.Close();
                            using (MySqlCommand cmd2 = con.CreateCommand())
                            {
                                cmd2.Parameters.Clear();
                                if (listKptn[x].kptn == "")
                                {
                                    continue;
                                }

                                xTableName = decodeKPTNGlobal(0, listKptn[x].kptn);

                                if (AmountMin != "")
                                {

                                    cmd2.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                          "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                           "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                           "FROM " + xTableName + " where KPTNNO = '" + listKptn[x].kptn + "' AND (Total between '" + AmountMin + "' and '" + AmountMax + "'); ";
                                }
                                else
                                {
                                    cmd2.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                         "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                          "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                          "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                          "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                          "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                          "FROM " + xTableName + " where KPTNNO = '" + listKptn[x].kptn + "'; ";
                                }

                                MySqlDataReader transRdr = cmd2.ExecuteReader();

                                if (transRdr.HasRows && translogs == false)
                                {
                                    transRdr.Read();
                                    xcounter++;
                                    Int32 isCancelled = Convert.ToInt32(transRdr["isCancelled"]);

                                    if (WOCancel == true && isCancelled == 1) { transRdr.Close(); continue; }


                                    KPTNNO = transRdr["KPTNNO"].ToString();
                                    TransDate = transRdr["TransDate"].ToString();
                                    AmountPO = transRdr["AmountPO"].ToString();
                                    DateClaimed = transRdr["DateClaimed"].ToString();
                                    Principal = transRdr["Principal"].ToString();
                                    Charge = transRdr["Charge"].ToString();
                                    OtherCharge = transRdr["OtherCharge"].ToString();
                                    Total = transRdr["Total"].ToString();
                                    ExchangeRate = transRdr["ExchangeRate"].ToString();
                                    SenderFName = transRdr["SenderFName"].ToString();
                                    SenderMName = transRdr["SenderMName"].ToString();
                                    SenderLName = transRdr["SenderLName"].ToString();
                                    ReceiverFName = transRdr["ReceiverFName"].ToString();
                                    ReceiverMName = transRdr["ReceiverMName"].ToString();
                                    ReceiverLName = transRdr["ReceiverLName"].ToString();
                                    ReceiverStreet = transRdr["ReceiverStreet"].ToString();
                                    ReceiverProvinceCity = transRdr["ReceiverProvinceCity"].ToString();
                                    ReceiverCountry = transRdr["ReceiverCountry"].ToString();
                                    SenderStreet = transRdr["SenderStreet"].ToString();
                                    SenderProvinceCity = transRdr["SenderProvinceCity"].ToString();
                                    SenderCountry = transRdr["SenderCountry"].ToString();
                                    SenderBirthDate = transRdr["SenderBirthDate"].ToString();
                                    IDType = transRdr["IDType"].ToString();
                                    IDNo = transRdr["IDNo"].ToString();
                                    ExpiryDate = transRdr["ExpiryDate"].ToString();
                                    //SenderContactNo = transRdr["SenderContactNo"].ToString();
                                    ReceiverContactNo = transRdr["ReceiverContactNo"].ToString();
                                    PaymentType = transRdr["PaymentType"].ToString();
                                    TransType = transRdr["TransType"].ToString();
                                    CancelledDate = transRdr["CancelledDate"].ToString();
                                    CancelReason = transRdr["CancelReason"].ToString();
                                    Purpose = transRdr["Purpose"].ToString();

                                    transRdr.Close();

                                    String BDate = SenderBirthDate == "" ? String.Empty : Convert.ToDateTime(SenderBirthDate).ToString("yyyy-MM-dd");
                                    command.Parameters.Clear();
                                    command.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN, c.Mobile from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.FirstName = @FName and c.LastName = @LName and c.MiddleName = @MName and DATE_FORMAT(c.Birthdate,'%Y-%m-%d') = @BDate;";
                                    command.Parameters.AddWithValue("FName", SenderFName);
                                    command.Parameters.AddWithValue("LName", SenderLName);
                                    command.Parameters.AddWithValue("MName", SenderMName);
                                    command.Parameters.AddWithValue("BDate", BDate);
                                    MySqlDataReader custRdr = command.ExecuteReader();

                                    if (!custRdr.HasRows)
                                    {
                                        HomeCity = "";
                                        ZipCode = "";
                                        Occupation = "";
                                        SSN = "";
                                     
                                        
                                    }
                                    else
                                    {
                                        custRdr.Read();
                                        HomeCity = custRdr["HomeCity"].ToString();
                                        ZipCode = custRdr["ZipCode"].ToString();
                                        Occupation = custRdr["Occupation"].ToString();
                                        SSN = custRdr["SSN"].ToString();
                                        SenderContactNo = custRdr["Mobile"].ToString();
                                    }
                                    custRdr.Close();

                                    tl.Add(new TransList
                                    {

                                        kptnno = KPTNNO,
                                        TransDate = TransDate,
                                        POAmount = AmountPO,
                                        DateClaimed = DateClaimed,
                                        Principal = Principal,
                                        Charge = Charge,
                                        Othercharge = OtherCharge,
                                        Total = Total,
                                        ExchangeRate = ExchangeRate,
                                        SenderFName = SenderFName,
                                        SenderMName = SenderMName,
                                        SenderLName = SenderLName,
                                        ReceiverFName = ReceiverFName,
                                        ReceiverMName = ReceiverMName,
                                        ReceiverLName = ReceiverLName,
                                        ReceiverStreet = ReceiverStreet,
                                        ReceiverProvinceCity = ReceiverProvinceCity,
                                        ReceiverCountry = ReceiverCountry,
                                        SenderStreet = SenderStreet,
                                        SenderProvinceCity = SenderProvinceCity,
                                        SenderCountry = SenderCountry,
                                        SenderBirthDate = SenderBirthDate,
                                        IDType = IDType,
                                        IDNo = IDNo,
                                        ExpiryDate = ExpiryDate,
                                        SenderContactNo = SenderContactNo,
                                        ReceiverContactNo = ReceiverContactNo,
                                        PaymentType = PaymentType,
                                        TransType = TransType,
                                        CancelledDate = CancelledDate,
                                        CancelReason = CancelReason,
                                        Purpose = Purpose,
                                        City = HomeCity,
                                        occupation = Occupation,
                                        SSN = SSN,
                                        Status = Status,
                                        ZipCode = ZipCode

                                    });

                                }
                                else
                                {
                                    transRdr.Close();
                                    xTableNameD2B = decodeKPTNGlobald2b(0, listKptn[x].kptn);

                                    cmd2.Parameters.Clear();
                                    if (AmountMin != "")
                                    {
                                        cmd2.CommandText = "select RejectedDate, KPTNNO, TransDate, AmountPO, " +
                                                          "CompletedDate as DateClaimed, Principal, Charge, " +
                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                           "SenderContactNo,ReceiverContactNo,PaymentType,'INTERNATIONAL' as TransType,CancelledDate,CancelReason,Purpose " +
                                                           "FROM " + xTableNameD2B + " where KPTNNO = '" + listKptn[x].kptn + "' and (Total between '" + AmountMin + "' and '" + AmountMax + "'); ";
                                    }
                                    else
                                    {
                                        cmd2.CommandText = "select RejectedDate, KPTNNO, TransDate, AmountPO, " +
                                                          "CompletedDate as DateClaimed, Principal, Charge, " +
                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                           "SenderContactNo,ReceiverContactNo,PaymentType,'INTERNATIONAL' as TransType,CancelledDate,CancelReason,Purpose " +
                                                           "FROM " + xTableNameD2B + " where KPTNNO = '" + listKptn[x].kptn + "'; ";
                                    }

                                    MySqlDataReader d2brdr = cmd2.ExecuteReader();

                                    if (d2brdr.HasRows && translogs == false)
                                    {
                                        d2brdr.Read();


                                        xcounter++;

                                        String RejectedDate = d2brdr["RejectedDate"].ToString();
                                        CancelledDate = d2brdr["CancelledDate"].ToString();

                                        if (WOCancel == true && (RejectedDate != "" || CancelledDate != ""))
                                        {
                                            d2brdr.Close(); continue;
                                        }

                                        KPTNNO = d2brdr["KPTNNO"].ToString();
                                        TransDate = d2brdr["TransDate"].ToString();
                                        AmountPO = d2brdr["AmountPO"].ToString();
                                        DateClaimed = d2brdr["DateClaimed"].ToString();
                                        Principal = d2brdr["Principal"].ToString();
                                        Charge = d2brdr["Charge"].ToString();
                                        OtherCharge = d2brdr["OtherCharge"].ToString();
                                        Total = d2brdr["Total"].ToString();
                                        ExchangeRate = d2brdr["ExchangeRate"].ToString();
                                        SenderFName = d2brdr["SenderFName"].ToString();
                                        SenderMName = d2brdr["SenderMName"].ToString();
                                        SenderLName = d2brdr["SenderLName"].ToString();
                                        ReceiverFName = d2brdr["ReceiverFName"].ToString();
                                        ReceiverMName = d2brdr["ReceiverMName"].ToString();
                                        ReceiverLName = d2brdr["ReceiverLName"].ToString();
                                        ReceiverStreet = d2brdr["ReceiverStreet"].ToString();
                                        ReceiverProvinceCity = d2brdr["ReceiverProvinceCity"].ToString();
                                        ReceiverCountry = d2brdr["ReceiverCountry"].ToString();
                                        SenderStreet = d2brdr["SenderStreet"].ToString();
                                        SenderProvinceCity = d2brdr["SenderProvinceCity"].ToString();
                                        SenderCountry = d2brdr["SenderCountry"].ToString();
                                        SenderBirthDate = d2brdr["SenderBirthDate"].ToString();
                                        IDType = d2brdr["IDType"].ToString();
                                        IDNo = d2brdr["IDNo"].ToString();
                                        ExpiryDate = d2brdr["ExpiryDate"].ToString();
                                        SenderContactNo = d2brdr["SenderContactNo"].ToString();
                                        ReceiverContactNo = d2brdr["ReceiverContactNo"].ToString();
                                        PaymentType = d2brdr["PaymentType"].ToString();
                                        TransType = d2brdr["TransType"].ToString();
                                        //CancelledDate = d2brdr["CancelledDate"].ToString();
                                        CancelReason = d2brdr["CancelReason"].ToString();
                                        Purpose = d2brdr["Purpose"].ToString();

                                        d2brdr.Close();

                                        String BDate = SenderBirthDate == "" ? String.Empty : Convert.ToDateTime(SenderBirthDate).ToString("yyyy-MM-dd");
                                        command.Parameters.Clear();
                                        command.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN, c.Mobile from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.FirstName = @FName and c.LastName = @LName and c.MiddleName = @MName and DATE_FORMAT(c.Birthdate,'%Y-%m-%d') = @BDate;";
                                        command.Parameters.AddWithValue("FName", SenderFName);
                                        command.Parameters.AddWithValue("LName", SenderLName);
                                        command.Parameters.AddWithValue("MName", SenderMName);
                                        command.Parameters.AddWithValue("BDate", BDate);
                                        MySqlDataReader custRdr = command.ExecuteReader();

                                        if (!custRdr.HasRows)
                                        {
                                            HomeCity = "";
                                            ZipCode = "";
                                            Occupation = "";
                                            SSN = "";
                                        }
                                        else
                                        {
                                            custRdr.Read();
                                            HomeCity = custRdr["HomeCity"].ToString();
                                            ZipCode = custRdr["ZipCode"].ToString();
                                            Occupation = custRdr["Occupation"].ToString();
                                            SSN = custRdr["SSN"].ToString();
                                            SenderContactNo = custRdr["Mobile"].ToString();
                                        }
                                        custRdr.Close();

                                        tl.Add(new TransList
                                        {

                                            kptnno = KPTNNO,
                                            TransDate = TransDate,
                                            POAmount = AmountPO,
                                            DateClaimed = DateClaimed,
                                            Principal = Principal,
                                            Charge = Charge,
                                            Othercharge = OtherCharge,
                                            Total = Total,
                                            ExchangeRate = ExchangeRate,
                                            SenderFName = SenderFName,
                                            SenderMName = SenderMName,
                                            SenderLName = SenderLName,
                                            ReceiverFName = ReceiverFName,
                                            ReceiverMName = ReceiverMName,
                                            ReceiverLName = ReceiverLName,
                                            ReceiverStreet = ReceiverStreet,
                                            ReceiverProvinceCity = ReceiverProvinceCity,
                                            ReceiverCountry = ReceiverCountry,
                                            SenderStreet = SenderStreet,
                                            SenderProvinceCity = SenderProvinceCity,
                                            SenderCountry = SenderCountry,
                                            SenderBirthDate = SenderBirthDate,
                                            IDType = IDType,
                                            IDNo = IDNo,
                                            ExpiryDate = ExpiryDate,
                                            SenderContactNo = SenderContactNo,
                                            ReceiverContactNo = ReceiverContactNo,
                                            PaymentType = PaymentType,
                                            TransType = TransType,
                                            CancelledDate = CancelledDate,
                                            CancelReason = CancelReason,
                                            Purpose = Purpose,
                                            City = HomeCity,
                                            occupation = Occupation,
                                            SSN = SSN,
                                            Status = Status,
                                            ZipCode = ZipCode

                                        });

                                    }
                                    else
                                    {
                                        con.Close();
                                        d2brdr.Close();
                                        // Start Kiosk Here

                                        String xTableNameKiosk = "";
                                        using (MySqlConnection conKiosk = dbconKiosk.getConnection())
                                        {
                                            conKiosk.Open();
                                            using (MySqlCommand cmdK = conKiosk.CreateCommand())
                                            {


                                                xTableNameKiosk = decodeKPTNGlobalKiosk(listKptn[x].kptn);

                                                if (AmountMin != "")
                                                {

                                                    cmdK.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                                          "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                                           "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                                           "FROM " + xTableNameKiosk + " where KPTNNO = '" + listKptn[x].kptn + "' AND (Total between '" + AmountMin + "' and '" + AmountMax + "'); ";
                                                }
                                                else
                                                {
                                                    cmdK.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                                         "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                                          "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                                          "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                                          "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                                          "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                                          "FROM " + xTableNameKiosk + " where KPTNNO = '" + listKptn[x].kptn + "'; ";
                                                }


                                                MySqlDataReader transRdrKiosk = cmdK.ExecuteReader();

                                                if (transRdrKiosk.HasRows && translogs == false)
                                                {
                                                    transRdrKiosk.Read();
                                                    xcounter++;
                                                    Int32 isCancelled = Convert.ToInt32(transRdrKiosk["isCancelled"]);

                                                    if (WOCancel == true && isCancelled == 1) { transRdrKiosk.Close(); continue; }


                                                    KPTNNO = transRdrKiosk["KPTNNO"].ToString();
                                                    TransDate = transRdrKiosk["TransDate"].ToString();
                                                    AmountPO = transRdrKiosk["AmountPO"].ToString();
                                                    DateClaimed = transRdrKiosk["DateClaimed"].ToString();
                                                    Principal = transRdrKiosk["Principal"].ToString();
                                                    Charge = transRdrKiosk["Charge"].ToString();
                                                    OtherCharge = transRdrKiosk["OtherCharge"].ToString();
                                                    Total = transRdrKiosk["Total"].ToString();
                                                    ExchangeRate = transRdrKiosk["ExchangeRate"].ToString();
                                                    SenderFName = transRdrKiosk["SenderFName"].ToString();
                                                    SenderMName = transRdrKiosk["SenderMName"].ToString();
                                                    SenderLName = transRdrKiosk["SenderLName"].ToString();
                                                    ReceiverFName = transRdrKiosk["ReceiverFName"].ToString();
                                                    ReceiverMName = transRdrKiosk["ReceiverMName"].ToString();
                                                    ReceiverLName = transRdrKiosk["ReceiverLName"].ToString();
                                                    ReceiverStreet = transRdrKiosk["ReceiverStreet"].ToString();
                                                    ReceiverProvinceCity = transRdrKiosk["ReceiverProvinceCity"].ToString();
                                                    ReceiverCountry = transRdrKiosk["ReceiverCountry"].ToString();
                                                    SenderStreet = transRdrKiosk["SenderStreet"].ToString();
                                                    SenderProvinceCity = transRdrKiosk["SenderProvinceCity"].ToString();
                                                    SenderCountry = transRdrKiosk["SenderCountry"].ToString();
                                                    SenderBirthDate = transRdrKiosk["SenderBirthDate"].ToString();
                                                    IDType = transRdrKiosk["IDType"].ToString();
                                                    IDNo = transRdrKiosk["IDNo"].ToString();
                                                    ExpiryDate = transRdrKiosk["ExpiryDate"].ToString();
                                                    SenderContactNo = transRdrKiosk["SenderContactNo"].ToString();
                                                    ReceiverContactNo = transRdrKiosk["ReceiverContactNo"].ToString();
                                                    PaymentType = transRdrKiosk["PaymentType"].ToString();
                                                    TransType = transRdrKiosk["TransType"].ToString();
                                                    CancelledDate = transRdrKiosk["CancelledDate"].ToString();
                                                    CancelReason = transRdrKiosk["CancelReason"].ToString();
                                                    Purpose = transRdrKiosk["Purpose"].ToString();
                                                    transRdrKiosk.Close();

                                                    conKiosk.Close();

                                                    using (MySqlConnection cong = dbconnetwork.getConnection())
                                                    {
                                                        cong.Open();
                                                        using (MySqlCommand commad = cong.CreateCommand())
                                                        {
                                                            String BDate = SenderBirthDate == "" ? String.Empty : Convert.ToDateTime(SenderBirthDate).ToString("yyyy-MM-dd");
                                                            command.Parameters.Clear();
                                                            command.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN, c.Mobile from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.FirstName = @FName and c.LastName = @LName and c.MiddleName = @MName and DATE_FORMAT(c.Birthdate,'%Y-%m-%d') = @BDate;";
                                                            command.Parameters.AddWithValue("FName", SenderFName);
                                                            command.Parameters.AddWithValue("LName", SenderLName);
                                                            command.Parameters.AddWithValue("MName", SenderMName);
                                                            command.Parameters.AddWithValue("BDate", BDate);
                                                            MySqlDataReader custRdr = command.ExecuteReader();

                                                            if (!custRdr.HasRows)
                                                            {
                                                                HomeCity = "";
                                                                ZipCode = "";
                                                                Occupation = "";
                                                                SSN = "";

                                                            }
                                                            else
                                                            {
                                                                custRdr.Read();
                                                                HomeCity = custRdr["HomeCity"].ToString();
                                                                ZipCode = custRdr["ZipCode"].ToString();
                                                                Occupation = custRdr["Occupation"].ToString();
                                                                SSN = custRdr["SSN"].ToString();
                                                                SenderContactNo = custRdr["Mobile"].ToString();
                                                            }
                                                            custRdr.Close();
                                                        }
                                                        cong.Close();
                                                    }



                                                    tl.Add(new TransList
                                                    {

                                                        kptnno = KPTNNO,
                                                        TransDate = TransDate,
                                                        POAmount = AmountPO,
                                                        DateClaimed = DateClaimed,
                                                        Principal = Principal,
                                                        Charge = Charge,
                                                        Othercharge = OtherCharge,
                                                        Total = Total,
                                                        ExchangeRate = ExchangeRate,
                                                        SenderFName = SenderFName,
                                                        SenderMName = SenderMName,
                                                        SenderLName = SenderLName,
                                                        ReceiverFName = ReceiverFName,
                                                        ReceiverMName = ReceiverMName,
                                                        ReceiverLName = ReceiverLName,
                                                        ReceiverStreet = ReceiverStreet,
                                                        ReceiverProvinceCity = ReceiverProvinceCity,
                                                        ReceiverCountry = ReceiverCountry,
                                                        SenderStreet = SenderStreet,
                                                        SenderProvinceCity = SenderProvinceCity,
                                                        SenderCountry = SenderCountry,
                                                        SenderBirthDate = SenderBirthDate,
                                                        IDType = IDType,
                                                        IDNo = IDNo,
                                                        ExpiryDate = ExpiryDate,
                                                        SenderContactNo = SenderContactNo,
                                                        ReceiverContactNo = ReceiverContactNo,
                                                        PaymentType = PaymentType,
                                                        TransType = TransType,
                                                        CancelledDate = CancelledDate,
                                                        CancelReason = CancelReason,
                                                        Purpose = Purpose,
                                                        City = HomeCity,
                                                        occupation = Occupation,
                                                        SSN = SSN,
                                                        Status = Status,
                                                        ZipCode = ZipCode

                                                    });

                                                }
                                                else
                                                {
                                                    transRdrKiosk.Close();
                                                    
                                                    continue;
                                                }
                                                //End Kiosk
                                            }

                                        }





                                    }

                                }


                            }
                        }


                        if (listKptn.Count == 0)
                        {
                            return new SeccomResponse { respcode = 0, message = "No Data Found!", data = null };
                        }



                        con.Close();
                        return new SeccomResponse { respcode = 1, message = "Success!", data = new List<TransList>(tl), RowsCount = tl.Count };





                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
                    }


                }

            }

        }
        catch (Exception ex)
        {
            dbconnetwork.CloseConnection();
            return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
        }

    }


    //Modified by: Rr
    //Date : 07-20-2016
    //Description: Added Kiosk Transactions
    //Done : 07-20-2016
    //Modified by: Rr
    //Date : 10-12-16
    //Description: Added Billspay Transactions
    //Done : 10-12-16
    public SeccomResponse getTransReviewBySenderAddress(String Username, String Password, String SenderStreet, 
                                                            String ZipCode, String City, String State, String DateFrom,
                                                            String DateTo, String AmountMin, String AmountMax, 
                                                            Boolean WOCancel)
    {
        try
        {

            if (Username != loginuser || Password != loginpass)
            {
                return new SeccomResponse { respcode = 7, message = getRespMessage(7) };
            }

            List<TransList> tl = new List<TransList>();
            String HomeCity = string.Empty;
            String ZipCodeVar = ZipCode;
            String Occupation = string.Empty;
            String SSN = string.Empty;
            String Status = string.Empty;
            String KPTNNO = string.Empty;
            String TransDate = string.Empty;
            String AmountPO = string.Empty;
            String DateClaimed = string.Empty;
            String Principal = string.Empty;
            String Charge = string.Empty;
            String OtherCharge = string.Empty;
            String Total = string.Empty;
            String ExchangeRate = string.Empty;
            String SenderFName = string.Empty;
            String SenderMName = string.Empty;
            String SenderLName = string.Empty;
            String ReceiverFName = string.Empty;
            String ReceiverMName = string.Empty;
            String ReceiverLName = string.Empty;
            String ReceiverStreet = string.Empty;
            String ReceiverProvinceCity = string.Empty;
            String ReceiverCountry = string.Empty;
            String SenderStreetVar = string.Empty;
            String SenderProvinceCity = string.Empty;
            String SenderCountry = string.Empty;
            String SenderBirthDate = string.Empty;
            String IDType = string.Empty;
            String IDNo = string.Empty;
            String ExpiryDate = string.Empty;
            String SenderContactNo = string.Empty;
            String ReceiverContactNo = string.Empty;
            String PaymentType = string.Empty;
            String TransType = string.Empty;
            String CancelledDate = string.Empty;
            String CancelReason = string.Empty;
            String Purpose = string.Empty;
            String xTableName = string.Empty;
            String xTableNameD2B = string.Empty;
            Boolean translogs = false;
            Boolean in365 = false;
            Int32 xcounter = 0;

            using (MySqlConnection con = dbconnetwork.getConnection())
            {
                DateTime DateNow = getServerDateGlobal(false);
                con.Open();
                using (MySqlCommand command = con.CreateCommand())
                {
                    try
                    {

                        DataTable dt = new DataTable();
                        DataSet ds = new DataSet();
                        List<FirstList> listKptn = new List<FirstList>();
                        String[] custArr = new String[7];
                        String from = (DateFrom == "" || DateFrom == null ? DateNow.AddMonths(-6).ToString("yyyy-MM-dd 00:00:00") : (Convert.ToDateTime(DateFrom)).ToString("yyyy-MM-dd 00:00:00"));
                        String to = (DateTo == "" || DateTo == null ? DateNow.ToString("yyyy-MM-dd 23:59:59") : (Convert.ToDateTime(DateTo)).ToString("yyyy-MM-dd 23:59:59"));
                        String df = Convert.ToDateTime(to).AddMonths(-6).ToString("MM");


                        int rr = Convert.ToInt32(df);

                        for (int z = 0; z <= 6; z++)
                        {
                            String xrr = rr.ToString();
                            if (rr < 10)
                                xrr = "0" + xrr;


                            custArr[z] = xrr;

                            if (rr % 12 == 0)
                            {
                                rr = 1;
                                rr--;
                            }

                            rr++;

                        }
                        
                        String query;
                        Int32 i = 0;
                        if (SenderStreet == null) SenderStreet = "";
                        if (AmountMin == null) AmountMin = "";
                        if (ZipCode == null) ZipCode = "";
                        if (City == null) City = "";
                        if (State == null) State = "";
                        

                        if (ZipCode != "" && City != "" && State != "" && AmountMin != "")
                        {

                            query = "where SenderStreet = @SenderStreet and c.ZipCode = @ZipCode And d.HomeCity = @City and SenderProvinceCity = @State and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";

                        }
                        else if (ZipCode != "" && City != "" && State!= "")
                        {
                            query = "where SenderStreet = @SenderStreet and c.ZipCode = @ZipCode And d.HomeCity = @City and SenderProvinceCity = @State AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (ZipCode != "" && State != "" && AmountMin != "")
                        {
                            query = "where SenderStreet = @SenderStreet and c.ZipCode = @ZipCode and SenderProvinceCity = @State and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (City != "" && State != "" && AmountMin != "")
                        {
                            query = "where SenderStreet = @SenderStreet  And d.HomeCity = @City and SenderProvinceCity = @State and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (ZipCode != "" && City != "")
                        {
                            query = "where SenderStreet = @SenderStreet  And d.HomeCity = @City AND c.ZipCode = @ZipCode AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (ZipCode != "" && State != "")
                        {
                            query = "where SenderStreet = @SenderStreet  And SenderProvinceCity = @State AND c.ZipCode = @ZipCode AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (ZipCode != "" && AmountMin != "")
                        {
                            query = "where SenderStreet = @SenderStreet  And SenderProvinceCity = @State AND c.ZipCode = @ZipCode and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (City != "" && State != "")
                        {
                            query = "where SenderStreet = @SenderStreet  And SenderProvinceCity = @State AND d.HomeCity = @City AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (City != "" && AmountMin != "")
                        {
                            query = "where SenderStreet = @SenderStreet  and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND d.HomeCity = @City AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (State != "" && AmountMin != "")
                        {
                            query = "where SenderStreet = @SenderStreet  and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND SenderProvinceCity = @State AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (ZipCode != "")
                        {
                            query = "where SenderStreet = @SenderStreet   AND c.ZipCode = @ZipCode AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (City != "")
                        {
                            query = "where SenderStreet = @SenderStreet   AND d.HomeCity = @City AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (State != "")
                        {
                            query = "where SenderStreet = @SenderStreet   AND SenderProvinceCity = @State AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else if (AmountMin != "")
                        {
                            query = "where SenderStreet = @SenderStreet   and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND  (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }
                        else 
                        {
                            query = "where SenderStreet = @SenderStreet AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }

                        for (i = 0; i < custArr.Length; i++)
                        {



                            command.Parameters.Clear();
                            command.CommandText = "Select TransDate, KPTNNo from kptransactionsglobal.sendout" + custArr[i] + " s LEFT JOIN kpcustomersglobal.customers c ON s.SenderFName = c.FirstName AND s.SenderLName = c.LastName AND s.SenderMName = c.MiddleName LEFT join kpcustomersglobal.customersdetails d ON d.CustID = c.CustID " + query;
                            command.Parameters.AddWithValue("AmountMin", AmountMin);
                            command.Parameters.AddWithValue("AmountMax", AmountMax);
                            command.Parameters.AddWithValue("DateFrom", from);
                            command.Parameters.AddWithValue("DateTo", to);
                            command.Parameters.AddWithValue("State", State   );
                            command.Parameters.AddWithValue("City", City);
                            command.Parameters.AddWithValue("ZipCode", ZipCode);
                            command.Parameters.AddWithValue("SenderStreet", SenderStreet);
                            MySqlDataReader rdr2 = command.ExecuteReader();

                            while (rdr2.Read())
                            {
                                listKptn.Add(new FirstList
                                {
                                    kptn = rdr2["KPTNNo"].ToString(),
                                    TransDate = rdr2["TransDate"].ToString()
                                });


                            }


                            rdr2.Close();


                            command.Parameters.Clear();
                            command.CommandText = "Select TransDate, KPTNNo from kptransactionsglobal.sendoutd2b" + custArr[i] + " s LEFT JOIN kpcustomersglobal.customers c ON s.SenderFName = c.FirstName AND s.SenderLName = c.LastName AND s.SenderMName = c.MiddleName LEFT join kpcustomersglobal.customersdetails d ON d.CustID = c.CustID " + query;
                            command.Parameters.AddWithValue("AmountMin", AmountMin);
                            command.Parameters.AddWithValue("AmountMax", AmountMax);
                            command.Parameters.AddWithValue("DateFrom", from);
                            command.Parameters.AddWithValue("DateTo", to);
                            command.Parameters.AddWithValue("State", State);
                            command.Parameters.AddWithValue("City", City);
                            command.Parameters.AddWithValue("ZipCode", ZipCode);
                            command.Parameters.AddWithValue("SenderStreet", SenderStreet);
                            MySqlDataReader rdrd2b = command.ExecuteReader();

                            while (rdrd2b.Read())
                            {
                                listKptn.Add(new FirstList
                                {
                                    kptn = rdrd2b["KPTNNo"].ToString(),
                                    TransDate = rdrd2b["TransDate"].ToString()
                                });


                            }


                            rdrd2b.Close();

                        }

                        //Start Kiosk Transactions
                        con.Close();
                        i = 0;
                        for (i = 0; i < custArr.Length; i++)
                        {

                            using (MySqlConnection conkiosk = dbconKiosk.getConnection()) 
                            {
                                conkiosk.Open();
                                using(MySqlCommand cmd = conkiosk.CreateCommand())
                                {
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "Select TransDate, KPTNNo from kpkiosktransactionsglobal.sendout" + custArr[i] + " s LEFT JOIN kpcustomersglobal.customers c ON s.SenderFName = c.FirstName AND s.SenderLName = c.LastName AND s.SenderMName = c.MiddleName LEFT join kpcustomersglobal.customersdetails d ON d.CustID = c.CustID " + query;
                                    cmd.Parameters.AddWithValue("AmountMin", AmountMin);
                                    cmd.Parameters.AddWithValue("AmountMax", AmountMax);
                                    cmd.Parameters.AddWithValue("DateFrom", from);
                                    cmd.Parameters.AddWithValue("DateTo", to);
                                    cmd.Parameters.AddWithValue("State", State);
                                    cmd.Parameters.AddWithValue("City", City);
                                    cmd.Parameters.AddWithValue("ZipCode", ZipCode);
                                    cmd.Parameters.AddWithValue("SenderStreet", SenderStreet);
                                    MySqlDataReader rdr2 = cmd.ExecuteReader();

                                    while (rdr2.Read())
                                    {
                                        listKptn.Add(new FirstList
                                        {
                                            kptn = rdr2["KPTNNo"].ToString(),
                                            TransDate = rdr2["TransDate"].ToString()
                                        });


                                    }


                                    rdr2.Close();
                                }
                                conkiosk.Close();
                            }

                        }
                        i = 0;
                        for (i = 0; i < custArr.Length; i++)
                        {
                            if (ZipCode != "" && City != "" && State != "" && AmountMin != "")
                            {

                                query = "where c.Street = @SenderStreet and c.ZipCode = @ZipCode And d.HomeCity = @City and c.ProvinceCity = @State and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";

                            }
                            else if (ZipCode != "" && City != "" && State != "")
                            {
                                query = "where c.Street = @SenderStreet and c.ZipCode = @ZipCode And d.HomeCity = @City and c.ProvinceCity = @State AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (ZipCode != "" && State != "" && AmountMin != "")
                            {
                                query = "where c.Street = @SenderStreet and c.ZipCode = @ZipCode and c.ProvinceCity = @State and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (City != "" && State != "" && AmountMin != "")
                            {
                                query = "where c.Street = @SenderStreet  And d.HomeCity = @City and c.ProvinceCity = @State and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (ZipCode != "" && City != "")
                            {
                                query = "where c.Street = @SenderStreet  And d.HomeCity = @City AND c.ZipCode = @ZipCode AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (ZipCode != "" && State != "")
                            {
                                query = "where c.Street = @SenderStreet  And c.ProvinceCity = @State AND c.ZipCode = @ZipCode AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (ZipCode != "" && AmountMin != "")
                            {
                                query = "where c.Street = @SenderStreet  And c.ProvinceCity = @State AND c.ZipCode = @ZipCode and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (City != "" && State != "")
                            {
                                query = "where c.Street = @SenderStreet  And c.ProvinceCity = @State AND d.HomeCity = @City AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (City != "" && AmountMin != "")
                            {
                                query = "where c.Street = @SenderStreet  and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND d.HomeCity = @City AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (State != "" && AmountMin != "")
                            {
                                query = "where c.Street = @SenderStreet  and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND c.ProvinceCity = @State AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (ZipCode != "")
                            {
                                query = "where c.Street = @SenderStreet   AND c.ZipCode = @ZipCode AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (City != "")
                            {
                                query = "where c.Street = @SenderStreet   AND d.HomeCity = @City AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (State != "")
                            {
                                query = "where c.Street = @SenderStreet   AND c.ProvinceCity = @State AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else if (AmountMin != "")
                            {
                                query = "where c.Street = @SenderStreet   and (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND  (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }
                            else
                            {
                                query = "where c.Street = @SenderStreet AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                            }

                            using (MySqlConnection conB = dbconBillspay.getConnection())
                            {
                                conB.Open();
                                using (MySqlCommand cmd = conB.CreateCommand())
                                {
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "Select TransDate, KPTN as KPTNNo from kptransactionsglobal.sendoutbillspay" + custArr[i] + " s LEFT JOIN kpcustomersglobal.customers c ON s.PayorFName = c.FirstName AND s.PayorLName = c.LastName AND s.PayorMName = c.MiddleName LEFT join kpcustomersglobal.customersdetails d ON d.CustID = c.CustID " + query;
                                    cmd.Parameters.AddWithValue("AmountMin", AmountMin);
                                    cmd.Parameters.AddWithValue("AmountMax", AmountMax);
                                    cmd.Parameters.AddWithValue("DateFrom", from);
                                    cmd.Parameters.AddWithValue("DateTo", to);
                                    cmd.Parameters.AddWithValue("State", State);
                                    cmd.Parameters.AddWithValue("City", City);
                                    cmd.Parameters.AddWithValue("ZipCode", ZipCode);
                                    cmd.Parameters.AddWithValue("SenderStreet", SenderStreet);
                                    MySqlDataReader rdr2 = cmd.ExecuteReader();

                                    while (rdr2.Read())
                                    {
                                        listKptn.Add(new FirstList
                                        {
                                            kptn = rdr2["KPTNNo"].ToString(),
                                            TransDate = rdr2["TransDate"].ToString()
                                        });


                                    }


                                    rdr2.Close();
                                }
                                conB.Close();
                            }

                        }
                        //End Billspay Transactions
                        con.Open();

                        for (int x = 0; x < listKptn.Count; x++)
                        {
                            translogs = false;
                            if (listKptn[x].kptn == "")
                            {
                                continue;
                            }


                            if (listKptn[x].kptn.Substring(0, 3) == "BPG")
                            {
                                TransList response = new TransList();
                                response = getBillspay(listKptn[x].kptn, AmountMin, AmountMax, WOCancel);

                                if (response != null)
                                {
                                    tl.Add(response);

                                }
                                continue;
                            }

                            command.Parameters.Clear();
                            command.CommandText = "select `action` from kpadminlogsglobal.transactionlogs where kptnno='" + listKptn[x].kptn + "' and `action` NOT IN ('PEEP','PO REPRINT','SO REPRINT') order by `timestamp` desc limit 1";
                            MySqlDataReader statusRdr = command.ExecuteReader();
                            if (!statusRdr.HasRows)
                            {

                                translogs = true;
                            }
                            else
                            {
                                statusRdr.Read();
                                Status = statusRdr["action"].ToString();


                            }
                            statusRdr.Close();
                            using (MySqlCommand cmd2 = con.CreateCommand())
                            {
                                cmd2.Parameters.Clear();

                                xTableName = decodeKPTNGlobal(0, listKptn[x].kptn);

                                cmd2.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                      "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                       "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                       "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                       "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                       "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                       "FROM " + xTableName + " where KPTNNO = '" + listKptn[x].kptn + "'; ";

                                MySqlDataReader transRdr = cmd2.ExecuteReader();

                                if (transRdr.HasRows && translogs == false)
                                {
                                    transRdr.Read();
                                    xcounter++;
                                    Int32 isCancelled = Convert.ToInt32(transRdr["isCancelled"]);

                                    if (WOCancel == true && isCancelled == 1) { transRdr.Close(); continue; }


                                    KPTNNO = transRdr["KPTNNO"].ToString();
                                    TransDate = transRdr["TransDate"].ToString();
                                    AmountPO = transRdr["AmountPO"].ToString();
                                    DateClaimed = transRdr["DateClaimed"].ToString();
                                    Principal = transRdr["Principal"].ToString();
                                    Charge = transRdr["Charge"].ToString();
                                    OtherCharge = transRdr["OtherCharge"].ToString();
                                    Total = transRdr["Total"].ToString();
                                    ExchangeRate = transRdr["ExchangeRate"].ToString();
                                    SenderFName = transRdr["SenderFName"].ToString();
                                    SenderMName = transRdr["SenderMName"].ToString();
                                    SenderLName = transRdr["SenderLName"].ToString();
                                    ReceiverFName = transRdr["ReceiverFName"].ToString();
                                    ReceiverMName = transRdr["ReceiverMName"].ToString();
                                    ReceiverLName = transRdr["ReceiverLName"].ToString();
                                    ReceiverStreet = transRdr["ReceiverStreet"].ToString();
                                    ReceiverProvinceCity = transRdr["ReceiverProvinceCity"].ToString();
                                    ReceiverCountry = transRdr["ReceiverCountry"].ToString();
                                    SenderStreetVar = transRdr["SenderStreet"].ToString();
                                    SenderProvinceCity = transRdr["SenderProvinceCity"].ToString();
                                    SenderCountry = transRdr["SenderCountry"].ToString();
                                    SenderBirthDate = transRdr["SenderBirthDate"].ToString();
                                    IDType = transRdr["IDType"].ToString();
                                    IDNo = transRdr["IDNo"].ToString();
                                    ExpiryDate = transRdr["ExpiryDate"].ToString();
                                    SenderContactNo = transRdr["SenderContactNo"].ToString();
                                    ReceiverContactNo = transRdr["ReceiverContactNo"].ToString();
                                    PaymentType = transRdr["PaymentType"].ToString();
                                    TransType = transRdr["TransType"].ToString();
                                    CancelledDate = transRdr["CancelledDate"].ToString();
                                    CancelReason = transRdr["CancelReason"].ToString();
                                    Purpose = transRdr["Purpose"].ToString();


                                    transRdr.Close();
                                
                                }
                                else
                                {
                                    transRdr.Close();
                                    xTableNameD2B = decodeKPTNGlobald2b(0, listKptn[x].kptn);

                                    cmd2.Parameters.Clear();
                                    cmd2.CommandText = "select RejectedDate, KPTNNO, TransDate, AmountPO, " +
                                                      "CompletedDate as DateClaimed, Principal, Charge, " +
                                                       "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                       "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                       "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                       "SenderContactNo,ReceiverContactNo,PaymentType,'INTERNATIONAL' as TransType,CancelledDate,CancelReason,Purpose " +
                                                       "FROM " + xTableNameD2B + " where KPTNNO = '" + listKptn[x].kptn + "'; ";

                                    MySqlDataReader d2brdr = cmd2.ExecuteReader();

                                    if (d2brdr.HasRows && translogs == false)
                                    {
                                        d2brdr.Read();


                                        xcounter++;

                                        String RejectedDate = d2brdr["RejectedDate"].ToString();
                                        CancelledDate = d2brdr["CancelledDate"].ToString();

                                        if (WOCancel == true && (RejectedDate != "" || CancelledDate != ""))
                                        {
                                            d2brdr.Close(); continue;
                                        }

                                        KPTNNO = d2brdr["KPTNNO"].ToString();
                                        TransDate = d2brdr["TransDate"].ToString();
                                        AmountPO = d2brdr["AmountPO"].ToString();
                                        DateClaimed = d2brdr["DateClaimed"].ToString();
                                        Principal = d2brdr["Principal"].ToString();
                                        Charge = d2brdr["Charge"].ToString();
                                        OtherCharge = d2brdr["OtherCharge"].ToString();
                                        Total = d2brdr["Total"].ToString();
                                        ExchangeRate = d2brdr["ExchangeRate"].ToString();
                                        SenderFName = d2brdr["SenderFName"].ToString();
                                        SenderMName = d2brdr["SenderMName"].ToString();
                                        SenderLName = d2brdr["SenderLName"].ToString();
                                        ReceiverFName = d2brdr["ReceiverFName"].ToString();
                                        ReceiverMName = d2brdr["ReceiverMName"].ToString();
                                        ReceiverLName = d2brdr["ReceiverLName"].ToString();
                                        ReceiverStreet = d2brdr["ReceiverStreet"].ToString();
                                        ReceiverProvinceCity = d2brdr["ReceiverProvinceCity"].ToString();
                                        ReceiverCountry = d2brdr["ReceiverCountry"].ToString();
                                        SenderStreet = d2brdr["SenderStreet"].ToString();
                                        SenderProvinceCity = d2brdr["SenderProvinceCity"].ToString();
                                        SenderCountry = d2brdr["SenderCountry"].ToString();
                                        SenderBirthDate = d2brdr["SenderBirthDate"].ToString();
                                        IDType = d2brdr["IDType"].ToString();
                                        IDNo = d2brdr["IDNo"].ToString();
                                        ExpiryDate = d2brdr["ExpiryDate"].ToString();
                                        SenderContactNo = d2brdr["SenderContactNo"].ToString();
                                        ReceiverContactNo = d2brdr["ReceiverContactNo"].ToString();
                                        PaymentType = d2brdr["PaymentType"].ToString();
                                        TransType = d2brdr["TransType"].ToString();
                                        //   CancelledDate = d2brdr["CancelledDate"].ToString();
                                        CancelReason = d2brdr["CancelReason"].ToString();
                                        Purpose = d2brdr["Purpose"].ToString();

                                        d2brdr.Close();

                                    }
                                    else 
                                    {
                                        d2brdr.Close();
                                        con.Close();

                                        using (MySqlConnection conKiosk = dbconKiosk.getConnection())
                                        {
                                            String xTableKiosk = "";
                                            conKiosk.Open();
                                            using (MySqlCommand cmd = conKiosk.CreateCommand())
                                            {
                                                xTableKiosk = decodeKPTNGlobalKiosk(listKptn[x].kptn);

                                                cmd.Parameters.Clear();
                                                cmd.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                          "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                           "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                           "FROM " + xTableKiosk + " where KPTNNO = '" + listKptn[x].kptn + "'; ";

                                                MySqlDataReader kioskRdr = cmd.ExecuteReader();

                                                if (kioskRdr.HasRows && translogs == false)
                                                {
                                                    kioskRdr.Read();

                                                    xcounter++;
                                                    Int32 isCancelled = Convert.ToInt32(kioskRdr["isCancelled"]);

                                                    if (WOCancel == true && isCancelled == 1) { kioskRdr.Close(); conKiosk.Close(); con.Open(); continue; }

                                                    KPTNNO = kioskRdr["KPTNNO"].ToString();
                                                    TransDate = kioskRdr["TransDate"].ToString();
                                                    AmountPO = kioskRdr["AmountPO"].ToString();
                                                    DateClaimed = kioskRdr["DateClaimed"].ToString();
                                                    Principal = kioskRdr["Principal"].ToString();
                                                    Charge = kioskRdr["Charge"].ToString();
                                                    OtherCharge = kioskRdr["OtherCharge"].ToString();
                                                    Total = kioskRdr["Total"].ToString();
                                                    ExchangeRate = kioskRdr["ExchangeRate"].ToString();
                                                    SenderFName = kioskRdr["SenderFName"].ToString();
                                                    SenderMName = kioskRdr["SenderMName"].ToString();
                                                    SenderLName = kioskRdr["SenderLName"].ToString();
                                                    ReceiverFName = kioskRdr["ReceiverFName"].ToString();
                                                    ReceiverMName = kioskRdr["ReceiverMName"].ToString();
                                                    ReceiverLName = kioskRdr["ReceiverLName"].ToString();
                                                    ReceiverStreet = kioskRdr["ReceiverStreet"].ToString();
                                                    ReceiverProvinceCity = kioskRdr["ReceiverProvinceCity"].ToString();
                                                    ReceiverCountry = kioskRdr["ReceiverCountry"].ToString();
                                                    SenderStreet = kioskRdr["SenderStreet"].ToString();
                                                    SenderProvinceCity = kioskRdr["SenderProvinceCity"].ToString();
                                                    SenderCountry = kioskRdr["SenderCountry"].ToString();
                                                    SenderBirthDate = kioskRdr["SenderBirthDate"].ToString();
                                                    IDType = kioskRdr["IDType"].ToString();
                                                    IDNo = kioskRdr["IDNo"].ToString();
                                                    ExpiryDate = kioskRdr["ExpiryDate"].ToString();
                                                    SenderContactNo = kioskRdr["SenderContactNo"].ToString();
                                                    ReceiverContactNo = kioskRdr["ReceiverContactNo"].ToString();
                                                    PaymentType = kioskRdr["PaymentType"].ToString();
                                                    TransType = kioskRdr["TransType"].ToString();
                                                    //  CancelledDate = d2brdr["CancelledDate"].ToString();
                                                    CancelReason = kioskRdr["CancelReason"].ToString();
                                                    Purpose = kioskRdr["Purpose"].ToString();

                                                    kioskRdr.Close();


                                                }
                                                else
                                                {
                                                    kioskRdr.Close();
                                                    con.Open();
                                                    continue;
                                                }


                                            }

                                            con.Open();

                                        }

                                    
                                    }

                                  

                                  

                                    

                                }



                                String BDate = SenderBirthDate == "" ? String.Empty : Convert.ToDateTime(SenderBirthDate).ToString("yyyy-MM-dd");
                                command.Parameters.Clear();
                                command.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN,c.Mobile from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.FirstName = @FName and c.LastName = @LName and c.MiddleName = @MName and DATE_FORMAT(c.Birthdate,'%Y-%m-%d') = @BDate;";
                                command.Parameters.AddWithValue("FName", SenderFName);
                                command.Parameters.AddWithValue("LName", SenderLName);
                                command.Parameters.AddWithValue("MName", SenderMName);
                                command.Parameters.AddWithValue("BDate", BDate);
                                MySqlDataReader custRdr = command.ExecuteReader();

                                if (!custRdr.HasRows)
                                {
                                    HomeCity = City;
                                    //  ZipCode = ZipCodeVar;
                                    Occupation = "";
                                    SSN = "";
                                }
                                else
                                {
                                    custRdr.Read();
                                    HomeCity = custRdr["HomeCity"].ToString();
                                    ZipCode = custRdr["ZipCode"].ToString();
                                    Occupation = custRdr["Occupation"].ToString();
                                    SSN = custRdr["SSN"].ToString();
                                    SenderContactNo = custRdr["Mobile"].ToString();

                                }
                                custRdr.Close();

                                tl.Add(new TransList
                                {

                                    kptnno = KPTNNO,
                                    TransDate = TransDate,
                                    POAmount = AmountPO,
                                    DateClaimed = DateClaimed,
                                    Principal = Principal,
                                    Charge = Charge,
                                    Othercharge = OtherCharge,
                                    Total = Total,
                                    ExchangeRate = ExchangeRate,
                                    SenderFName = SenderFName,
                                    SenderMName = SenderMName,
                                    SenderLName = SenderLName,
                                    ReceiverFName = ReceiverFName,
                                    ReceiverMName = ReceiverMName,
                                    ReceiverLName = ReceiverLName,
                                    ReceiverStreet = ReceiverStreet,
                                    ReceiverProvinceCity = ReceiverProvinceCity,
                                    ReceiverCountry = ReceiverCountry,
                                    SenderStreet = SenderStreet,
                                    SenderProvinceCity = SenderProvinceCity,
                                    SenderCountry = SenderCountry,
                                    SenderBirthDate = SenderBirthDate,
                                    IDType = IDType,
                                    IDNo = IDNo,
                                    ExpiryDate = ExpiryDate,
                                    SenderContactNo = SenderContactNo,
                                    ReceiverContactNo = ReceiverContactNo,
                                    PaymentType = PaymentType,
                                    TransType = TransType,
                                    CancelledDate = CancelledDate,
                                    CancelReason = CancelReason,
                                    Purpose = Purpose,
                                    City = HomeCity,
                                    occupation = Occupation,
                                    SSN = SSN,
                                    Status = Status,
                                    ZipCode = ZipCode

                                });

                            }
                        }



                        if (listKptn.Count == 0)
                        {
                            return new SeccomResponse { respcode = 0, message = "No Data Found!", data = null };
                        }



                        con.Close();
                        return new SeccomResponse { respcode = 1, message = "Success!", data = new List<TransList>(tl), RowsCount = tl.Count };





                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
                    }


                }

            }

        }
        catch (Exception ex)
        {
            dbconnetwork.CloseConnection();
            return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
        }

    }
    //Modified by: Rr
    //Date : 07-20-2016
    //Description: Added Kiosk Transactions
    //Done : 07-20-2016

    // //Modified by: Rr
    //Date : 10-12-16
    //Description: Billspayment Transactions aDded!
    //Done : 10-12-16
    public SeccomResponse getTransReviewByPhoneNo(String Username, String Password, String PhoneNo, String DateTo,
                                                         String DateFrom, String AmountMin, String AmountMax, Boolean WOCancel)
    {
        try
        {

            if (Username != loginuser || Password != loginpass)
            {
                return new SeccomResponse { respcode = 7, message = getRespMessage(7) };
            }

            List<TransList> tl = new List<TransList>();
            String HomeCity = string.Empty;
            String ZipCode = string.Empty;
            String Occupation = string.Empty;
            String SSN = string.Empty;
            String Status = string.Empty;
            String KPTNNO = string.Empty;
            String TransDate = string.Empty;
            String AmountPO = string.Empty;
            String DateClaimed = string.Empty;
            String Principal = string.Empty;
            String Charge = string.Empty;
            String OtherCharge = string.Empty;
            String Total = string.Empty;
            String ExchangeRate = string.Empty;
            String SenderFName = string.Empty;
            String SenderMName = string.Empty;
            String SenderLName = string.Empty;
            String ReceiverFName = string.Empty;
            String ReceiverMName = string.Empty;
            String ReceiverLName = string.Empty;
            String ReceiverStreet = string.Empty;
            String ReceiverProvinceCity = string.Empty;
            String ReceiverCountry = string.Empty;
            String SenderStreet = string.Empty;
            String SenderProvinceCity = string.Empty;
            String SenderCountry = string.Empty;
            String SenderBirthDate = string.Empty;
            String IDType = string.Empty;
            String IDNo = string.Empty;
            String ExpiryDate = string.Empty;
            String SenderContactNo = string.Empty;
            String ReceiverContactNo = string.Empty;
            String PaymentType = string.Empty;
            String TransType = string.Empty;
            String CancelledDate = string.Empty;
            String CancelReason = string.Empty;
            String Purpose = string.Empty;
            String xTableName = string.Empty;
            String xTableNameD2B = string.Empty;
            Boolean translogs = false;
            Boolean in365 = false;
            Int32 xcounter = 0;

            using (MySqlConnection con = dbconnetwork.getConnection())
            {
                DateTime DateNow = getServerDateGlobal(false);
                con.Open();
                using (MySqlCommand command = con.CreateCommand())
                {
                    try
                    {

                     
                        DataTable dt = new DataTable();
                        DataSet ds = new DataSet();
                        List<FirstList> listKptn = new List<FirstList>();
                        String[] custArr = new String[7];
                      
                        String from = (DateFrom == "" || DateFrom == null ? DateNow.AddMonths(-6).ToString("yyyy-MM-dd 00:00:00") : (Convert.ToDateTime(DateFrom)).ToString("yyyy-MM-dd 00:00:00"));
                        String to = (DateTo == "" || DateTo == null ? DateNow.ToString("yyyy-MM-dd 23:59:59") : (Convert.ToDateTime(DateTo)).ToString("yyyy-MM-dd 23:59:59"));
                        String df = Convert.ToDateTime(to).AddMonths(-6).ToString("MM");


                        int rr = Convert.ToInt32(df);

                        for (int z = 0; z <= 6; z++)
                        {
                            String xrr = rr.ToString();
                            if (rr < 10)
                                xrr = "0" + xrr;


                            custArr[z] = xrr;

                            if (rr % 12 == 0)
                            {
                                rr = 1;
                                rr--;
                            }

                            rr++;

                        }
                        
                        
                        String query;
                        Int32 i = 0;
                        if (PhoneNo == null) PhoneNo = "";
                        if (AmountMin == null) AmountMin = "";
                        


                        if (AmountMin != "")
                        {

                            query = "where c.Mobile = @PhoneNo AND (Total Between '" + AmountMin + "' and '" + AmountMax + "') AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";

                        }
                      
                        else
                        {
                            query = "where c.Mobile = @PhoneNo AND (TransDate BETWEEN '" + from + "' and '" + to + "')  GROUP BY KPTNNo ORDER BY TransDate DESC;";
                        }


                        //For Regular and D2B Transactions
                        for (i = 0; i < custArr.Length; i++)
                        {



                            command.Parameters.Clear();
                            command.CommandText = "Select TransDate, KPTNNo from kptransactionsglobal.sendout" + custArr[i] + " s left join kpcustomersglobal.customers c ON (s.SenderFName = c.FirstName AND s.SenderLName = c.LastName AND s.SenderMName = c.MiddleName)  " + query;
                            command.Parameters.AddWithValue("AmountMin", AmountMin);
                            command.Parameters.AddWithValue("AmountMax", AmountMax);
                            command.Parameters.AddWithValue("DateFrom", from);
                            command.Parameters.AddWithValue("DateTo", to);
                            command.Parameters.AddWithValue("PhoneNo", PhoneNo);
                            MySqlDataReader rdr2 = command.ExecuteReader();

                             while (rdr2.Read())
                            {
                                listKptn.Add(new FirstList
                                {
                                    kptn = rdr2["KPTNNo"].ToString(),
                                    TransDate = rdr2["TransDate"].ToString()
                                });


                            }

                             rdr2.Close();

                             command.Parameters.Clear();
                             command.CommandText = "Select TransDate, KPTNNo from kptransactionsglobal.sendoutd2b" + custArr[i] + " s left join kpcustomersglobal.customers c ON (s.SenderFName = c.FirstName AND s.SenderLName = c.LastName AND s.SenderMName = c.MiddleName)  " + query;
                             command.Parameters.AddWithValue("AmountMin", AmountMin);
                             command.Parameters.AddWithValue("AmountMax", AmountMax);
                             command.Parameters.AddWithValue("DateFrom", from);
                             command.Parameters.AddWithValue("DateTo", to);
                             command.Parameters.AddWithValue("PhoneNo", PhoneNo);
                             MySqlDataReader rdrd2b = command.ExecuteReader();

                             while (rdrd2b.Read())
                             {
                                 listKptn.Add(new FirstList
                                 {
                                     kptn = rdrd2b["KPTNNo"].ToString(),
                                     TransDate = rdrd2b["TransDate"].ToString()
                                 });


                             }


                             rdrd2b.Close();

                        }

                        con.Close();
                        //Start of KIOSK TRANSACTIONS
                        using(MySqlConnection conKiosk = dbconKiosk.getConnection())
                        {
                            conKiosk.Open();
                            using (MySqlCommand cmd = conKiosk.CreateCommand()) 
                            {
                               
                                i = 0;
                                for (i = 0; i < custArr.Length; i++)
                                {



                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "Select TransDate, KPTNNo from kpkiosktransactionsglobal.sendout" + custArr[i] + " s left join kpcustomersglobal.customers c ON (s.SenderFName = c.FirstName AND s.SenderLName = c.LastName AND s.SenderMName = c.MiddleName)  " + query;
                                    cmd.Parameters.AddWithValue("AmountMin", AmountMin);
                                    cmd.Parameters.AddWithValue("AmountMax", AmountMax);
                                    cmd.Parameters.AddWithValue("DateFrom", from);
                                    cmd.Parameters.AddWithValue("DateTo", to);
                                    cmd.Parameters.AddWithValue("PhoneNo", PhoneNo);
                                    MySqlDataReader rdrKiosk = cmd.ExecuteReader();

                                    while (rdrKiosk.Read())
                                    {
                                        listKptn.Add(new FirstList
                                        {
                                            kptn = rdrKiosk["KPTNNo"].ToString(),
                                            TransDate = rdrKiosk["TransDate"].ToString()
                                        });


                                    }


                                    rdrKiosk.Close();
                                }

                              
                            }

                            conKiosk.Close();
                        }
                        //START OF BILLSPAY  TRANSACTIONS
                        using (MySqlConnection conKiosk = dbconBillspay.getConnection())
                        {
                            conKiosk.Open();
                            using (MySqlCommand cmd = conKiosk.CreateCommand())
                            {

                                i = 0;
                                for (i = 0; i < custArr.Length; i++)
                                {



                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "Select TransDate, KPTN as KPTNNo from kptransactionsglobal.sendoutbillspay" + custArr[i] + " s left join kpcustomersglobal.customers c ON (s.PayorFName = c.FirstName AND s.PayorLName = c.LastName AND s.PayorMName = c.MiddleName)  " + query;
                                    cmd.Parameters.AddWithValue("AmountMin", AmountMin);
                                    cmd.Parameters.AddWithValue("AmountMax", AmountMax);
                                    cmd.Parameters.AddWithValue("DateFrom", from);
                                    cmd.Parameters.AddWithValue("DateTo", to);
                                    cmd.Parameters.AddWithValue("PhoneNo", PhoneNo);
                                    MySqlDataReader rdrKiosk = cmd.ExecuteReader();

                                    while (rdrKiosk.Read())
                                    {
                                        listKptn.Add(new FirstList
                                        {
                                            kptn = rdrKiosk["KPTNNo"].ToString(),
                                            TransDate = rdrKiosk["TransDate"].ToString()
                                        });


                                    }


                                    rdrKiosk.Close();
                                }


                            }

                            conKiosk.Close();
                        }

                        //END OF BILLSPAY TRANSACTIONS 

                        con.Open();

                        for (int x = 0; x < listKptn.Count; x++)
                        {




                            if (listKptn[x].kptn.Substring(0, 3) == "BPG")
                            {
                                TransList response = new TransList();
                                response = getBillspay(listKptn[x].kptn, "", "", WOCancel);

                                if (response != null)
                                {
                                    tl.Add(response);

                                }
                                continue;
                            }



                            translogs = false;

                            if (listKptn[x].kptn == "")
                            {
                                continue;
                            }

                            command.Parameters.Clear();
                            command.CommandText = "select `action` from kpadminlogsglobal.transactionlogs where kptnno='" + listKptn[x].kptn + "' and `action` NOT IN ('PEEP','PO REPRINT','SO REPRINT') order by `timestamp` desc limit 1";
                            MySqlDataReader statusRdr = command.ExecuteReader();
                            if (!statusRdr.HasRows)
                            {

                                translogs = true;
                            }
                            else
                            {
                                statusRdr.Read();
                                Status = statusRdr["action"].ToString();


                            }
                            statusRdr.Close();
                            using (MySqlCommand cmd2 = con.CreateCommand())
                            {
                                cmd2.Parameters.Clear();

                                xTableName = decodeKPTNGlobal(0, listKptn[x].kptn);

                                cmd2.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                      "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                       "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                       "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                       "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                       "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                       "FROM " + xTableName + " where KPTNNO = '" + listKptn[x].kptn + "'; ";

                                MySqlDataReader transRdr = cmd2.ExecuteReader();

                                if (transRdr.HasRows && translogs == false)
                                {
                                    transRdr.Read();
                                    xcounter++;
                                    Int32 isCancelled = Convert.ToInt32(transRdr["isCancelled"]);

                                    if (WOCancel == true && isCancelled == 1) { transRdr.Close(); continue; }


                                    KPTNNO = transRdr["KPTNNO"].ToString();
                                    TransDate = transRdr["TransDate"].ToString();
                                    AmountPO = transRdr["AmountPO"].ToString();
                                    DateClaimed = transRdr["DateClaimed"].ToString();
                                    Principal = transRdr["Principal"].ToString();
                                    Charge = transRdr["Charge"].ToString();
                                    OtherCharge = transRdr["OtherCharge"].ToString();
                                    Total = transRdr["Total"].ToString();
                                    ExchangeRate = transRdr["ExchangeRate"].ToString();
                                    SenderFName = transRdr["SenderFName"].ToString();
                                    SenderMName = transRdr["SenderMName"].ToString();
                                    SenderLName = transRdr["SenderLName"].ToString();
                                    ReceiverFName = transRdr["ReceiverFName"].ToString();
                                    ReceiverMName = transRdr["ReceiverMName"].ToString();
                                    ReceiverLName = transRdr["ReceiverLName"].ToString();
                                    ReceiverStreet = transRdr["ReceiverStreet"].ToString();
                                    ReceiverProvinceCity = transRdr["ReceiverProvinceCity"].ToString();
                                    ReceiverCountry = transRdr["ReceiverCountry"].ToString();
                                    SenderStreet = transRdr["SenderStreet"].ToString();
                                    SenderProvinceCity = transRdr["SenderProvinceCity"].ToString();
                                    SenderCountry = transRdr["SenderCountry"].ToString();
                                    SenderBirthDate = transRdr["SenderBirthDate"].ToString();
                                    IDType = transRdr["IDType"].ToString();
                                    IDNo = transRdr["IDNo"].ToString();
                                    ExpiryDate = transRdr["ExpiryDate"].ToString();
                                    SenderContactNo = PhoneNo;
                                    ReceiverContactNo = transRdr["ReceiverContactNo"].ToString();
                                    PaymentType = transRdr["PaymentType"].ToString();
                                    TransType = transRdr["TransType"].ToString();
                                    CancelledDate = transRdr["CancelledDate"].ToString();
                                    CancelReason = transRdr["CancelReason"].ToString();
                                    Purpose = transRdr["Purpose"].ToString();


                                    transRdr.Close();

                                }
                                else
                                {
                                    transRdr.Close();
                                    xTableNameD2B = decodeKPTNGlobald2b(0, listKptn[x].kptn);

                                    cmd2.Parameters.Clear();
                                    cmd2.CommandText = "select RejectedDate, KPTNNO, TransDate, AmountPO, " +
                                                      "CompletedDate as DateClaimed, Principal, Charge, " +
                                                       "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                       "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                       "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                       "SenderContactNo,ReceiverContactNo,PaymentType,'INTERNATIONAL' as TransType,CancelledDate,CancelReason,Purpose " +
                                                       "FROM " + xTableNameD2B + " where KPTNNO = '" + listKptn[x].kptn + "'; ";

                                    MySqlDataReader d2brdr = cmd2.ExecuteReader();

                                    if (d2brdr.HasRows && translogs == false)
                                    {
                                        d2brdr.Read();


                                        xcounter++;

                                        String RejectedDate = d2brdr["RejectedDate"].ToString();
                                        CancelledDate = d2brdr["CancelledDate"].ToString();

                                        if (WOCancel == true && (RejectedDate != "" || CancelledDate != ""))
                                        {
                                            d2brdr.Close(); continue;
                                        }

                                        KPTNNO = d2brdr["KPTNNO"].ToString();
                                        TransDate = d2brdr["TransDate"].ToString();
                                        AmountPO = d2brdr["AmountPO"].ToString();
                                        DateClaimed = d2brdr["DateClaimed"].ToString();
                                        Principal = d2brdr["Principal"].ToString();
                                        Charge = d2brdr["Charge"].ToString();
                                        OtherCharge = d2brdr["OtherCharge"].ToString();
                                        Total = d2brdr["Total"].ToString();
                                        ExchangeRate = d2brdr["ExchangeRate"].ToString();
                                        SenderFName = d2brdr["SenderFName"].ToString();
                                        SenderMName = d2brdr["SenderMName"].ToString();
                                        SenderLName = d2brdr["SenderLName"].ToString();
                                        ReceiverFName = d2brdr["ReceiverFName"].ToString();
                                        ReceiverMName = d2brdr["ReceiverMName"].ToString();
                                        ReceiverLName = d2brdr["ReceiverLName"].ToString();
                                        ReceiverStreet = d2brdr["ReceiverStreet"].ToString();
                                        ReceiverProvinceCity = d2brdr["ReceiverProvinceCity"].ToString();
                                        ReceiverCountry = d2brdr["ReceiverCountry"].ToString();
                                        SenderStreet = d2brdr["SenderStreet"].ToString();
                                        SenderProvinceCity = d2brdr["SenderProvinceCity"].ToString();
                                        SenderCountry = d2brdr["SenderCountry"].ToString();
                                        SenderBirthDate = d2brdr["SenderBirthDate"].ToString();
                                        IDType = d2brdr["IDType"].ToString();
                                        IDNo = d2brdr["IDNo"].ToString();
                                        ExpiryDate = d2brdr["ExpiryDate"].ToString();
                                        SenderContactNo = PhoneNo;
                                        ReceiverContactNo = d2brdr["ReceiverContactNo"].ToString();
                                        PaymentType = d2brdr["PaymentType"].ToString();
                                        TransType = d2brdr["TransType"].ToString();
                                        //  CancelledDate = d2brdr["CancelledDate"].ToString();
                                        CancelReason = d2brdr["CancelReason"].ToString();
                                        Purpose = d2brdr["Purpose"].ToString();

                                        d2brdr.Close();

                                    }
                                    //Start of KIOSK Transactions
                                    else
                                    {

                                        con.Close();

                                        using (MySqlConnection conKiosk = dbconKiosk.getConnection())
                                        {
                                            String xTableKiosk = "";
                                            conKiosk.Open();
                                            using (MySqlCommand cmd = conKiosk.CreateCommand())
                                            {
                                                xTableKiosk = decodeKPTNGlobalKiosk(listKptn[x].kptn);

                                                cmd.Parameters.Clear();
                                                cmd.CommandText = "select isCancelled, KPTNNO, TransDate, AmountPO, " +
                                                          "IF(IsClaimed = '1',sysmodified,null) as DateClaimed, Principal, Charge, " +
                                                           "OtherCharge, Total,ExchangeRate,SenderFName,SenderMName,SenderLName,ReceiverFName, " +
                                                           "ReceiverMName,ReceiverLName,ReceiverStreet,ReceiverProvinceCity,ReceiverCountry, " +
                                                           "SenderStreet,SenderProvinceCity,SenderCountry,SenderBirthDate, IDType,IDNo, ExpiryDate, " +
                                                           "SenderContactNo,ReceiverContactNo,PaymentType,TransType,CancelledDate,CancelReason,Purpose " +
                                                           "FROM " + xTableKiosk + " where KPTNNO = '" + listKptn[x].kptn + "'; ";

                                                MySqlDataReader kioskRdr = cmd.ExecuteReader();

                                                if (kioskRdr.HasRows && translogs == false)
                                                {
                                                    kioskRdr.Read();

                                                    xcounter++;
                                                    Int32 isCancelled = Convert.ToInt32(kioskRdr["isCancelled"]);

                                                    if (WOCancel == true && isCancelled == 1) { kioskRdr.Close(); conKiosk.Close(); con.Open(); continue; }

                                                    KPTNNO = kioskRdr["KPTNNO"].ToString();
                                                    TransDate = kioskRdr["TransDate"].ToString();
                                                    AmountPO = kioskRdr["AmountPO"].ToString();
                                                    DateClaimed = kioskRdr["DateClaimed"].ToString();
                                                    Principal = kioskRdr["Principal"].ToString();
                                                    Charge = kioskRdr["Charge"].ToString();
                                                    OtherCharge = kioskRdr["OtherCharge"].ToString();
                                                    Total = kioskRdr["Total"].ToString();
                                                    ExchangeRate = kioskRdr["ExchangeRate"].ToString();
                                                    SenderFName = kioskRdr["SenderFName"].ToString();
                                                    SenderMName = kioskRdr["SenderMName"].ToString();
                                                    SenderLName = kioskRdr["SenderLName"].ToString();
                                                    ReceiverFName = kioskRdr["ReceiverFName"].ToString();
                                                    ReceiverMName = kioskRdr["ReceiverMName"].ToString();
                                                    ReceiverLName = kioskRdr["ReceiverLName"].ToString();
                                                    ReceiverStreet = kioskRdr["ReceiverStreet"].ToString();
                                                    ReceiverProvinceCity = kioskRdr["ReceiverProvinceCity"].ToString();
                                                    ReceiverCountry = kioskRdr["ReceiverCountry"].ToString();
                                                    SenderStreet = kioskRdr["SenderStreet"].ToString();
                                                    SenderProvinceCity = kioskRdr["SenderProvinceCity"].ToString();
                                                    SenderCountry = kioskRdr["SenderCountry"].ToString();
                                                    SenderBirthDate = kioskRdr["SenderBirthDate"].ToString();
                                                    IDType = kioskRdr["IDType"].ToString();
                                                    IDNo = kioskRdr["IDNo"].ToString();
                                                    ExpiryDate = kioskRdr["ExpiryDate"].ToString();
                                                    SenderContactNo = PhoneNo;
                                                    ReceiverContactNo = kioskRdr["ReceiverContactNo"].ToString();
                                                    PaymentType = kioskRdr["PaymentType"].ToString();
                                                    TransType = kioskRdr["TransType"].ToString();
                                                    //  CancelledDate = d2brdr["CancelledDate"].ToString();
                                                    CancelReason = kioskRdr["CancelReason"].ToString();
                                                    Purpose = kioskRdr["Purpose"].ToString();

                                                    kioskRdr.Close();


                                                }
                                                else
                                                {
                                                    kioskRdr.Close();
                                                    con.Open();
                                                    continue;
                                                }


                                            }

                                            con.Open();

                                        }
                                        //END of KIOSK Transactions

                                    }

                                }
                                String BDate = SenderBirthDate == "" ? String.Empty : Convert.ToDateTime(SenderBirthDate).ToString("yyyy-MM-dd");
                                command.Parameters.Clear();
                                command.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.FirstName = @FName and c.LastName = @LName and c.MiddleName = @MName and DATE_FORMAT(c.Birthdate,'%Y-%m-%d') = @BDate;";
                                command.Parameters.AddWithValue("FName", SenderFName);
                                command.Parameters.AddWithValue("LName", SenderLName);
                                command.Parameters.AddWithValue("MName", SenderMName);
                                command.Parameters.AddWithValue("BDate", BDate);
                                MySqlDataReader custRdr = command.ExecuteReader();

                                if (!custRdr.HasRows)
                                {
                                    HomeCity = "";
                                    ZipCode = "";
                                    Occupation = "";
                                    SSN = "";
                                }
                                else
                                {
                                    custRdr.Read();
                                    HomeCity = custRdr["HomeCity"].ToString();
                                    ZipCode = custRdr["ZipCode"].ToString();
                                    Occupation = custRdr["Occupation"].ToString();
                                    SSN = custRdr["SSN"].ToString();
                                }
                                custRdr.Close();

                                tl.Add(new TransList
                                {

                                    kptnno = KPTNNO,
                                    TransDate = TransDate,
                                    POAmount = AmountPO,
                                    DateClaimed = DateClaimed,
                                    Principal = Principal,
                                    Charge = Charge,
                                    Othercharge = OtherCharge,
                                    Total = Total,
                                    ExchangeRate = ExchangeRate,
                                    SenderFName = SenderFName,
                                    SenderMName = SenderMName,
                                    SenderLName = SenderLName,
                                    ReceiverFName = ReceiverFName,
                                    ReceiverMName = ReceiverMName,
                                    ReceiverLName = ReceiverLName,
                                    ReceiverStreet = ReceiverStreet,
                                    ReceiverProvinceCity = ReceiverProvinceCity,
                                    ReceiverCountry = ReceiverCountry,
                                    SenderStreet = SenderStreet,
                                    SenderProvinceCity = SenderProvinceCity,
                                    SenderCountry = SenderCountry,
                                    SenderBirthDate = SenderBirthDate,
                                    IDType = IDType,
                                    IDNo = IDNo,
                                    ExpiryDate = ExpiryDate,
                                    SenderContactNo = SenderContactNo,
                                    ReceiverContactNo = ReceiverContactNo,
                                    PaymentType = PaymentType,
                                    TransType = TransType,
                                    CancelledDate = CancelledDate,
                                    CancelReason = CancelReason,
                                    Purpose = Purpose,
                                    City = HomeCity,
                                    occupation = Occupation,
                                    SSN = SSN,
                                    Status = Status,
                                    ZipCode = ZipCode

                                });

                            }


                           
                        }

                        if (listKptn.Count == 0)
                        {
                            return new SeccomResponse { respcode = 0, message = "No Data Found!", data = null };
                        }



                        con.Close();
                        return new SeccomResponse { respcode = 1, message = "Success!", data = new List<TransList>(tl), RowsCount = tl.Count };





                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
                    }


                }

            }

        }
        catch (Exception ex)
        {
            dbconnetwork.CloseConnection();
            return new SeccomResponse { respcode = 0, message = ex.ToString(), data = null };
        }

    }

    private TransList getBillspay(String KPTN, String AmountMin, String AmountMax, Boolean WOCancel)
    {
        Boolean translogs = false;
        TransList result = new TransList();
        String HomeCity = string.Empty;
        String ZipCode = string.Empty;
        String Occupation = string.Empty;
        String SSN = string.Empty;
        String Status = string.Empty;
        String KPTNNO = string.Empty;
        String TransDate = string.Empty;
        String AmountPO = string.Empty;
        String DateClaimed = string.Empty;
        String Principal = string.Empty;
        String Charge = string.Empty;
        String OtherCharge = string.Empty;
        String Total = string.Empty;
        String ExchangeRate = string.Empty;
        String SenderFName = string.Empty;
        String SenderMName = string.Empty;
        String SenderLName = string.Empty;
        String ReceiverFName = string.Empty;
        String ReceiverMName = string.Empty;
        String ReceiverLName = string.Empty;
        String ReceiverStreet = string.Empty;
        String ReceiverProvinceCity = string.Empty;
        String ReceiverCountry = string.Empty;
        String SenderStreet = string.Empty;
        String SenderProvinceCity = string.Empty;
        String SenderCountry = string.Empty;
        String SenderBirthDate = string.Empty;
        String IDType = string.Empty;
        String IDNo = string.Empty;
        String ExpiryDate = string.Empty;
        String SenderContactNo = string.Empty;
        String ReceiverContactNo = string.Empty;
        String PaymentType = string.Empty;
        String TransType = string.Empty;
        String CancelledDate = string.Empty;
        String CancelReason = string.Empty;
        String Purpose = string.Empty;
        String xTableName = string.Empty;
        String xTableNameD2B = string.Empty;
        Boolean in365 = false;
        String SenderCustID = string.Empty;
        Int32 xcounter = 0;

        using (MySqlConnection con = dbconBillspay.getConnection())
        {
            con.Open();
            using (MySqlCommand command = con.CreateCommand())
            {
                command.Parameters.Clear();
                command.CommandText = "select `action` from kpadminpartnerslog.transactionslogs where kptnno='" + KPTN + "' and `action` NOT IN ('PEEP','PO REPRINT','SO REPRINT') order by txndate desc limit 1;";
                MySqlDataReader statusRdr = command.ExecuteReader();

                if (!statusRdr.HasRows)
                {

                    translogs = true;
                }
                else
                {
                    statusRdr.Read();
                    Status = statusRdr["action"].ToString();


                }
                statusRdr.Close();

            }

            using (MySqlCommand cmd2 = con.CreateCommand())
            {
                cmd2.Parameters.Clear();

                xTableName = decodeKPTNGlobalBillspay(KPTN);

                if (AmountMin != "")
                {

                    cmd2.CommandText = "Select IF(CancelReason is null,0,1) as isCancelled, KPTNNO, TransDate,'' as DateClaimed, AmountPaid as Principal, IF(CustomerCharge = '0.00',PartnerCharge,CustomerCharge) as Charge,'0.00' as OtherCharge, Total,ExchangeRate,SenderCustID,AccountFName as ReceiverFName, " +
                                           "AccountMName as ReceiverMName,AccountLName as ReceiverLName,'CASH' as PaymentType,'INTERNATIONAL' as TransType,CancelledDate,CancelReason,'' as Purpose " +
                                           "FROM " + xTableName + " where KPTNNO = '" + KPTN + "' AND (Total between '" + AmountMin + "' and '" + AmountMax + "');";
                }
                else
                {
                    cmd2.CommandText = "Select IF(CancelReason is null,0,1) as isCancelled, KPTNNO, TransDate,'' as DateClaimed, AmountPaid as Principal, IF(CustomerCharge = '0.00',PartnerCharge,CustomerCharge) as Charge,'0.00' as OtherCharge, Total,ExchangeRate,SenderCustID,AccountFName as ReceiverFName, " +
                                        "AccountMName as ReceiverMName,AccountLName as ReceiverLName,'CASH' as PaymentType,'INTERNATIONAL' as TransType,CancelledDate,CancelReason,'' as Purpose " +
                                        "FROM " + xTableName + " where KPTNNO = '" + KPTN + "'; ";
                }

                MySqlDataReader transRdr = cmd2.ExecuteReader();

                if (transRdr.HasRows && translogs == false)
                {
                    transRdr.Read();
                    xcounter++;
                    Int32 isCancelled = Convert.ToInt32(transRdr["isCancelled"]);

                    if (WOCancel == true && isCancelled == 1) { transRdr.Close(); return null; }


                    KPTNNO = transRdr["KPTNNO"].ToString();
                    TransDate = transRdr["TransDate"].ToString();
                    AmountPO = "";
                    DateClaimed = transRdr["DateClaimed"].ToString();
                    Principal = transRdr["Principal"].ToString();
                    Charge = transRdr["Charge"].ToString();
                    OtherCharge = transRdr["OtherCharge"].ToString();
                    Total = transRdr["Total"].ToString();
                    ExchangeRate = transRdr["ExchangeRate"].ToString();
                    SenderCustID = transRdr["SenderCustID"].ToString();
                    ReceiverFName = transRdr["ReceiverFName"].ToString();
                    ReceiverMName = transRdr["ReceiverMName"].ToString();
                    ReceiverLName = transRdr["ReceiverLName"].ToString();
                    ReceiverStreet = "";
                    ReceiverProvinceCity = "";
                    ReceiverCountry = "";
                    ReceiverContactNo = "";
                    PaymentType = transRdr["PaymentType"].ToString();
                    TransType = transRdr["TransType"].ToString();
                    CancelledDate = transRdr["CancelledDate"].ToString();
                    CancelReason = transRdr["CancelReason"].ToString();
                    Purpose = transRdr["Purpose"].ToString();

                    transRdr.Close();


                    cmd2.Parameters.Clear();
                    cmd2.CommandText = "select d.HomeCity, c.ZipCode, d.Occupation, d.SSN, c.FirstName,c.LastName,c.MiddleName,c.Street as SenderStreet,c.ProvinceCity as SenderProvinceCity,c.Country as SenderCountry,c.BirthDate as SenderBirthDate,c.IDType,c.IDNo,c.ExpiryDate,c.Mobile as SenderContactNo from kpcustomersglobal.customers c left join kpcustomersglobal.customersdetails d ON c.CustID = d.CustID where c.CustID = @custID";
                    cmd2.Parameters.AddWithValue("custID", SenderCustID);

                    MySqlDataReader custRdr = cmd2.ExecuteReader();

                    if (!custRdr.HasRows)
                    {
                        HomeCity = "";
                        ZipCode = "";
                        Occupation = "";
                        SSN = "";
                        
                    }
                    else
                    {
                        custRdr.Read();
                        SenderFName = custRdr["FirstName"].ToString();
                        SenderLName = custRdr["LastName"].ToString();
                        SenderMName = custRdr["MiddleName"].ToString();
                        HomeCity = custRdr["HomeCity"].ToString();
                        ZipCode = custRdr["ZipCode"].ToString();
                        Occupation = custRdr["Occupation"].ToString();
                        SSN = custRdr["SSN"].ToString();
                        SenderStreet = custRdr["SenderStreet"].ToString();
                        SenderProvinceCity = custRdr["SenderProvinceCity"].ToString();
                        SenderCountry = custRdr["SenderCountry"].ToString();
                        SenderBirthDate = custRdr["SenderBirthDate"].ToString();
                        IDType = custRdr["IDType"].ToString();
                        IDNo = custRdr["IDNo"].ToString();
                        ExpiryDate = custRdr["ExpiryDate"].ToString();
                        SenderContactNo = custRdr["SenderContactNo"].ToString();


                    }
                    custRdr.Close();



                }


                String BDate = SenderBirthDate == "" ? String.Empty : Convert.ToDateTime(SenderBirthDate).ToString("yyyy-MM-dd");

                result = new TransList
                {

                    kptnno = KPTNNO,
                    TransDate = TransDate,
                    POAmount = AmountPO,
                    DateClaimed = DateClaimed,
                    Principal = Principal,
                    Charge = Charge,
                    Othercharge = OtherCharge,
                    Total = Total,
                    ExchangeRate = ExchangeRate,
                    SenderFName = SenderFName,
                    SenderMName = SenderMName,
                    SenderLName = SenderLName,
                    ReceiverFName = ReceiverFName,
                    ReceiverMName = ReceiverMName,
                    ReceiverLName = ReceiverLName,
                    SenderStreet = SenderStreet,
                    SenderProvinceCity = SenderProvinceCity,
                    SenderCountry = SenderCountry,
                    SenderBirthDate = BDate,
                    IDType = IDType,
                    IDNo = IDNo,
                    ExpiryDate = ExpiryDate,
                    SenderContactNo = SenderContactNo,
                    ReceiverContactNo = ReceiverContactNo,
                    PaymentType = PaymentType,
                    TransType = TransType,
                    CancelledDate = CancelledDate,
                    CancelReason = CancelReason,
                    Purpose = Purpose,
                    City = HomeCity,
                    occupation = Occupation,
                    SSN = SSN,
                    Status = Status,
                    ZipCode = ZipCode

                };

                return result;
            }

        }



    }


    private String generateTableNameGlobal(Int32 type, String TransDate)
    {
        //DateTime dt = getServerDate(false);

    
            DateTime TransDatetoDate = Convert.ToDateTime(TransDate);
            if (type == 0)
            {
                kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.sendout" : "kpglobal.sendout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd")));
                return (isUse365Global == 0) ? "kpglobal.sendout" : "kpglobal.sendout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
            }
            else if (type == 1)
            {
                kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.payout" : "kpglobal.payout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd")));
                return (isUse365Global == 0) ? "kpglobal.payout" : "kpglobal.payout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
            }
            else if (type == 2)
            {
                kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.tempkptn" : "kpglobal.tempkptn"));
                return (isUse365Global == 0) ? "kpglobal.tempkptn" : "kpglobal.tempkptn";
            }
            else
            {
                kplog.Error("FAILED:: message: Invalid transaction type");
                throw new Exception("Invalid transaction type");
            }
        }

    private String generateTableNameD2B(Int32 type, String TransDate)
    {

            DateTime TransDatetoDate = Convert.ToDateTime(TransDate);
            if (type == 0)
            {
                return (isUse365Global == 0) ? "kpglobal.sendoutd2b" : "kpglobal.sendoutd2b" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
            }
            else if (type == 1)
            {
                return (isUse365Global == 0) ? "kpglobal.payoutd2b" : "kpglobal.payoutd2b" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
            }
            else if (type == 2)
            {
                return (isUse365Global == 0) ? "kpglobal.tempkptn" : "kpglobal.tempkptn";
            }
            else
            {
                kplog.Error("Invalid transaction type");
                throw new Exception("Invalid transaction type");
            }
        
    }
    
    private DateTime getServerDateGlobal(Boolean isOpenConnection)
    {

        try
        {
            //throw new Exception(isOpenConnection.ToString());
            if (!isOpenConnection)
            {
                using (MySqlConnection conn = dbconnetwork.getConnection())
                {
                    conn.Open();
                    using (MySqlCommand command = conn.CreateCommand())
                    {

                        DateTime serverdate;

                        command.CommandText = "Select NOW() as serverdt;";
                        using (MySqlDataReader Reader = command.ExecuteReader())
                        {
                            Reader.Read();

                            serverdate = Convert.ToDateTime(Reader["serverdt"]);
                            Reader.Close();
                            conn.Close();

                            return serverdate;
                        }

                    }
                }
            }
            else
            {

                DateTime serverdate;

                command.CommandText = "Select NOW() as serverdt;";

                using (MySqlDataReader Reader = command.ExecuteReader())
                {
                    Reader.Read();
                    serverdate = Convert.ToDateTime(Reader["serverdt"]);
                    Reader.Close();
                    return serverdate;
                }


            }

        }
        catch (Exception ex)
        {
            kplog.Fatal("FAILED:: respcode: 0, message: " + ex.ToString());
            throw new Exception(ex.Message);
        }
    }

    public String getcustomertable(String lastname)
    {
        String customers = "";
        if (lastname.StartsWith("A") || lastname.StartsWith("B") || lastname.StartsWith("C"))
        {
            customers = "AtoC";
        }
        else if (lastname.StartsWith("D") || lastname.StartsWith("E") || lastname.StartsWith("F"))
        {
            customers = "DtoF";
        }
        else if (lastname.StartsWith("G") || lastname.StartsWith("H") || lastname.StartsWith("I"))
        {
            customers = "GtoI";
        }
        else if (lastname.StartsWith("J") || lastname.StartsWith("K") || lastname.StartsWith("L"))
        {
            customers = "JtoL";
        }
        else if (lastname.StartsWith("M") || lastname.StartsWith("N") || lastname.StartsWith("O"))
        {
            customers = "MtoO";
        }
        else if (lastname.StartsWith("P") || lastname.StartsWith("Q") || lastname.StartsWith("R"))
        {
            customers = "PtoR";
        }
        else if (lastname.StartsWith("S") || lastname.StartsWith("T") || lastname.StartsWith("U"))
        {
            customers = "StoU";
        }
        else if (lastname.StartsWith("V") || lastname.StartsWith("W") || lastname.StartsWith("X"))
        {
            customers = "VtoX";
        }
        else if (lastname.StartsWith("Y") || lastname.StartsWith("Z"))
        {
            customers = "YtoZ";
        }
        return customers;
    }

    public String getRespMessage(Int32 code)
    {
        String x = "SYSTEM_ERROR";
        switch (code)
        {
            case 1:
                return x = "Success";
            case 2:
                return x = "Duplicate kptn";
            case 3:
                return x = "KPTN already claimed";
            case 4:
                return x = "KPTN not found";
            case 5:
                return x = "Customer not found";
            case 6:
                return x = "Customer already exist";
            case 7:
                return x = "Invalid credentials";
            case 8:
                return x = "KPTN already cancelled";
            case 9:
                return x = "Transaction is not yet claimed";
            case 10:
                return x = "Version does not match";
            case 11:
                return x = "Problem occured during saving. Please resave the transaction.";
            case 12:
                return x = "Problem saving transaction. Please close the sendout form and open it again. Thank you.";
            case 13:
                return x = "Invalid station number.";
            case 14:
                return x = "Error generating receipt number.";
            case 15:
                return x = "Unable to save transaction. Invalid amount provided.";
            default:
                return x;
        }
    }

    private String decodeKPTNGlobal(int type, String kptn)
    {
        try
        {
            String month = kptn.Substring(kptn.Length - 2, 2);
            String day = kptn.Substring(6, 2);
            int x = Convert.ToInt32(month);
            int y = Convert.ToInt32(day);
            if (type == 0)
            {

                if (x > 12 || x < 0 || x == 0)
                {
                    throw new Exception("4");
                }
                else if (y > 31 || y < 0 || y == 0)
                {
                    throw new Exception("4");
                }
                else
                {
                    kplog.Info("SUCCESS:: decodeKPTNGlobal: " + ("kpglobal.sendout" + month + day));
                    return "kpglobal.sendout" + month + day;
                }

            }
            else if (type == 1)
            {

                if (x > 12 || x < 0 || x == 0)
                {
                    throw new Exception("4");
                }
                else if (y > 31 || y < 0 || y == 0)
                {
                    throw new Exception("4");
                }
                else
                {
                    kplog.Info("SUCCESS:: decodeKPTNGlobal: " + ("kpglobal.payout" + month + day));
                    return "kpglobal.payout" + month + day;
                }
            }
            else
            {
                kplog.Error("FAILED:: message: Invalid type");
                throw new Exception("invalid type");
            }
        }
        catch (Exception ex)
        {
            kplog.Fatal("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
            throw new Exception("4");
        }
    }

    private String decodeKPTNGlobald2b(int type, String kptn)
    {
        try
        {

            String month = kptn.Substring(kptn.Length - 2, 2);
            String day = kptn.Substring(6, 2);
            int x = Convert.ToInt32(month);
            int y = Convert.ToInt32(day);
            if (type == 0)
            {

                if (x > 12 || x < 0 || x == 0)
                {
                    throw new Exception("4");
                }
                else if (y > 31 || y < 0 || y == 0)
                {
                    throw new Exception("4");
                }
                else
                {
                    kplog.Info("SUCCESS:: decodeKPTNGlobal: kpglobal.sendoutd2b" + month + day);
                    return "kpglobal.sendoutd2b" + month + day;
                }

            }
            else if (type == 1)
            {

                if (x > 12 || x < 0 || x == 0)
                {
                    throw new Exception("4");
                }
                else if (y > 31 || y < 0 || y == 0)
                {
                    throw new Exception("4");
                }
                else
                {
                    kplog.Info("SUCCESS:: decodeKPTNGlobal: kpglobal.payoutd2b" + month + day);
                    return "kpglobal.payoutd2b" + month + day;
                }
            }
            else
            {
                kplog.Error("FAILED:: ddecodeKPTNGlobal: Invalid type");
                throw new Exception("invalid type");
            }
        }
        catch (Exception ex)
        {
            kplog.Error("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
            throw new Exception("4");
        }
    }

    private String decodeKPTNGlobalKiosk(String kptn)
    {
        try
        {

            String month = kptn.Substring(kptn.Length - 2, 2);
            String day = kptn.Substring(6, 2);
            int x = Convert.ToInt32(month);
            int y = Convert.ToInt32(day);


            if (x > 12 || x < 0 || x == 0)
            {
                throw new Exception("4");
            }
            else if (y > 31 || y < 0 || y == 0)
            {
                throw new Exception("4");
            }
            else
            {
                kplog.Info("SUCCESS:: kpkioskglobal: " + ("kpkioskglobal.sendout" + month));
                return "kpkioskglobal.sendout" + month + day;
            }

        }
        catch (Exception ex)
        {
            kplog.Fatal("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
            throw new Exception("4");
        }
    }

    private String decodeKPTNGlobalBillspay(String kptn)
    {
        try
        {

            String month = kptn.Substring(kptn.Length - 2, 2);
            String day = kptn.Substring(6, 2);
            int x = Convert.ToInt32(month);
            int y = Convert.ToInt32(day);


            if (x > 12 || x < 0 || x == 0)
            {
                throw new Exception("4");
            }
            else if (y > 31 || y < 0 || y == 0)
            {
                throw new Exception("4");
            }
            else
            {
                kplog.Info("SUCCESS:: decodeKPTNGlobalBillspay: " + ("kpbillspayment.sendout" + month));
                return "kpbillspayment.sendout" + month + day;
            }

        }
        catch (Exception ex)
        {
            kplog.Fatal("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
            throw new Exception("4");
        }
    }



    

    #endregion

    #region CTR

    public TransReviewResponse getCTR(String Username, String Password, String Display) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass)
            {
                return new TransReviewResponse { respcode = 7, respmsg = "UnAuthorized WebserviceUser" };
            }


            if (Display.ToUpper() == "PENDING")
            {

                String Name = string.Empty;
                List<CTRList> ctr = new List<CTRList>();
                using (MySqlConnection con = dbconCTR.getConnection())
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        try
                        {
                            Boolean filestat = false;
                            cmd.Parameters.Clear();
                            cmd.CommandText = "Select TransDate,FullName,CustomerType,Amount,FileName FROM kpglobal.ctrheader where Status = 'PENDING' AND Amount > 10000";
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (!rdr.HasRows)
                            {
                                return new TransReviewResponse { respcode = 0, respmsg = "No Pending CTR", data = null };
                            }

                            while (rdr.Read())
                            {
                                filestat = false;
                                if (rdr["FileName"].ToString() != "" || rdr["FileName"].ToString() != string.Empty) 
                                {
                                    filestat = true;
                                }
                                
                                ctr.Add(new CTRList
                                {
                                    Name = rdr["FullName"].ToString(),
                                    Amount = Convert.ToDouble(rdr["Amount"]),
                                    CustomerType = rdr["CustomerType"].ToString(),
                                    TransDate = Convert.ToDateTime(rdr["TransDate"]).ToString("yyyy-MM-dd"),
                                    FileStat = filestat
                                    

                                });

                            }
                            rdr.Close();
                            con.Close();

                            return new TransReviewResponse { respcode = 1, respmsg = "Success!", data = new List<CTRList>(ctr) };
                        }
                        catch (Exception ex)
                        {

                            return new TransReviewResponse { respcode = 0, respmsg = "Error!", errordetail = ex.ToString() };
                        }

                    }
                }

            }
            else if (Display.ToUpper() == "VALIDATION") 
            {
                String Name = string.Empty;
                List<CTRList> ctr = new List<CTRList>();
                using (MySqlConnection con = dbconCTR.getConnection())
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        try
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "Select TransDate,FullName,CustomerType,Amount,Resource,DateFiled,ReviewStatus,Reason,FileName FROM kpglobal.ctrheader where Status = 'VALIDATION' AND Amount > 10000";
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (!rdr.HasRows)
                            {
                                return new TransReviewResponse { respcode = 0, respmsg = "No VALIDATION CTR", data = null };
                            }

                            while (rdr.Read())
                            {
                                string rasonchcker = string.Empty;
                                string revchcker = rdr["ReviewStatus"].ToString();
                                if(revchcker == "DENY")
                                {
                                   rasonchcker = rdr["Reason"].ToString();
                                }
                                else 
                                {
                                    rasonchcker = null;
                                }

                                ctr.Add(new CTRList
                                {
                                    Name = rdr["FullName"].ToString(),
                                    Amount = Convert.ToDouble(rdr["Amount"]),
                                    CustomerType = rdr["CustomerType"].ToString(),
                                    TransDate = Convert.ToDateTime(rdr["TransDate"]).ToString("yyyy-MM-dd"),
                                    DateFiled = rdr["DateFiled"].ToString(),
                                    Resource = rdr["Resource"].ToString(),
                                    ReviewStatus = rdr["ReviewStatus"].ToString(),
                                    FileName = rdr["FileName"].ToString(),
                                    Reason = rasonchcker

                                });

                            }
                            rdr.Close();
                            con.Close();

                            return new TransReviewResponse { respcode = 1, respmsg = "Success!", data = new List<CTRList>(ctr) };
                        }
                        catch (Exception ex)
                        {

                            return new TransReviewResponse { respcode = 0, respmsg = "Error!", errordetail = ex.ToString() };
                        }

                    }
                }
            }
            else if (Display.ToUpper() == "QUEUED")
            {
                String Name = string.Empty;
                List<CTRList> ctr = new List<CTRList>();
                using (MySqlConnection con = dbconCTR.getConnection())
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        try
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "Select TransDate,FullName,CustomerType,Amount,Resource,DateFiled,ReviewedBy,DateReviewed,FileName FROM kpglobal.ctrheader where Status = 'QUEUED' AND Amount > 10000";
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (!rdr.HasRows)
                            {
                                return new TransReviewResponse { respcode = 0, respmsg = "No QUEUED CTR", data = null };
                            }

                            while (rdr.Read())
                            {


                                ctr.Add(new CTRList
                                {
                                    Name = rdr["FullName"].ToString(),
                                    Amount = Convert.ToDouble(rdr["Amount"]),
                                    CustomerType = rdr["CustomerType"].ToString(),
                                    TransDate = Convert.ToDateTime(rdr["TransDate"]).ToString("yyyy-MM-dd"),
                                    DateFiled = rdr["DateFiled"].ToString(),
                                    Resource = rdr["Resource"].ToString(),
                                    DateReviewed = rdr["DateReviewed"].ToString(),
                                    ReviewedBy = rdr["ReviewedBy"].ToString(),
                                    FileName = rdr["FileName"].ToString()


                                });

                            }
                            rdr.Close();
                            con.Close();

                            return new TransReviewResponse { respcode = 1, respmsg = "Success!", data = new List<CTRList>(ctr) };
                        }
                        catch (Exception ex)
                        {

                            return new TransReviewResponse { respcode = 0, respmsg = "Error!", errordetail = ex.ToString() };
                        }

                    }
                }
            }
            else 
            {
                return new TransReviewResponse { respcode = 0, respmsg = "Wrong Display Type!" };
            }

           
        }
        catch (Exception ex)
        {

            return new TransReviewResponse { respcode = 0 , respmsg = "Error!", errordetail = ex.ToString()};
        }
    }

    public TransReviewResponse getCtrDetails(String Username, String Password, DateTime TransDate, String CustomerType, String Name) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass)
            {
                return new TransReviewResponse { respcode = 7, respmsg = "UnAuthorized WebserviceUser" };
            }

            String Sqlquery = string.Empty;
            using(MySqlConnection con = dbconCTR.getConnection())
            {
                con.Open();
                List<CTRList> ctr = new List<CTRList>();
                String TransDateFinal = TransDate.ToString("yyyy-MM-dd");
                using(MySqlCommand cmd = con.CreateCommand())
                {
                    if (CustomerType.ToUpper() == "SENDER")
                    {
                        Sqlquery = "SELECT TransDate,KPTN,  SenderFullName, ReceiverFullName, Amount, SOBranch FROM kpglobal.ctrdetails WHERE CtrDate = '" + TransDateFinal + "' AND SenderFullName = @Name AND `STATUS` = 'ACTIVE' AND `Action` = 'SENDOUT';";

                    }
                    else if (CustomerType.ToUpper() == "RECEIVER")
                    {
                        Sqlquery = "SELECT TransDate,KPTN,  SenderFullName, ReceiverFullName, Amount, SOBranch FROM kpglobal.ctrdetails WHERE CtrDate = '" + TransDateFinal + "' AND ReceiverFullName = @Name AND `STATUS` = 'ACTIVE' And `Action` = 'PAYOUT';";
                    }
                    else
                        return new TransReviewResponse { respcode = 0 ,respmsg = "Wrong CustomerType!"};

                    cmd.Parameters.Clear();
                    cmd.CommandText = Sqlquery;
                    cmd.Parameters.AddWithValue("Name",Name);
                    
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (!rdr.HasRows) 
                    {
                        return new TransReviewResponse { respcode = 0 , respmsg = "No record found!"};
                    }
                    while (rdr.Read()) 
                    {

                        ctr.Add(new CTRList 
                        {
                            TransDate = Convert.ToDateTime(rdr["TransDate"]).ToString("yyyy-MM-dd"),
                            KPTN = rdr["KPTN"].ToString(),
                            SenderFullName = rdr["SenderFullName"].ToString(),
                            ReceiverFullName = rdr["ReceiverFullName"].ToString(),
                            Amount = Convert.ToDouble(rdr["Amount"]),
                            SOBranch = rdr["SOBranch"].ToString()
                        });
                    }
                    rdr.Close();

                    return new TransReviewResponse { respcode = 1 , respmsg = "Success!", data = new List<CTRList>(ctr)};

                
                }
            
            }
        }
        catch (Exception ex)
        {
            
            return new TransReviewResponse { respcode = 0 , respmsg = "Error!", errordetail = ex.ToString()};
        }
    
    }
     
    public TransReviewResponse submitCTR(String Username, String Password, String TransDate, String CustomerType, String FullName, String Resource, String FileName) 
    {
         try
         
         {
            if (Username != loginuser || Password != loginpass) 
            {
                return new TransReviewResponse { respcode = 7 , respmsg = "UnAuthorized Webservice User!"};
            }

            DateTime dateFiled = getServerDateGlobal(false);
            using (MySqlConnection con = dbconCTR.getConnection())
            {
                con.Open();
                MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        String sql = string.Empty;

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select * from kpglobal.ctrheader where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'PENDING'";
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (!rdr.HasRows)
                        {
                            return new TransReviewResponse { respcode = 0, respmsg = "No record found!" };
                        }

                     
                            sql = "UPDATE kpglobal.ctrheader SET status = 'VALIDATION', FileName = '"+FileName+"', Resource = @Resource, DateFiled = @DateFiled, ReviewStatus = 'SUBMIT'  where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'PENDING'";

                     
                        
                         

                        rdr.Close(); 
                        cmd.Parameters.Clear();
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        cmd.Parameters.AddWithValue("Resource", Resource);
                        cmd.Parameters.AddWithValue("DateFiled", dateFiled.ToString("yyyy-MM-dd"));
                        int xx = cmd.ExecuteNonQuery();
                        if (xx < 1)
                        {
                            trans.Rollback();
                            return new TransReviewResponse { respcode = 0, respmsg = "Error in updating CTR" };
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Insert into kpadminlogsglobal.ctrlogs (Date,TransDate,FullName,CustomerType,Action,Resource) VALUES(@Date,@TransDate,@FullName,@CustomerType,@Action,@Resource)";
                        cmd.Parameters.AddWithValue("Date", dateFiled.ToString());
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        cmd.Parameters.AddWithValue("Action", "SUBMIT");
                        cmd.Parameters.AddWithValue("Resource", Resource);
                        int yy = cmd.ExecuteNonQuery();
                        if (yy < 0)
                        {
                            trans.Rollback();
                            return new TransReviewResponse { respcode = 0, respmsg = "SQL ERROR insertion in Logs!" };
                        }

                        trans.Commit();
                        con.Close();
                        return new TransReviewResponse { respcode = 1, respmsg = "Success Reviewing CTR!" };
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new TransReviewResponse { respcode = 0, respmsg = "Error!", errordetail = ex.ToString() };
                    }
                }

            }

           
        }
        catch (Exception ex)
        {
          
              return new TransReviewResponse { respcode = 0 , respmsg = "Error!", errordetail = ex.ToString()};
        }
    
    }

    public TransReviewResponse denyCTR(String Username, String Password, String TransDate, String CustomerType, String FullName, String Resource, String Reason) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass)
            {
                return new TransReviewResponse { respcode = 7, respmsg = "UnAuthorized Webservice User!" };
            }

            DateTime dateFiled = getServerDateGlobal(false);
            using (MySqlConnection con = dbconCTR.getConnection())
            {
                con.Open(); 
                MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        String sql = string.Empty;

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select * from kpglobal.ctrheader where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'PENDING'";
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (!rdr.HasRows)
                        {
                            return new TransReviewResponse { respcode = 0, respmsg = "No record found!" };
                        }

                     
                        sql = "UPDATE kpglobal.ctrheader SET status = 'VALIDATION', Reason = '"+Reason+"',Resource = @Resource, DateFiled = @DateFiled, ReviewStatus = 'DENY'  where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'PENDING'";

       
                        rdr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        cmd.Parameters.AddWithValue("Resource", Resource);
                        cmd.Parameters.AddWithValue("DateFiled", dateFiled.ToString("yyyy-MM-dd"));
                        int xx = cmd.ExecuteNonQuery();
                        if (xx < 1)
                        {
                            trans.Rollback();
                            return new TransReviewResponse { respcode = 0, respmsg = "Error in updating CTR" };
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Insert into kpadminlogsglobal.ctrlogs (Date,TransDate,FullName,CustomerType,Action,Resource) VALUES(@Date,@TransDate,@FullName,@CustomerType,@Action,@Resource)";
                        cmd.Parameters.AddWithValue("Date", dateFiled.ToString());
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        cmd.Parameters.AddWithValue("Action", "DENY");
                        cmd.Parameters.AddWithValue("Resource", Resource);
                        int yy = cmd.ExecuteNonQuery();
                        if (yy < 0)
                        {
                            trans.Rollback();
                            return new TransReviewResponse { respcode = 0, respmsg = "SQL ERROR insertion in Logs!" };
                        }

                        trans.Commit();
                        con.Close();
                        return new TransReviewResponse { respcode = 1, respmsg = "Success Reviewing CTR!" };
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new TransReviewResponse { respcode = 0, respmsg = "Error!", errordetail = ex.ToString() };
                    }
                }

            }


        }
        catch (Exception ex)
        {

            return new TransReviewResponse { respcode = 0, respmsg = "Error!", errordetail = ex.ToString() };
        }
    }

    public TransReviewResponse validateCTR(String Username, String Password, String ValidateType, String TransDate, String CustomerType, String FullName, String ReviewedBy) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass) 
            {
                return new TransReviewResponse { respcode = 7, respmsg = "Unauthorized Webservice User!"};
            }
            using(MySqlConnection con = dbconCTR.getConnection())
            {
                DateTime dtnow = getServerDateGlobal(false);
                con.Open();
                using(MySqlCommand cmd = con.CreateCommand())
                {
                    MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {
                        String sql = string.Empty;
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select * from kpglobal.ctrheader where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS IN ('VALIDATION','QUEUED')";
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (!rdr.HasRows) 
                        {
                            return new TransReviewResponse { respcode = 0 , respmsg = "No record found!"};
                        }
                        rdr.Close();
                        
                        if(ValidateType.ToUpper() == "QUEUED")
                        {
                            sql = "UPDATE kpglobal.ctrheader SET status = 'QUEUED', ReviewedBy = @ReviewedBy, DateReviewed = @DateReviewed  where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'VALIDATION';";
                        }
                        else if(ValidateType.ToUpper() == "DENIED")
                        {
                            sql = "UPDATE kpglobal.ctrheader SET status = 'DENIED', ReviewedBy = @ReviewedBy, DateReviewed = @DateReviewed  where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'VALIDATION';";
                        }
                        else if (ValidateType.ToUpper() == "RETURN")
                        {

                            sql = "UPDATE kpglobal.ctrheader SET status = 'PENDING', ReviewedBy = NULL, DateReviewed = NULL, Resource = NULL, DateFiled = NULL  where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'VALIDATION';"; ;
                        }
                        else if (ValidateType.ToUpper() == "COMPLETED")
                        {

                            sql = "UPDATE kpglobal.ctrheader SET status = 'COMPLETED', CompletedDate = @DateReviewed, CompletedBy = @ReviewedBy  where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'QUEUED';"; ;
                        }
                        else 
                        {
                            trans.Rollback();
                            con.Close();
                            return new TransReviewResponse { respcode = 0 , respmsg = "Invalid ValidationType!"};
                        }
                        cmd.Parameters.Clear();
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("ReviewedBy",ReviewedBy);
                        cmd.Parameters.AddWithValue("DateReviewed",dtnow.ToString("yyyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        int xx = cmd.ExecuteNonQuery();
                        if (xx < 0) 
                        {
                            trans.Rollback();
                            con.Close();
                            return new TransReviewResponse { respcode = 0 , respmsg = "SQL Error!"};
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Insert into kpadminlogsglobal.ctrlogs (Date,TransDate,FullName,CustomerType,Action,Resource) VALUES(@Date,@TransDate,@FullName,@CustomerType,@Action,@Resource)";
                        cmd.Parameters.AddWithValue("Date",dtnow.ToString());
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("FullName",FullName);
                        cmd.Parameters.AddWithValue("CustomerType",CustomerType);
                        cmd.Parameters.AddWithValue("Action",ValidateType);
                        cmd.Parameters.AddWithValue("Resource",ReviewedBy);
                        int yy = cmd.ExecuteNonQuery();
                        if (yy < 0) 
                        {
                            trans.Rollback();
                            return new TransReviewResponse { respcode = 0, respmsg = "SQL ERROR insertion in Logs!"};
                        }
                        trans.Commit();
                        con.Close();
                        return new TransReviewResponse { respcode = 1 , respmsg = "SUCCESS Validating CTR!"};

                    }
                    catch (Exception ex)
                    {
                       
                        trans.Rollback();
                        con.Close();
                        return new TransReviewResponse { respcode = 0 , respmsg = "Error!", errordetail = ex.ToString() };
                    }
                }
            }
        }
        catch (Exception ex)
        {
           
            return new TransReviewResponse { respcode = 0 , respmsg = "Error", errordetail = ex.ToString()};
        }
    }

    public TransReviewResponse validDenyCTR(String Username, String Password, String TransDate, String CustomerType, String FullName, String ReviewedBy, String Remark) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass)
            {
                return new TransReviewResponse { respcode = 7, respmsg = "Unauthorized Webservice User!" };
            }
            using (MySqlConnection con = dbconCTR.getConnection())
            {
                DateTime dtnow = getServerDateGlobal(false);
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {
                        String sql = string.Empty;
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select * from kpglobal.ctrheader where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS IN ('VALIDATION','QUEUED')";
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (!rdr.HasRows)
                        {
                            return new TransReviewResponse { respcode = 0, respmsg = "No record found!" };
                        }
                        rdr.Close();

                        
                      
                            sql = "UPDATE kpglobal.ctrheader SET status = 'DENIED', ReviewedBy = @ReviewedBy, DateReviewed = @DateReviewed, Remark = @Remark, CompletedDate = @DateReviewed  where FullName = @FullName AND TransDate = @TransDate AND CustomerType = @CustomerType AND Amount > 10000 AND STATUS = 'VALIDATION';";
                        
                  
                        cmd.Parameters.Clear();
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("ReviewedBy", ReviewedBy);
                        cmd.Parameters.AddWithValue("DateReviewed", dtnow.ToString("yyyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("Remark", Remark);
                        int xx = cmd.ExecuteNonQuery();
                        if (xx < 0)
                        {
                            trans.Rollback();
                            con.Close();
                            return new TransReviewResponse { respcode = 0, respmsg = "SQL Error!" };
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Insert into kpadminlogsglobal.ctrlogs (Date,TransDate,FullName,CustomerType,Action,Resource) VALUES(@Date,@TransDate,@FullName,@CustomerType,@Action,@Resource)";
                        cmd.Parameters.AddWithValue("Date", dtnow.ToString());
                        cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(TransDate).ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("FullName", FullName);
                        cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                        cmd.Parameters.AddWithValue("Action", "DENIED");
                        cmd.Parameters.AddWithValue("Resource", ReviewedBy);
                        int yy = cmd.ExecuteNonQuery();
                        if (yy < 0)
                        {
                            trans.Rollback();
                            return new TransReviewResponse { respcode = 0, respmsg = "SQL ERROR insertion in Logs!" };
                        }
                        trans.Commit();
                        con.Close();
                        return new TransReviewResponse { respcode = 1, respmsg = "SUCCESS Validating CTR!" };

                    }
                    catch (Exception ex)
                    {

                        trans.Rollback();
                        con.Close();
                        return new TransReviewResponse { respcode = 0, respmsg = "Error!", errordetail = ex.ToString() };
                    }
                }
            }
        }
        catch (Exception ex)
        {

            return new TransReviewResponse { respcode = 0, respmsg = "Error", errordetail = ex.ToString() };
        }
    }

    public TransReviewResponse getCTRLogs(String Username, String Password, String LogType, DateTime DateFrom, DateTime DateTo) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass) 
            {
                kplog.Error("FAILED: error! : respcode = 0 , Message = UnAuthorized Webservice User!");
                return new TransReviewResponse { respcode = 7, respmsg = "UnAuthorized Webservice User!"};
            }

            DateTime dtTo = DateFrom.AddMonths(1);

            if (DateTo > dtTo) 
            {
                return new TransReviewResponse { respcode = 0, respmsg = "Date must be up to 1 month only!"};
            }

            using(MySqlConnection con = dbconCTR.getConnection())
            {
                con.Open();
                using(MySqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        List<CTRList> ctr = new List<CTRList>();
                        String sql = string.Empty;
                        if(LogType.ToUpper() == "ALL")
                        {
                            sql = "SELECT TransDate,FullName,CustomerType,Amount,Resource, DateFiled, ReviewedBy, DateReviewed, Status, CompletedDate, Reason,FileName,Remark FROM kpglobal.ctrheader Where STATUS IN ('COMPLETED','DENIED') AND CompletedDate BETWEEN @DateFrom AND @DateTo";
                        }
                        else if(LogType.ToUpper() == "COMPLETED")
                        {
                                                        
                            sql = "SELECT TransDate,FullName,CustomerType,Amount,Resource, DateFiled, ReviewedBy, DateReviewed, Status, CompletedDate, Reason,FileName,Remark FROM kpglobal.ctrheader Where STATUS = 'COMPLETED' AND CompletedDate BETWEEN @DateFrom AND @DateTo";
                        }
                        else if(LogType.ToUpper() == "DENIED")
                        {
                            sql = "SELECT TransDate,FullName,CustomerType,Amount,Resource, DateFiled, ReviewedBy, DateReviewed, Status, CompletedDate, Reason,FileName,Remark FROM kpglobal.ctrheader Where STATUS = 'DENIED' AND CompletedDate BETWEEN @DateFrom AND @DateTo";
                        
                        }
                        cmd.Parameters.Clear();
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("DateFrom",DateFrom.ToString("yyyy-MM-dd 00:00:00"));
                        cmd.Parameters.AddWithValue("DateTo",DateTo.ToString("yyyy-MM-dd 23:59:59"));
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (!rdr.HasRows) 
                        {
                            kplog.Error("FAILED: error! : respcode = 0 , Message = No record found!");
                            return new TransReviewResponse { respcode = 0, respmsg = "No record found!" };
                        }
                        while (rdr.Read()) 
                        {
                            String tempDt = string.Empty;
                            string chcker = rdr["Status"].ToString();

                            if (chcker.ToUpper() == "DENIED")
                            {
                                tempDt = rdr["DateReviewed"].ToString();
                            }
                            else    
                            {
                                tempDt = rdr["CompletedDate"].ToString();
                            }

                            ctr.Add(new CTRList 
                            {
                            
                                TransDate = Convert.ToDateTime(rdr["TransDate"]).ToString("yyyy-MM-dd"),
                                Name = rdr["FullName"].ToString(),
                                CustomerType = rdr["CustomerType"].ToString(),
                                Amount = Convert.ToDouble(rdr["Amount"]),
                                Resource = rdr["Resource"].ToString(),
                                DateFiled = rdr["DateFiled"].ToString(),
                                ReviewedBy = rdr["ReviewedBy"].ToString(),
                                DateReviewed = Convert.ToDateTime(rdr["DateReviewed"]).ToString("yyyy-MM-dd"),
                                Status = rdr["Status"].ToString(),
                                CompletedDate = Convert.ToDateTime(tempDt).ToString("yyyy-MM-dd"),
                                Reason = rdr["Reason"].ToString(),
                                FileName = rdr["FileName"].ToString(),
                                Remark = rdr["Remark"].ToString()
                                
                                
                            
                            });
                        }
                        rdr.Close();
                        kplog.Info("SUCCESS!: respcode = 1 , message = SUCCESS!");
                        return new TransReviewResponse { respcode = 1, respmsg = "Success!", data = new List<CTRList>(ctr)};
                   
                    }
                    catch (Exception ex)
                    {
                        kplog.Fatal("FAILED: Error! respcode = 0 , message = Error! , errordetail = "+ex.ToString());
                        return new TransReviewResponse { respcode = 0 , respmsg = "Error!", errordetail = ex.ToString()};
                    }
                
                }
            
            }
        }
        catch (Exception ex)
        {

            return new TransReviewResponse { respcode = 0 , respmsg = "FATAL ERROR!", errordetail = ex.ToString()};
        }
    }

    public TransReviewResponse addEmailCTR(String Username, String Password, String Email) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass) 
            {
                return new TransReviewResponse { respcode = 7 , respmsg = "UnAuthorized Webservice User!"};
            }

            using(MySqlConnection con = dbconCTR.getConnection())
            {

                con.Open();
                using(MySqlCommand cmd = con.CreateCommand())
                {

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT isActive FROM kpglobal.ctremail where Email_Add = @Email";
                    cmd.Parameters.AddWithValue("Email",Email);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if(rdr.HasRows)
                    {

                        rdr.Read();
                        if (Convert.ToDouble(rdr["isActive"]) == 1)
                        {
                            return new TransReviewResponse { respcode = 0, respmsg = "Email Already Exist!" };
                        }
                        else 
                        {
                            return new TransReviewResponse { respcode = 0 , respmsg = "Email is InActive!"};
                        }
    
                    }

                    rdr.Close();


                    cmd.Parameters.Clear();
                    cmd.CommandText = "INSERT INTO kpglobal.ctremail (Email_Add,isActive) VALUES (@Email,'1');";
                    cmd.Parameters.AddWithValue("Email",Email);
                    int xx = cmd.ExecuteNonQuery();
                    if (xx < 0) 
                    {
                        return new TransReviewResponse { respcode = 0 , respmsg = "Error inserting in ctremail!"};
                    }
                    return new TransReviewResponse { respcode = 1 , respmsg = "Success!"};


                }
            }
        }
        catch (Exception ex)
        {
           return new TransReviewResponse { respcode = 0 , respmsg = "FATAL ERROR!", errordetail = ex.ToString()};
        }
    }

    public TransReviewResponse removeEmailCTR(String Username, String Password, String Email) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass)
            {
                return new TransReviewResponse { respcode = 7, respmsg = "UnAuthorized Webservice User!" };
            }

            using (MySqlConnection con = dbconCTR.getConnection()) 
            {
                con.Open();
                using(MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "Select * from kpglobal.ctremail where Email_add = @Email";
                    cmd.Parameters.AddWithValue("Email", Email);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (!rdr.HasRows) 
                    {
                        return new TransReviewResponse { respcode = 0 , respmsg = "No Email Found!"};
                    }
                    rdr.Close();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "UPDATE kpglobal.ctremail SET isActive = '0' WHERE Email_Add = @Email;";
                    cmd.Parameters.AddWithValue("Email",Email);
                    int xx = cmd.ExecuteNonQuery();
                    if (xx < 0) 
                    { 
                        return new TransReviewResponse { respcode = 0 , respmsg = "Error in updating ctremail"};
                    }

                    return new TransReviewResponse { respcode = 1 , respmsg = "Success!"};

                
                }
            }
        }
        catch (Exception ex)
        {
            return new TransReviewResponse { respcode = 0 , respmsg = "FATAL ERROR!", errordetail = ex.ToString()};
        }
    }

    public TransReviewResponse getEmailCTR(String Username , String Password) 
    {
        try
        {
            if (Username != loginuser || Password != loginpass)
            {
                return new TransReviewResponse { respcode = 7, respmsg = "UnAuthorized Webservice User!" };
            }

            using (MySqlConnection con = dbconCTR.getConnection())
            {
                con.Open();
                List<CTRList> ctr = new List<CTRList>();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "Select Email_Add from kpglobal.ctremail where isActive = '1'";
                
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (!rdr.HasRows) 
                    {
                        return new TransReviewResponse { respcode = 0 , respmsg = "No Emails Found!"};
                    }
                    while (rdr.Read()) 
                    {
                        ctr.Add(new CTRList
                        {
                        
                            Email = rdr["Email_Add"].ToString()
                        });
                    }
                    rdr.Close();

                 

                    return new TransReviewResponse { respcode = 1, respmsg = "Success!", data = new List<CTRList>(ctr) };


                }
            }
        }
        catch (Exception ex)
        {
            return new TransReviewResponse { respcode = 0, respmsg = "FATAL ERROR!", errordetail = ex.ToString() , data = null};
        }
    }

    public FTPResponse getFTPServer(String Username, String Password) 
    {
        if (Username != loginuser || Password != loginpass)
        {
            return new FTPResponse { respcode = 7, respmsg = "UnAuthorized Webservice USER!" };
        }
        try
        {
            IniFile ini = new IniFile(path);

            String Url = ini.IniReadValue("FTPConfig CTR","Server");
            String UserFtp = ini.IniReadValue("FTPConfig CTR","UID");
            String PassFtp = ini.IniReadValue("FTPConfig CTR","Password");

            return new FTPResponse { respcode =1 , respmsg = "Success!", Server = Url,Username = UserFtp, Password = PassFtp};

        }
        catch (Exception ex)
        {
            return new FTPResponse { respcode = 0, respmsg = "ERROR!", errordetail = ex.ToString()};
        }
    
    }



    #endregion
}
 