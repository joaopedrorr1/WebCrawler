using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text;
using System.Data.Odbc;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;

namespace WebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            startCrawlerasync();
            Console.ReadLine();
        }

        public static string retornaVetorDeObjetoJSON(List<Ip> ips, int total)
        {
            string endereco = string.Empty;
            string porta = string.Empty;
            string pais = string.Empty;
            string protocolo = string.Empty;
            string strJson = string.Empty;

            JavaScriptSerializer js = new JavaScriptSerializer();

            var arrayJson = new object[total];

            foreach (var item in ips)
            {
                for (int i = 0; i < total; i++)
                {
                    endereco = ips[i].Address;
                    porta = ips[i].Port;
                    pais = ips[i].Country;
                    protocolo = ips[i].Protocol;

                    arrayJson[i] = new { jEndereco = endereco, JPorta = porta, JPais = pais, JProtocolo = protocolo };

                    strJson = js.Serialize(arrayJson);
                }
            }
            return strJson;
        }

        private static async Task startCrawlerasync()
        {
            try
            {
                //the url of the page we want to test
                var url = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(url);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                // a list to add all the list of cars and the various prices 
                var ips = new List<Ip>();
                var trs =
                htmlDocument.DocumentNode.Descendants("tr")
                    .Where(node => node.GetAttributeValue("valign", "").Equals("top")).ToList();

                foreach (var tr in trs)
                {
                    var ip = new Ip
                    {

                        //Address = tr.Descendants("a").FirstOrDefault().ChildAttributes("title").FirstOrDefault().Value,
                        //Port = tr.Descendants("span").FirstOrDefault().InnerText,
                        //Country = tr.Descendants("img").FirstOrDefault().InnerText,
                        //Protocol = tr.Descendants("td").FirstOrDefault().InnerText
                        Updated = tr.Descendants("a").FirstOrDefault().InnerText,
                        Address = tr.Descendants("a").FirstOrDefault().InnerText,
                        Port = tr.Descendants("td").FirstOrDefault().InnerText,
                        Country = tr.Descendants("td").FirstOrDefault().InnerText,
                        Speed = tr.Descendants("td").FirstOrDefault().InnerText,
                        Online = tr.Descendants("td").FirstOrDefault().InnerText,
                        Protocol = tr.Descendants("td").FirstOrDefault().InnerText,
                        Anonymity = tr.Descendants("td").FirstOrDefault().InnerText
                    };

                    ips.Add(ip);
                }
                string arquivoJson = retornaVetorDeObjetoJSON(ips, ips.Count);
                System.IO.File.WriteAllText(@"C:\IpsJson.json", arquivoJson);
                Console.WriteLine("Sucesso....");
                Console.WriteLine("Precione Enter para continuar...");
                ConsoleKeyInfo keyinfor = Console.ReadKey(true);
                if (keyinfor.Key == ConsoleKey.Enter)
                {
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
