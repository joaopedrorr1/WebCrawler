﻿using System;
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
                var ips = new List<Ip>();

                var paginacao =
                htmlDocument.DocumentNode.Descendants("ul")
                    .Where(node => node.GetAttributeValue("class", "").Equals("pagination")).ToList();

                Int64 totpagina = Convert.ToInt64(paginacao.Select(x => x.Elements("li")).Last().Last().InnerText);
            
                for(int i=1;i<= totpagina;i++)
                {
                    var urlpag = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/" + i;
                    var htmlpag = await httpClient.GetStringAsync(urlpag);
                    htmlDocument.LoadHtml(htmlpag);
                
                
                var trs =
                htmlDocument.DocumentNode.Descendants("tr")
                    .Where(node => node.GetAttributeValue("valign", "").Equals("top")).ToList();

                    foreach (var tr in trs)
                    {
                        var ip = new Ip
                        {

                            Updated = tr.Descendants("td").FirstOrDefault().InnerText,
                            Address = tr.Descendants("td").Skip(1).FirstOrDefault().InnerText,
                            Port = tr.Descendants("td").Skip(2).FirstOrDefault().InnerText,
                            Country = tr.Descendants("td").Skip(3).FirstOrDefault().InnerText,
                            Speed = tr.Descendants("td").Skip(4).FirstOrDefault().InnerText,
                            Online = tr.Descendants("td").Skip(5).FirstOrDefault().InnerText,
                            Protocol = tr.Descendants("td").Skip(6).FirstOrDefault().InnerText,
                            Anonymity = tr.Descendants("td").Skip(7).FirstOrDefault().InnerText
                        };

                        ips.Add(ip);
                    }
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
