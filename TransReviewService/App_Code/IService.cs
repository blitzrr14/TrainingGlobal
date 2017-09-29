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
// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.
[ServiceContract]
public interface IService
{
    //TRANSACTIONAL REVIEW
    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getTransReviewByName/?Username={Username}&Password={Password}&FirstName={FirstName}&MiddleName={MiddleName}&LastName={LastName}&DateFrom={DateFrom}&DateTo={DateTo}&AmountMin={AmountMin}&AmountMax={AmountMax}&CustType={CustType}&WOCancel={WOCancel}")]
    SeccomResponse getTransReviewByName(String Username, String Password, String FirstName, String MiddleName, String LastName, String DateFrom, String DateTo, String AmountMin, String AmountMax, String CustType, Boolean WOCancel);

 

    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getTransReviewByPhoneNo/?Username={Username}&Password={Password}&PhoneNo={PhoneNo}&DateTo={DateTo}&DateFrom={DateFrom}&AmountMin={AmountMin}&AmountMax={AmountMax}&WOCancel={WOCancel}")]
    SeccomResponse getTransReviewByPhoneNo(String Username, String Password, String PhoneNo, String DateTo, String DateFrom, String AmountMin, String AmountMax, Boolean WOCancel);

   

    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getTransReviewByTotalAmount/?Username={Username}&Password={Password}&DateTo={DateTo}&DateFrom={DateFrom}&AmountMin={AmountMin}&AmountMax={AmountMax}&WOCancel={WOCancel}")]
    SeccomResponse getTransReviewByTotalAmount(String Username, String Password, String DateTo, String DateFrom, String AmountMin, String AmountMax, Boolean WOCancel);


    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getTransReviewBySenderAddress/?Username={Username}&Password={Password}&SenderStreet={SenderStreet}&ZipCode={ZipCode}&City={City}&State={State}&DateFrom={DateFrom}&DateTo={DateTo}&AmountMin={AmountMin}&AmountMax={AmountMax}&WOCancel={WOCancel}")]
    SeccomResponse getTransReviewBySenderAddress(String Username, String Password, String SenderStreet, String ZipCode, String City, String State, String DateFrom, String DateTo, String AmountMin, String AmountMax, Boolean WOCancel);


    //CURRENCY TRANSACTIONAL REPORT

    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getCTR/?Username={Username}&Password={Password}&Display={Display}")]
    TransReviewResponse getCTR(String Username, String Password, String Display);

    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getCtrDetails/?Username={Username}&Password={Password}&TransDate={TransDate}&CustomerType={CustomerType}&Name={Name}")]
    TransReviewResponse getCtrDetails(String Username, String Password, DateTime TransDate, String CustomerType, String Name);


    //[OperationContract]
    //[WebInvoke(Method = "POST",
    //    RequestFormat = WebMessageFormat.Json,
    //    ResponseFormat = WebMessageFormat.Json,
    //    BodyStyle = WebMessageBodyStyle.WrappedRequest,
    //    UriTemplate = "/reviewCTR/")]
    //TransReviewResponse reviewCTR(String Username, String Password, String ReviewType, String TransDate, String CustomerType, String FullName, String Resource, String FileName, String ReviewReason);

    [OperationContract]
    [WebInvoke(Method = "POST",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/submitCTR/")]
    TransReviewResponse submitCTR(String Username, String Password, String TransDate, String CustomerType, String FullName, String Resource, String FileName);

    [OperationContract]
    [WebInvoke(Method = "POST",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/denyCTR/")]
    TransReviewResponse denyCTR(String Username, String Password, String TransDate, String CustomerType, String FullName, String Resource, String Reason);

    [OperationContract]
    [WebInvoke(Method = "POST",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/validateCTR/")]
    TransReviewResponse validateCTR(String Username, String Password, String ValidateType, String TransDate, String CustomerType, String FullName, String ReviewedBy);

    [OperationContract]
    [WebInvoke(Method = "POST",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/validDenyCTR/")]
    TransReviewResponse validDenyCTR(String Username, String Password, String TransDate, String CustomerType, String FullName, String ReviewedBy, String Remark);
   
    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getCTRLogs/?Username={Username}&Password={Password}&LogType={LogType}&DateFrom={DateFrom}&DateTo={DateTo}")]
    TransReviewResponse getCTRLogs(String Username, String Password, String LogType, DateTime DateFrom, DateTime DateTo);

    [OperationContract]
    [WebInvoke(Method = "POST",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/addEmailCTR/")]
    TransReviewResponse addEmailCTR(String Username, String Password, String Email);


    [OperationContract]
    [WebInvoke(Method = "POST",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/removeEmailCTR/")]
    TransReviewResponse removeEmailCTR(String Username, String Password, String Email);


    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getEmailCTR/?Username={Username}&Password={Password}")]
    TransReviewResponse getEmailCTR(String Username, String Password);


    [OperationContract]
    [WebInvoke(Method = "GET",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.WrappedRequest,
        UriTemplate = "/getFTPServer/?Username={Username}&Password={Password}")]
    FTPResponse getFTPServer(String Username, String Password);
}

