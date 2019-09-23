using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            inicarCrawler(); //chamada do método que inicia o processo
            Console.ReadLine();
        }

        public static string retornaVetorDeObjetoJSON(List<Ip> ips)
        {
            var arrayJson = JsonConvert.SerializeObject(ips);
            return arrayJson;
        }

        private static async Task inicarCrawler()
        {
            try
            {
                var url = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(url);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                var ips = new List<Ip>();//criando lista de Ips

                var paginacao =
                htmlDocument.DocumentNode.Descendants("ul")
                    .Where(node => node.GetAttributeValue("class", "").Equals("pagination")).ToList();

                Int64 totpagina = Convert.ToInt64(paginacao.Select(x => x.Elements("li")).Last().Last().InnerText);//total de pag da paginação
            
                for(int i=1;i<= totpagina;i++)
                {
                    var urlpag = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/" + i;
                    var htmlpag = await httpClient.GetStringAsync(urlpag);
                    htmlDocument.LoadHtml(htmlpag);
                
                
                var trs =
                htmlDocument.DocumentNode.Descendants("tr")
                    .Where(node => node.GetAttributeValue("valign", "").Equals("top")).ToList();

                    foreach (var tr in trs)//capturar os dados do site
                    {
                        var ip = new Ip
                        {
                            Address = tr.Descendants("td").Skip(1).FirstOrDefault().InnerText,
                            Port = tr.Descendants("td").Skip(2).FirstOrDefault().InnerText,
                            Country = tr.Descendants("td").Skip(3).FirstOrDefault().InnerText,
                            Protocol = tr.Descendants("td").Skip(6).FirstOrDefault().InnerText,
                        };

                        ips.Add(ip);//adcionando ips a lista de Ips
                    }
                }
                string arquivoJson = retornaVetorDeObjetoJSON(ips);//método que recebe um array de objetos Ips e devolve no formato JSON
                System.IO.File.WriteAllText(@"C:\IpsJson.json", arquivoJson); //persistência do retorno do metodo retornaVetorDeObjetoJSON em arquivo .JSON
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
