using dropper.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.IO;
using EAGetMail;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Threading.Tasks;

namespace dropper.Services
{
    public class YahooService : IYahooService
    {
        public async Task<List<string>> GetShippingOrders()
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
                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];
                    Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                        info.Index, info.Size, info.UIDL);

                    // Download email from Yahoo POP3 server
                    Mail oMail = oClient.GetMail(info);
                    if (oMail.From.ToString().ToLower() == "\"amazon.com\" <shipment-tracking@amazon.com>")
                    {

                        Regex reg = new Regex(@"Order #+[A-Z0-9.-]+</a>", RegexOptions.IgnoreCase);
                        Match match;

                        List<string> results = new List<string>();
                        for (match = reg.Match(oMail.HtmlBody); match.Success; match = match.NextMatch())
                        {
                            if (!(results.Contains(match.Value)))
                            {
                                string orderId = match.Value.Substring(7, match.Value.Length - 11);
                                orders.Add(orderId);
                            }

                        }
                    }
                }
                
                oClient.Quit();

                return orders;
            }
            catch (Exception ep)
            {
                Console.WriteLine(ep.Message);
                return null;
            }
        }
    }
}