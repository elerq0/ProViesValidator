using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace ViesValidator
{
    public class Application
    {
        private Boolean isCompleted;
        private List<String> euCountries = new List<String>();

        public Int32 State { get; set; }
        public String Comment { get; set; }
        public String MemberState { get; set; }
        public String VatNumber { get; set; }
        public String Date { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }
        public String ConsulationNumber { get; set; }

        public Application()
        {
            euCountries.Add("AT");
            euCountries.Add("BE");
            euCountries.Add("BG");
            euCountries.Add("CY");
            euCountries.Add("CZ");
            euCountries.Add("DE");
            euCountries.Add("DK");
            euCountries.Add("EE");
            euCountries.Add("EL");
            euCountries.Add("ES");
            euCountries.Add("EU");
            euCountries.Add("FI");
            euCountries.Add("FR");
            euCountries.Add("GB");
            euCountries.Add("HR");
            euCountries.Add("HU");
            euCountries.Add("IE");
            euCountries.Add("IT");
            euCountries.Add("LT");
            euCountries.Add("LU");
            euCountries.Add("LV");
            euCountries.Add("MT");
            euCountries.Add("NL");
            euCountries.Add("PT");
            euCountries.Add("RO");
            euCountries.Add("SE");
            euCountries.Add("SI");
            euCountries.Add("SK");
            euCountries.Add("PL");


            State = 0;
            Comment = String.Empty;
            MemberState = String.Empty;
            VatNumber = String.Empty;
            Date = String.Empty;
            Name = String.Empty;
            Address = String.Empty;
            ConsulationNumber = String.Empty;
    }

        public void Run(String arg1, String arg2, String arg3, String arg4, int timeoutLimit)
        {
            try
            {
                isCompleted = false;
                ViesValid(arg1, arg2, arg3, arg4);

                int counter = 0;
                while (!isCompleted)
                {
                    Thread.Sleep(100);
                    counter += 1;
                    if (counter >= timeoutLimit)
                        throw new Exception("Timeout");
                }
            }
            catch(Exception exc)
            {
                State = -1;
                Comment = exc.Message;
            }
        }
       
        private async Task<String> ViesValidReq(String memberStateCode, String memberNumber, String reqMemberStateCode, String reqMemberNumber)
        {
            try
            {
                String url = "http://ec.europa.eu/taxation_customs/vies/vatResponse.html";
                String formData = "?memberStateCode=" + memberStateCode +
                                    "&number=" + memberNumber +
                                    "&traderName=" +
                                    "&traderStreet=" +
                                    "&traderPostalCode=" +
                                    "&traderCity=" +
                                    "&requesterMemberStateCode=" + reqMemberStateCode +
                                    "&requesterNumber=" + reqMemberNumber +
                                    "&action=check";

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response;
                String responseString;
                
                response = await httpClient.PostAsync(url + formData, null);
                return responseString = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                throw new Exception("Nieudana próba połączenia z serwisem");
            }

        }

        private async Task ViesValidInterprete(String memberStateCode, String memberNumber, String reqMemberStateCode, String reqMemberNumber)
        {
            String responseString = await ViesValidReq(memberStateCode, memberNumber, reqMemberStateCode, reqMemberNumber);
            String result;

            try
            {
                result = GetResult(responseString);
                MemberState = GetParameters(responseString, "Member State");
                VatNumber = GetParameters(responseString, "VAT Number");
                Date = GetParameters(responseString, "Date when request received");
                Name = GetParameters(responseString, "Name");
                Address = GetParameters(responseString, "Address");
                ConsulationNumber = GetParameters(responseString, "Consultation Number");

                Regex rx = new Regex("([a-zA-Z-0-9 ]*), ([a-zA-Z-0-9 ]*)");
                Match match = rx.Matches(result)[0];

                if (match.Groups[1].Value.ToLower() == "yes")
                {
                    State = 1;
                    Comment = "";
                }
                else
                {
                    State = 0;
                    Comment = match.Groups[2].Value;
                }

                isCompleted = true;
            }
            catch (Exception)
            {
                throw new Exception("Serwis niedostępny lub zwrócił niepoprawne dane!");
            }
        }

        private async void ViesValid(String memberStateCode, String memberNumber, String reqMemberStateCode, String reqMemberNumber)
        {
            try
            {
                if (memberStateCode == "PL")
                    throw new Exception("Procedura niedozwolona dla firm zarejestrowanych na terenie RP!");

                if (!euCountries.Contains(memberStateCode))
                    throw new Exception("Niedozwolony kod państwa: " + memberStateCode);

                await ViesValidInterprete(memberStateCode, memberNumber, reqMemberStateCode, reqMemberNumber);
               
            }
            catch(Exception exc)
            {
                isCompleted = true;
                State = -1;
                Comment = exc.Message;
            }
        }

        private String GetParameters(string text, string parameter)
        {
            Regex rx = new Regex("<td class=\"labelStyle\">" + parameter + "</td>(?: |\\r|\\n|\\t)*<td>(.*)\n*</td>");
            MatchCollection matches = rx.Matches(text);
            return matches[0].Groups[1].Value.Replace("<br />", " ");
        }

        private String GetResult(string text)
        {
            Regex rx = new Regex("<span class=\"(?:in)*validStyle\">(.*)</span>");
            MatchCollection matches = rx.Matches(text);
            return matches[0].Groups[1].Value;
        }

    }
}
