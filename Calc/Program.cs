using CommandLine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Calc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Arguments arguments = new Arguments();
            Parser.Default.ParseArguments<Arguments>(args).WithParsed<Arguments>(a => 
            {
                arguments.InputFrom = a.InputFrom;
                if (a.InputTo == null)
                {
                    var index = a.InputFrom.LastIndexOf("\\");
                    arguments.InputTo = a.InputFrom.Remove(index) + $"\\{a.FileName}_resultatai.csv";
                }
                else
                {
                    arguments.InputTo = a.InputTo + $"\\{a.FileName}_resultatai.csv";
                }
                if (a.InspectionDate == null)
                {
                    arguments.InspectionDate = "2021,1,2";
                }
                else
                {
                    arguments.InspectionDate = a.InspectionDate;
                }
            });
            try
            {
                await ReadAndWriteContributions(arguments.InputFrom, arguments.InputTo, DateTime.Parse(arguments.InspectionDate));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occured: " + ex.Message);
            }
        }
        private static async Task ReadAndWriteContributions(string from, string to,DateTime inspectionDate)
        {
            using FileStream inputStream = new FileStream(from, FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(inputStream, Encoding.UTF8);
            using FileStream outputStream = new FileStream(to, FileMode.OpenOrCreate);
            using var sw = new StreamWriter(outputStream, Encoding.UTF8);
            string line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                var client = GetCodeWithDiscount(line,inspectionDate);
                await sw.WriteAsync(client);
                outputStream.Flush();
            }
        }
        private static string GetCodeWithDiscount(string line,DateTime inspectionDate)
        {
            Loan loan = new Loan(line.Split(';'), inspectionDate);
            var codeAndDiscount = loan.GetNextMonthPayment();
            string discount = codeAndDiscount.StandartContributionWithDiscount().ToString();
            if (codeAndDiscount.StandartContributionWithDiscount() == 0)
            {
                discount = "-";
            }
            return $"{codeAndDiscount.LoanCode()};{discount};\n";
        }
    }
}
