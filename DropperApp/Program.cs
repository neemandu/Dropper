using EAGetMail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

namespace DropperApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<string> orders = new List<string>();
            MailServer oServer = new MailServer("imap.mail.yahoo.com",
                       "ddglobal@yahoo.com", "323df4@@34234!!@#dfDDSFSedf", ServerProtocol.Imap4);
            MailClient oClient = new MailClient("TryIt");

            // Set SSL connection
            oServer.SSLConnection = true;

            // Set 995 SSL port
            oServer.Port = 993;

            try
            {
                oClient.Connect(oServer);



                oClient.GetMailInfosParam.Reset();
                oClient.GetMailInfosParam.GetMailInfosOptions |= GetMailInfosOptionType.NewOnly;
                oClient.GetMailInfosParam.GetMailInfosOptions |= GetMailInfosOptionType.DateRange;
                oClient.GetMailInfosParam.GetMailInfosOptions |= GetMailInfosOptionType.OrderByDateTime;

                oClient.GetMailInfosParam.SenderContains = "\"amazon.com\" <shipment-tracking@amazon.com>";
                oClient.GetMailInfosParam.DateRange.SINCE = System.DateTime.Now.AddMonths(-1);


                MailInfo[] infos = oClient.GetMailInfos();
                Dictionary<string, string> ordersTrackingsMapiing = new Dictionary<string, string>();
                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];
                    Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                        info.Index, info.Size, info.UIDL);

                    // Download email from Yahoo POP3 server
                    Mail oMail = oClient.GetMail(info);
                    if (oMail.From.ToString().ToLower() == "\"amazon.com\" <shipment-tracking@amazon.com>")
                    {
                        string orderId = GetOrderIdoMail(oMail.HtmlBody);
                        string trackingNumberLink = GetTrackingNumberLink(oMail.HtmlBody, orderId);
                        ordersTrackingsMapiing.Add(orderId, trackingNumberLink);
                    }
                }

                oClient.Quit();











                // If modifying these scopes, delete your previously saved credentials
                // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
                string[] Scopes = { SheetsService.Scope.Spreadsheets, "email" };
                string ApplicationName = "dudi";


                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None
                        ,new FileDataStore(credPath, true)
                       ).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define request parameters.
                String spreadsheetId = "1BWbne5R79uoaTrwV_v2eZvCoKBRH17F2J4xLlnytCBM";
                //String range = "Sheet1!A1:A4";
                //SpreadsheetsResource.ValuesResource.GetRequest request =
                //        service.Spreadsheets.Values.Get(spreadsheetId, range);

                // Prints the names and majors of students in a sample spreadsheet:
                // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
                List<List<Object>> values = new List<List<object>>();

                foreach (var order in ordersTrackingsMapiing)
                {
                    GetTrackingInfo(out string trackingNumber, out string supllier, order.Value);

                    values.Add(new List<object> { order.Key, trackingNumber, supllier });
                }


                ValueRange v = new ValueRange
                {
                    Values = values.ToArray()
                };
                SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = service.Spreadsheets.Values.Append(v, spreadsheetId, "Sheet1!A1:D1");
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                var response = appendRequest.Execute();
                //IList<IList<Object>> values = response.Values;
                //if (values != null && values.Count > 0)
                //{
                //    foreach (var row in values)
                //    {
                //        // Print columns A and E, which correspond to indices 0 and 4.
                //        Console.WriteLine(row[0]);
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("No data found.");
                //}
                //Console.Read();


            }
            catch (Exception ep)
            {
                Console.WriteLine(ep.Message);
            }

        }

        private static void GetTrackingInfo(out string trackingNumber, out string supllier, string value)
        {
            string html = string.Empty;
            string url = value;
            trackingNumber = "";
            supllier = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            Match match;
            Regex shipmentIdReg = new Regex(@"Tracking ID +[a-zA-Z0-9]+\n", RegexOptions.IgnoreCase);
            for (match = shipmentIdReg.Match(html); match.Success; match = match.NextMatch())
            {
                trackingNumber = match.Value.Substring(11, match.Value.Length - 12);
                break;
            }
            Regex refIdReg = new Regex(@"Shipped with +[A-Z0-9.-_]+\n", RegexOptions.IgnoreCase);
            for (match = refIdReg.Match(html); match.Success; match = match.NextMatch())
            {
                supllier = match.Value.Substring(12, match.Value.Length - 13);
                break;
            }
        }

        private static string GetTrackingNumberLink(string htmlBody, string orderId)
        {

            string shippmentId = "";
            string refId = "";
            Match match;
            Regex shipmentIdReg = new Regex(@"shipmentID%3D+[A-Z0-9.-_]+%", RegexOptions.IgnoreCase);
            for (match = shipmentIdReg.Match(htmlBody); match.Success; match = match.NextMatch())
            {
                shippmentId = match.Value.Substring(13, match.Value.Length - 14);
                break;
            }
            Regex refIdReg = new Regex(@"ref_%3D+[A-Z0-9.-_]+&", RegexOptions.IgnoreCase);
            for (match = refIdReg.Match(htmlBody); match.Success; match = match.NextMatch())
            {
                refId = match.Value.Substring(7, match.Value.Length - 8);
                break;
            }

            string link = $@"https://www.amazon.com/progress-tracker/package/ref={refId}?_encoding=UTF8&from=gp&itemId=&orderId={orderId}&packageIndex=0&shipmentId={shippmentId}";
            return link;
        }

        private static string GetOrderIdoMail(string htmlBody)
        {
            Regex orderIdReg = new Regex(@"orderId%3D+[A-Z0-9.-]+%", RegexOptions.IgnoreCase);
            Match match;
            string orderId = "";
            List<string> results = new List<string>();
            for (match = orderIdReg.Match(htmlBody); match.Success; match = match.NextMatch())
            {
                orderId = match.Value.Substring(10, match.Value.Length - 11);
            }
            return orderId;
        }
    }
}
