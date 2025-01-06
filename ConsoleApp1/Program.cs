using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Net;
using System.Net.Mail;

namespace Desafio
{
    public class Program
    {
        static void Main(string[] args)
        {

            if (args == null || args.Length < 3)
            {
                Console.WriteLine("Entrada Inválida");
            }
            else
            {
                string ativo;
                float valor_venda, valor_compra;
                ativo = args[0];
                valor_venda = float.Parse(args[1]);
                valor_compra = float.Parse(args[2]);
                Console.WriteLine("{0}, {1}, {2}", ativo, valor_venda, valor_compra);

                string QUERY_URL = "https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=PBR&interval=1min&apikey=DCCKCJXEJQCOMQ3C";
                Uri queryUri = new Uri(QUERY_URL);
                using (WebClient client = new WebClient())
                {
                    dynamic json_data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(client.DownloadString(queryUri));
                    // foreach( var group in json_data )
                    // {
                    //     Console.WriteLine("Key = {0}, Value = {1}", group.Key, group.Value);
                    // }

                    foreach( var group in json_data )
                    {
                        if(group.Key == "Meta Data"){
                            Console.WriteLine("Value = {0}", group.Value);
                        } else if(group.Key == "Time Series (1min)"){
                            Console.WriteLine("Value = {0}", group.Value);
                        }
                    }
                }
                //Thread.Sleep(300000); // 5min
            }
        }

        public static void EmailSend(){
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress("lucaogorducho@gmail.com");
            foreach( var email in emailsTo)
            {
                mail.To.Add(emailDestination);
            }
            
            mail.Subject = emailAssunto;
            mail.Body = emailMessage;
            mail.IsBodyHtml = true;

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.UseDefaultCredentials  = false;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential("lucaogorducho@gmail.com", "Luc4sKitsu");
                
                try
                {
                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            
        }
    }

    // public class ExtratorMetaData
    // {
        // public string? info { get; set; }
        // public string? symbol { get; set; }
        // public string? last_refresh { get; set; }
        // public string? interval { get; set; }
        // public string? output_size { get; set; }
        // public string? time_zone { get; set; }
    // }

    // public class ExtratorTimeSeries
    // {
    //    public float info { get; set; }
    //    public float symbol { get; set; }
    //    public float last_refresh { get; set; }
    //    public float interval { get; set; }
    //    public int output_size { get; set; }
    // }
}

