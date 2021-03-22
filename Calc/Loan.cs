using CommandLine;
using System;
using System.IO;

namespace Calc
{
    public enum LoanData
    {
        LOANCODE,
        STANDARTCONTRIBUTION,
        LOANSTARTDATE,
        LOANENDDATE,
        LATESTCONTRIBUTIONPAIDVALUE,
        DATEOFLASTPAYMENT
    }
    public class Loan
    {
        private string _loanCode { get; set; }
        private decimal _standartContribution { get; set; }
        private DateTime _loanStartDate { get; set; }
        private DateTime _loanEndDate { get; set; }
        private decimal _latestContributionsPaidValue { get; set; }
        private DateTime _dateOfLastPayment { get; set; }
        private readonly IDiscountCounter _discountCounter;
        public Loan(string[] clientLoansData,DateTime inspectionDate)
        {
            for (int i = 0; i < clientLoansData.Length - 1; i++)
            {
                switch (i)
                {
                    case (int)LoanData.LOANCODE:
                        _loanCode = clientLoansData[i];
                        break;
                    case (int)LoanData.STANDARTCONTRIBUTION:
                        _standartContribution = decimal.Parse(clientLoansData[i]);
                        break;
                    case (int)LoanData.LOANSTARTDATE:
                        _loanStartDate = DateTime.Parse(clientLoansData[i]);
                        break;
                    case (int)LoanData.LOANENDDATE:
                        _loanEndDate = DateTime.Parse(clientLoansData[i]);
                        break;
                    case (int)LoanData.LATESTCONTRIBUTIONPAIDVALUE:
                        _latestContributionsPaidValue = decimal.Parse(clientLoansData[i]);
                        break;
                    case (int)LoanData.DATEOFLASTPAYMENT:
                        _dateOfLastPayment = DateTime.Parse(clientLoansData[i]);
                        break;
                }
            }
            _discountCounter = new DiscountCounter(_loanStartDate,_standartContribution,_latestContributionsPaidValue,_loanCode,inspectionDate);
        }
        public DiscountForCode GetNextMonthPayment()
        {
            return _discountCounter.ReturnPaymentWithBiggestDiscount();
        }
    }
    public class DiscountForCode
    {
        private readonly string _loanCode;
        private readonly decimal _contributionWithDiscount;
        public DiscountForCode(string loanCode,decimal contributionWithDiscount)
        {
            _loanCode = loanCode;
            _contributionWithDiscount = contributionWithDiscount;
        }
        public string LoanCode()
        {
            return _loanCode;
        }
        public decimal StandartContributionWithDiscount()
        {
            return _contributionWithDiscount;
        }
    }
    public class DiscountCounter : IDiscountCounter
    {
        private readonly DateTime _loanStartDate;
        private readonly decimal _standartContribution;
        private readonly decimal _latestContributionsPaidValue;
        private readonly string _loanCode;
        private readonly DateTime _inspectionDate;
        public DiscountCounter(DateTime loanStartDate, decimal standartContribution, decimal latestContributionsPaidValue,string loanCode,DateTime inspectionDate)
        {
            _loanStartDate = loanStartDate;
            _latestContributionsPaidValue = latestContributionsPaidValue;
            _standartContribution = standartContribution;
            _loanCode = loanCode;
            _inspectionDate = inspectionDate;
        }
        private bool OnePercentDiscount()
        {
            var i = Math.Abs(((_loanStartDate.Year - _inspectionDate.Year) * 12) + _loanStartDate.Month - _inspectionDate.Month);
            if (i % 10 == 0)
                return true;
            return false;
        }
        private bool EvenNumberDiscont()
        {
            var i = Math.Abs(((_loanStartDate.Year - _inspectionDate.Year) * 12) + _loanStartDate.Month - _inspectionDate.Month);
            if (_standartContribution % 2 == 0 && i % 18 == 0)
                return true;
            return false;
        }
        private bool DoublePremiumDiscount()
        {
            if ((_latestContributionsPaidValue > (_standartContribution * 2)) && (_standartContribution > 100))
                return true;
            return false;
        }
        public DiscountForCode ReturnPaymentWithBiggestDiscount()
        {
            decimal minPayment = _standartContribution; ;
            decimal contribution = _standartContribution;
            if (OnePercentDiscount())
            {
                contribution = _standartContribution - (_standartContribution * 1 / 100);
                if (Decimal.Compare(contribution, minPayment) == -1)
                {
                    minPayment = contribution;
                }
            }
            if (DoublePremiumDiscount())
                contribution = _standartContribution - (_standartContribution % 100);
                if (Decimal.Compare(contribution, minPayment) == -1)
                {
                    minPayment = contribution;
                }
            if (EvenNumberDiscont())
                minPayment = 0;
            return new DiscountForCode(_loanCode, minPayment);
        }
    }
    public interface IDiscountCounter
    {
        DiscountForCode ReturnPaymentWithBiggestDiscount();
    }
    public class Arguments
    {
        [Option('r', "read", Required = true, HelpText = "Input file to be processed.")]
        public string InputFrom { get; set; }
        [Option('w', "write", Required = false, HelpText = "output file to be processed.")]
        public string InputTo { get; set; }
        [Option('t', "date", Required = false, HelpText = "inspection date (Y,M,D)")]
        public string InspectionDate { get; set; }
       public string FileName
        {
            get
            {
                var uri = new Uri(InputFrom);
                string filename = Path.GetFileName(uri.LocalPath);
                var index = filename.IndexOf(".");
                return filename.Remove(index); ;
            }
        }
    }
}
