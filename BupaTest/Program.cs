using System;
using System.Collections.Generic;
using System.IO;

using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RestSharp;


namespace BupaTest
{
    
    class Program
    {
        static void Main(string[] args)
        {

            
            //get registration input
            var input = getInput();
            //check Registration is valid input
            if (RegCheck(input) == false)
            {
                Console.WriteLine("Invalid Vichele Registration");
            }
            else
            {
                // contiue code
                GetVicheleInfoAsync(input);
                //get request fails, using json from text file as supstituted for output
                //this just gets the txt file with a response json taken from api documentation 
                var currentpath = System.IO.Directory.GetCurrentDirectory();
                var FilePath = currentpath + "/OutputExample.txt";
                
                var jsonText = File.ReadAllText(FilePath);
                // sends json to my output formatting function
                ConsoleOutput(jsonText);
            }

            Console.ReadLine();
        }
        public static string getInput() 
        {
            Console.WriteLine("Please Enter Registration");
            String input = Console.ReadLine();
            return (input);
        }

        public static Boolean RegCheck(String RegNumber)
        {
            string pattern = @"[A-Z0-9]+$";  //simple Regex to check it only contains numbers and Upper case letters, not formatting for lowercase atm
            Regex regex = new Regex(pattern);
            
            if (regex.IsMatch(RegNumber)) //regex check
            {
                return(true);
            }
            else
            {
                return (false);
            }
        }

        public static void GetVicheleInfoAsync(String Reg)
        {
            
            try
            {
                //set url
                var client = new RestClient("https://beta.check-mot.service.gov.uk");
                //send post request
                var request = new RestRequest("/trade/vehicles/mot-tests?registration="+Reg,Method.POST);

                
                //set headers --- I get 403 authentication error when connecting, issue is eithr URL or api key
                request.AddHeader("x-api-key", "fZi8YcjrZN1cGkQeZP7Uaa4rTxua8HovaswPuIno");
                request.AddHeader("Accept", "application/json+v6");
                //get response 
                var response = client.Post(request);
                var content = response.Content;

                Console.WriteLine(content);
                Console.WriteLine(response.StatusCode);

                //if working the content can be passed onto the output formatting
                //
                //ConsoleOutput(content);
            }
            catch (Exception error)
            {
                    //error handling
                Console.WriteLine(error);
            }
        }

        public static void ConsoleOutput(string jsonText)
        {

            
            ExampleJson outputjson = JsonConvert.DeserializeObject<ExampleJson>(jsonText);
            // converts input json to object using package Newtonsoft.Json
            Console.WriteLine("Make: "+ outputjson.make);
            Console.WriteLine("Model: " + outputjson.model);
            Console.WriteLine("Colour: " + outputjson.primaryColour);

            DateTime dateTop = DateTime.Parse("01/01/1550"); // used old date to for intial compare, no values should be older than this on a car MOT
            foreach(var x in outputjson.motTests)
            {
                //just cycles through to find the most up to data time
                var datetmp = DateTime.Parse(x.completedDate);
                if (DateTime.Compare(dateTop, datetmp) == -1)
                {
                    dateTop = datetmp;
                }
            }

            Console.WriteLine("Last MOT date: " + dateTop);

            foreach (var x in outputjson.motTests)
            {
                   // uses the most up to date time to select the mileage from that specfic MOT
                var datetmp = DateTime.Parse(x.completedDate);
                if(datetmp == dateTop)
                {
                    Console.WriteLine("Mileage at last MOT: " + x.odometerValue);
                }
            }


        }
        
    }
    //json object formatting to make output easier to manipulate 
    
    public class RfrAndComment
    {
        public string text { get; set; }
        public string type { get; set; }
        public bool dangerous { get; set; }
    }

    public class MotTest
    {
        public string completedDate { get; set; }
        public string testResult { get; set; }
        public string expiryDate { get; set; }
        public string odometerValue { get; set; }
        public string odometerUnit { get; set; }
        public string odometerResultType { get; set; }
        public string motTestNumber { get; set; }
        public List<RfrAndComment> rfrAndComments { get; set; }
    }

    public class ExampleJson
    {
        public string registration { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string firstUsedDate { get; set; }
        public string fuelType { get; set; }
        public string primaryColour { get; set; }
        public string vehicleId { get; set; }
        public string registrationDate { get; set; }
        public string manufactureDate { get; set; }
        public string engineSize { get; set; }
        public List<MotTest> motTests { get; set; }
    }


}
