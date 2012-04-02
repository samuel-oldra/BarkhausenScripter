using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scripter
{
    internal class Program
    {
        #region Private Fields

        private static readonly List<double> ItensRms = new List<double>();

        #endregion Private Fields



        #region Private Methods

        private static double CalcularMediaRms(List<double> lista)
        {
            double total = lista.Sum(item => Math.Pow(item, 2));
            return Math.Sqrt(total / lista.Count);
        }

        private static void GerarArquivoPico(string filePath, int margem = 2000)
        {
            int count = 1;
            int maiorIndice = 0;
            double maiorValor = 0;

            using (var writer = new StreamWriter(string.Concat(filePath, "p"), false))
            {
                string linha;
                StreamReader reader = null;
                using (reader = new StreamReader(filePath))
                {
                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(linha)) continue;
                        string[] colunas = linha.Split('\t');

                        if (colunas.Length < 3) continue;
                        double sec = Convert.ToDouble(colunas[2].Replace('.', ','));

                        if (sec > maiorValor)
                        {
                            maiorValor = sec;
                            maiorIndice = count;
                        }
                        count++;
                    }
                }

                count = 1;
                int stopIndice = maiorIndice + margem;
                int startIndice = maiorIndice - margem;

                using (reader = new StreamReader(filePath))
                {
                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(linha)) continue;
                        string[] colunas = linha.Split('\t');

                        if (colunas.Length < 3) continue;
                        double pri = Convert.ToDouble(colunas[1].Replace('.', ','));
                        double sec = Convert.ToDouble(colunas[2].Replace('.', ','));

                        if (count > startIndice && count <= stopIndice)
                            GravarArquivo(writer, pri, sec);
                        count++;
                    }
                }
            }
        }

        private static void GerarArquivoRms(string filePath, int passo = 10, int janela = 100)
        {
            ItensRms.Clear();

            using (var writer = new StreamWriter(filePath.Replace(".lvm", ".rms"), false))
            {
                using (var reader = new StreamReader(filePath))
                {
                    string linha;
                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(linha)) continue;
                        string[] colunas = linha.Split('\t');

                        if (colunas.Length < 3) continue;
                        double pri = Convert.ToDouble(colunas[1].Replace('.', ','));
                        double sec = Convert.ToDouble(colunas[2].Replace('.', ','));

                        GravarArquivoRms(writer, pri, sec, passo, janela);
                    }
                }
            }
        }

        private static void GerarArquivoRmsn(string filePath)
        {
            int count = 1;
            double total = 0;
            double media = 0;

            using (var writer = new StreamWriter(filePath.Replace(".rms", ".rmsn"), false))
            {
                string linha;
                StreamReader reader = null;
                using (reader = new StreamReader(filePath))
                {
                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(linha)) continue;
                        string[] colunas = linha.Split('\t');

                        if (colunas.Length < 3) continue;
                        double sec = Convert.ToDouble(colunas[2].Replace('.', ','));

                        if (count > 5000)
                        {
                            media = total / 5000;
                            break;
                        }
                        total += sec;
                        count++;
                    }
                }

                using (reader = new StreamReader(filePath))
                {
                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(linha)) continue;
                        string[] colunas = linha.Split('\t');

                        if (colunas.Length < 3) continue;
                        double pri = Convert.ToDouble(colunas[1].Replace('.', ','));
                        double sec = Convert.ToDouble(colunas[2].Replace('.', ','));

                        GravarArquivo(writer, pri - media, sec - media);
                    }
                }
            }
        }

        private static void GerarArquivoRmsnm(string[] args)
        {
            int count = 0;
            int files = 0;
            var itensRmsnmPri = new double[500000];
            var itensRmsnmSec = new double[500000];

            foreach (string arg in args)
            {
                if (!new FileInfo(arg).Exists) continue;
                files++;

                count = 0;
                using (var reader = new StreamReader(arg))
                {
                    string linha;
                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(linha)) continue;
                        string[] colunas = linha.Split('\t');

                        if (colunas.Length < 3) continue;
                        double pri = Convert.ToDouble(colunas[1].Replace('.', ','));
                        double sec = Convert.ToDouble(colunas[2].Replace('.', ','));

                        itensRmsnmPri[count] += pri;
                        itensRmsnmSec[count] += sec;

                        count++;
                    }
                }
            }

            if (files > 0)
                using (var writer = new StreamWriter(args[1].Replace(".rmsn", ".rmsnm"), false))
                    for (int i = 0; i < count; i++)
                        GravarArquivo(writer, itensRmsnmPri[i] / files, itensRmsnmSec[i] / files);
        }

        private static void GravarArquivo(StreamWriter writer, double pri, double sec)
        {
            writer.WriteLine(string.Format("\t{0:F6}\t{1:F6}", pri, sec).Replace(',', '.'));
        }

        private static void GravarArquivoRms(StreamWriter writer, double pri, double sec, int passo, int janela)
        {
            ItensRms.Add(sec);

            if (ItensRms.Count < janela) return;
            GravarArquivo(writer, pri, CalcularMediaRms(ItensRms));
            ItensRms.RemoveRange(0, passo);
        }

        private static void Main(string[] args)
        {
            if (DateTime.Now > new DateTime(2013, 06, 01)) return;

            string script = string.Empty;
            if (args.Length >= 1)
                script = args[0];

            switch (script)
            {
                case "/RMS":
                    if (args.Length == 2)
                        GerarArquivoRms(args[1]);
                    else if (args.Length == 3)
                        GerarArquivoRms(args[2], Convert.ToInt32(args[1]));
                    else if (args.Length == 4)
                        GerarArquivoRms(args[3], Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
                    break;

                case "/RMSN":
                    if (args.Length == 2)
                        GerarArquivoRmsn(args[1]);
                    break;

                case "/RMSNM":
                    if (args.Length >= 2)
                        GerarArquivoRmsnm(args);
                    break;

                case "/PICO":
                    if (args.Length == 2)
                        GerarArquivoPico(args[1]);
                    if (args.Length == 3)
                        GerarArquivoPico(args[2], Convert.ToInt32(args[1]));
                    break;

                default:
                    MostrarHelp();
                    //Console.ReadKey();
                    break;
            }
        }

        private static void MostrarHelp()
        {
            Console.WriteLine();
            Console.WriteLine("  SCRIPTS:");
            Console.WriteLine();
            Console.WriteLine("  /RMS - Gerar arquivo de RMS");
            Console.WriteLine("  Scripter.exe /RMS [nome_arquivo_lvm]");
            Console.WriteLine("  Scripter.exe /RMS [passo] [nome_arquivo_lvm]");
            Console.WriteLine("  Scripter.exe /RMS [passo] [janela] [nome_arquivo_lvm]");
            Console.WriteLine("  Ex.: Scripter.exe /RMS teste.lvm");
            Console.WriteLine("  Ex.: Scripter.exe /RMS 10 teste.lvm");
            Console.WriteLine("  Ex.: Scripter.exe /RMS 10 100 teste.lvm");
            Console.WriteLine();
            Console.WriteLine("  /RMSN - Gerar arquivo de RMSN (Normalizado)");
            Console.WriteLine("  Scripter.exe /RMSN [nome_arquivo_rms]");
            Console.WriteLine("  Ex.: Scripter.exe /RMSN teste.rms");
            Console.WriteLine();
            Console.WriteLine("  /RMSNM - Gerar arquivo de RMSNM");
            Console.WriteLine("  Scripter.exe /RMSNM [nome_arq_rmsn] [nome_arq_rmsn] ... [nome_arq_rmsn]");
            Console.WriteLine("  Ex.: Scripter.exe /RMSNM teste1.rmsn teste2.rmsn ... testen.rmsn");
            Console.WriteLine();
            Console.WriteLine("  /PICO - Gerar arquivo de PICO");
            Console.WriteLine("  Scripter.exe /PICO [nome_arq_lvm|nome_arq_rms|nome_arq_rmsn]");
            Console.WriteLine("  Scripter.exe /PICO [margem] [nome_arq_lvm|nome_arq_rms|nome_arq_rmsn]");
            Console.WriteLine("  Ex.: Scripter.exe /PICO teste.rms");
            Console.WriteLine("  Ex.: Scripter.exe /PICO 2000 teste.rms");
            Console.WriteLine();
        }

        #endregion Private Methods
    }
}