using System;
using System.Net;
using System.Text.Json;
using SendEmail;
using System.Text.Json.Serialization;
using System.Threading;

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
                double valor_venda, valor_compra;
                ativo = args[0];
                valor_venda = Convert.ToDouble(args[1]);
                valor_compra = Convert.ToDouble(args[2]);
                Console.WriteLine("{0}, {1}, {2}", ativo, valor_venda, valor_compra);

                string QUERY_URL = "https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol="+ativo+"&interval=1min&apikey=DCCKCJXEJQCOMQ3C";//Pesquisa o Ativo
                Uri queryUri = new Uri(QUERY_URL);
                
                //leitura do arquivo de email e configuracoes smpt
                const string filePath = "\\emailConfigs.txt";
                using var file = new StreamReader(filePath);
                string? emailDestiny;
                string? Provedor;
                string? Username;
                string? Password;
                if((emailDestiny = file.ReadLine()) == null | (Provedor = file.ReadLine()) == null | (Username = file.ReadLine()) == null | (Password = file.ReadLine()) == null)
                    return;     

                file.Close();

                while(true){//verifica continuinamente a cada 1 minuto
                    QueryAlert(queryUri, valor_venda, valor_compra, emailDestiny, Provedor, Username, Password);
                    // Thread.Sleep(60000); // 1min
                    
                }
            }
        }

        //funcao que analisa as novas requisicoes
        static void QueryAlert(Uri queryUri, double valor_venda, double valor_compra, string? emailDestiny, string? Provedor, string? Username, string? Password)
        {
            using (WebClient client = new WebClient())
            {
                dynamic? json_data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(client.DownloadString(queryUri));

                string lastDataUpdate = "";
                foreach(var group in json_data)
                {
                    if(group.Key == "Meta Data"){//analisa o primeiro dado do dicionario que indica o ultimo update
                        //Console.WriteLine("Value = {0}", group.Value);
                        dynamic dataFromMetaData = DeserializerDynamicFunction(group.Value);
                        Console.WriteLine("Last Refreshed = {0}", dataFromMetaData["3. Last Refreshed"]);
                        lastDataUpdate = Convert.ToString(dataFromMetaData["3. Last Refreshed"]);
                    } else if(group.Key == "Time Series (1min)"){//verifica o ultimo update e dispara o alerta de email caso satisfaça a condicao
                        //Console.WriteLine("Value = {0}", group.Value);
                        dynamic dataFromTimeSeries = DeserializerDynamicFunction(group.Value);
                        foreach (var datagroup in dataFromTimeSeries)
                        {
                            if(datagroup.Key == lastDataUpdate)
                            {
                                dynamic dataFromTime = DeserializerDynamicFunction(datagroup.Value);
                                string valueClose = Convert.ToString(dataFromTime["4. close"]);
                                Console.WriteLine("Close = {0}", valueClose);
                                string messageToSend;
                                if(Convert.ToDouble(valueClose) > valor_venda)//sugere a venda
                                {
                                    messageToSend = "Venda recomendada, preço sugerido:"+valor_venda+", preço atual:"+valueClose;
                                    AlertEmail(messageToSend, emailDestiny, Provedor, Username, Password);
                                }
                                else if(Convert.ToDouble(valueClose) < valor_compra)//sugere a compra
                                {
                                    messageToSend = "Venda recomendada, preço sugerido:"+valor_compra+", preço atual:"+valueClose;
                                    AlertEmail(messageToSend, emailDestiny, Provedor, Username, Password);
                                }
                            }
                        }
                    }
                }
            }
        }

        static dynamic DeserializerDynamicFunction(dynamic dataDeserialize)//funcao que serializa e deserializa dados do dicionario(Foi necessario devido a maneira que os dados chegavam no json)
        {
            dynamic data = JsonSerializer.Serialize(dataDeserialize);
            dynamic newData = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(data);
            return newData;
        }

        static void AlertEmail(string messageToSend, string? emailDestiny, string? Provedor, string? Username, string? Password)//dispara o email
        {
            var sendingEmail = new Email(Provedor, Username, Password);
            sendingEmail.SendEmail(
            emailTo: emailDestiny,
            emailSubject: "Alerta de Ativo",
            emailMessage: messageToSend
            );
        }
    }
}