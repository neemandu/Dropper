using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using EASendMail;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium;

namespace SupplierChecker
{
    class Program
    {
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        static void Main(string[] args)
        {
            try
            {

                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define request parameters.
                String spreadsheetId = "1mloC0hMaxQevunnoJBmrulbxLdTXHCECITqflCpZYcA";
                String range = "Product Table!F:I";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(spreadsheetId, range);

                // Prints the names and majors of students in a sample spreadsheet:
                // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
                ValueRange response = request.Execute();
                IList<IList<Object>> values = response.Values;
                List<string> asins = new List<string>();
                List<string> items = new List<string>();
                Dictionary<string, string> asinNamesMapping = new Dictionary<string, string>();
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        for (var ind = 0; ind < row.Count; ind++)
                        {
                            var item = row[ind];
                            string link = item.ToString();
                            items.Add(link);
                            if (ind > 0)
                            {
                                if (link.StartsWith("http"))
                                {
                                    Match match;
                                    Regex shipmentIdReg = new Regex(@"/B+[a-zA-Z0-9]+/ref", RegexOptions.IgnoreCase);
                                    match = shipmentIdReg.Match(link);
                                    if (match.Success)
                                    {
                                        string asin = match.Value.Substring(1, match.Value.Length - 5);
                                        asins.Add(asin);
                                        if (!asinNamesMapping.ContainsKey(asin))
                                        {
                                            asinNamesMapping.Add(asin, row[0].ToString());
                                        }
                                    }
                                    else
                                    {
                                        Regex otherReg = new Regex(@"(dp/B+[a-zA-Z0-9]{9})", RegexOptions.IgnoreCase);
                                        match = otherReg.Match(link);
                                        if (match.Success)
                                        {
                                            string asin = match.Value.Substring(3, match.Value.Length - 3);
                                            asins.Add(asin);
                                            if (!asinNamesMapping.ContainsKey(asin))
                                            {
                                                asinNamesMapping.Add(asin, row[0].ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No data found.");
                }


                List<string> productAmzonDontSell = new List<string>();
                string template = @"<!DOCTYPE html>
                                    <html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
                                    <head>
                                        <meta charset = ""utf-8""/>
 
                                         <title></title>
                                     <h2><u> The following products are not for sale by Amazon any more</u></h2>
                                    <table border=""1"">
                                     <tr style = ""background-color:blue;color:white"">
                                          <th>#</th>
                                          <th>Product Name</th>
                                          <th>ASIN</th>
                                      </tr>";
                int i = 1;
                int allCount = asins != null ? asins.Count : 0;
                int counter = 0;
                foreach (string asin in asins)
                {
                    counter++;
                    string url = $"https://www.amazon.com/gp/offer-listing/{asin}/ref=dp_olp_all_mbc?ie=UTF8&amp;condition=all";
                    bool isAmazonSelling = true;
                    try
                    {
                        isAmazonSelling = IsAmazonASeller(url);
                    }
                    catch(Exception ex)
                    {
                        try
                        {
                            isAmazonSelling = IsAmazonASeller(url);
                        }
                        catch (Exception ex2)
                        {

                        }
                    }
                    if (!isAmazonSelling)
                    {
                        string productName = asinNamesMapping.ContainsKey(asin) ? asinNamesMapping[asin] : "N/A";
                        template += $@"<tr>
                                          <td>{i}</td>
                                          <td>{productName}</td>
                                        <td><a href=""{url}"">{asin}</a></td>
                                    </tr>";
                        productAmzonDontSell.Add($@"{i}) URL: {url}
                                                    ASIN: {asin}");
                        i++;
                    }
                }
                template += $@"</table>
                                </head>
                                <body>
                                </body>
                                </html>";
                SendEmail(template);
            }
            catch (Exception ex)
            {

            }
        }

        private static void SendEmail(string productAmzonDontSell)
        {
            var fromAddress = new System.Net.Mail.MailAddress("neemandu@gmail.com", "neemandu@gmail.com");
            var toAddress = new System.Net.Mail.MailAddress("ddglobal@yahoo.com", "ddglobal@yahoo.com");
            const string fromPassword = "5052n33man@@";
            const string subject = "Products that Amazon does not sell anymore";
            string body = productAmzonDontSell;

            var smtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }



            //SmtpMail oMail = new SmtpMail("TryIt");
            //EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();

            //oMail.From = "ddglobal@yahoo.com";
            //oMail.To = "support@emailarchitect.net";
            //oMail.Subject = "Product that Amazon does not sell anymore";
            //oMail.TextBody = string.Join("\n", productAmzonDontSell);
            //SmtpServer oServer = new SmtpServer("smtp.mail.yahoo.com");
            //oServer.User = "ddglobal@yahoo.com";
            //oServer.Password = "323df4@@34234!!@#dfDDSFSedf";
            //oServer.Port = 465;
            //oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;
            //try
            //{
            //    oSmtp.SendMail(oServer, oMail);
            //}
            //catch (Exception ep)
            //{
            //    Console.WriteLine("failed to send email with the following error:");
            //    Console.WriteLine(ep.Message);
            //}
        }

        private static bool IsAmazonASeller(string url)
        {
            var driver = new EdgeDriver
            {
                Url = url
            };
            bool ans = false;
            try
            {
                if (driver.FindElementByXPath("//img[@alt='Amazon.com']").Displayed)
                    ans = true;
            }
            catch (NoSuchElementException)
            {
                    ans = false;              
            }

            driver.Quit();

            return ans;

            //string html = string.Empty;
            //HttpWebRequest supRequest = (HttpWebRequest)WebRequest.Create(url);
            //using (HttpWebResponse supResponse = (HttpWebResponse)supRequest.GetResponse())
            //using (Stream stream = supResponse.GetResponseStream())
            //using (StreamReader reader = new StreamReader(stream))
            //{
            //    html = reader.ReadToEnd();
            //    if (html.Contains("<img alt=\"Amazon.com\""))
            //        return true;
            //    else
            //    {
            //        Match match;
            //        Regex shipmentIdReg = new Regex(@"/gp/offer-listing/(?<asin>\w*)/ref=olp_page_next/(?<num>.+)?ie=UTF8&f_all=true&startIndex=10", RegexOptions.IgnoreCase);
            //        match = shipmentIdReg.Match(html);
            //        if (match.Success)
            //        {
            //            IsAmazonASeller($"https://www.amazon.com/gp/offer-listing/{match.Groups[1]}/ref=olp_page_next/{match.Groups[2]}ie=UTF8&f_all=true&startIndex=10");
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    }
            //    return false;
            //}
        }
    }
}