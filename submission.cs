using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ConsoleApp1
{
    public class Program
    {
        private const string xmlURL = ""; //GitHub URL for Hotels.XML.
        private const string xmlErrorURL = ""; //GitHub URL for HotelsErrors.XML.
        private const string xsdURL = ""; //GitHub URL for Hotels.XSD.

        private static string FetchUrl(string url) //This method fetches text from GitHub or a local file path.
        {
            if(HttpUrlValidater(url)) //This fetches from the former.
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (var webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    return webClient.DownloadString(url);
                }
            }
            return File.ReadAllText(url, Encoding.UTF8); //This fetches from the latter.
        }
        private static bool HttpUrlValidater(string url) //This method validates the URL.
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
        private static string MiniJsonSerializer(object objec) //This method produces a smaller JSON.
        {
            var stringBuilder = new StringBuilder();
            WriteJson(stringBuilder, objec);
            return stringBuilder.ToString();
        }
        private static void JsonWriter(StringBuilder stringBuilder, object objec) //This method writes JSON into stringBuilder.
        {
            if (objec == null)
            {
                stringBuilder.Append("null");
                return;
            }
            switch (objec)
            {
                case string stri:
                    stringBuilder.Append('"').Append(EscapeString(stri)).Append('"');
                    break;
                case bool boolean:
                    stringBuilder.Append(boolean ? "true" : "false");
                    break;
                case int _:
                case long _:
                case float _:
                case double _:
                case decimal _:
                    stringBuilder.Append(Convert.ToString(objec, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case IDictionary<string, object> dictionary:
                    sb.Append('{');
                    bool firstObject = true;
                    foreach(var keyValue in dictionary)
                    {
                        if (firstObject == false) 
                        {
                            stringBuilder.Append(',');
                        }
                        firstObject = false;
                        stringBuilder.Append('"').Append(EscapeString(keyValue.Key)).Append("\":");
                        WriteJson(stringBuilder, keyValue.Value);
                    }
                    stringBuilder.Append('}');
                    break;
                case IEnumerable<object> listObject:
                    stringBuilder.Append('[');
                    bool firstObject = true;
                    foreach (var object in listObject)
                    {
                        if(firstItem == false)
                        {
                            sb.Append(',');
                        } 
                        firstItem = false;
                        WriteJson(stringBuilder, object);
                    }
                    stringBuilder.Append(']');
                    break;
                default:
                    stringBuilder.Append('"').Append(EscapeString(objec.ToString())).Append('"');
                    break;
            }
        }
        private static string EscapeString(string stri) //This method escapes .NET string to ensure a safe JSON inclusion.
        {
            var stringBuilder = new StringBuilder(stringBuilder.Length + 8);
            foreach (var character in stri)
            {
                switch (character)
                {
                    case '\"': stringBuilder.Append("\\\""); break;
                    case '\\': stringBuilder.Append("\\\\"); break;
                    case '\b': stringBuilder.Append("\\b");  break;
                    case '\f': stringBuilder.Append("\\f");  break;
                    case '\n': stringBuilder.Append("\\n");  break;
                    case '\r': stringBuilder.Append("\\r");  break;
                    case '\t': stringBuilder.Append("\\t");  break;
                    default:
                        if (character < 0x20)
                        {
                            stringBuilder.Append("\\u");
                            stringBuilder.Append(((int)character).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(character);
                        }
                        break;
                }
            }
            return stringBuilder.ToString();
        }
        public static void Main(string[] args) //This is the main method of submission.cs.
        {
            string result = Verification(xmlURL, xsdURL); //This is the validation of Hotels.XML
            Console.WriteLine(result); //This is the printing of "No Error". Otherwise it returns an exception message.

            result = Verification(xmlErrorURL, xsdURL); //This is the validation of HotelsErrors.XML
            Console.WriteLine(result); //This is the printing of each validation problem on its own line.

            result = Xml2Json("Hotels.xml"); //This converts the Hotels.XML file to JSON and prints the file.
            Console.WriteLine(result);
        }
        public static string Verification(string xmlUrl, string xsdUrl) //This is the Verification method of submission.cs.
        {
            var validationMessages = new List<string>(); //This list stores any validation messages.
            try
            {
                string xsdText = Fetch(xsdURL); //This variable stores XSD text.
                var setOfXMLSchemas = new XmlSchemaSet(); //This is the set of one or more schemas.
                using(var xsdValidator = XmlReader.Create(new StringReader(xsdText))); //a validator is created for the XSD text and it is added to the schema set.
                {
                    setOfXMLSchemas.Add(null, xsdValidator); //The null namespace is passed for now since there is no desired namespace in XSD.
                }
                setOfXMLSchemas.Compile(); //The schema set is compiled.
                var readerSettings = XmlReaderSettings //This prepares the XML reader with settings for validation.
                {
                    ValidationType = ValidationType.Schema, //XSD validation is enabled.
                    Schemas = setOfXMLSchemas, //The schemas in setOfXMLSchemas are used.
                    DtdProcessing = DtdProcessing.Prohibit, //For safety, DTD processing is disabled.
                    IgnoreWhitespace = true, //Unimportant whitespace is ignored.
                    IgnoreComments = true //Comments are ignored.
                };
                readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings; //Report any warnings.
                readerSettings.ValidationEventHandler = readerSettings.ValidationEventHandler + (s, e) => //This captures all validation issues.
                {
                    //The following code captures the line number of the validation issues for easier debugging.
                    var exception = e.Exception;
                    string line = exception != null ? $" (line {exception.LineNumber}, pos {exception.LinePosition})" : "";
                    string severity = e.Severity == XmlSeverityType.Warning ? "Warning" : "Error";
                    errors.Add($"{severity}{line}: {e.Message}");
                };
                string xmlText = Fetch(xmlUrl); //Reading and validating the desired XML URL and then validating it.
                using(var xmlReader = XmlReader.Create(new StringReader(xmlText), readerSettings))
                {
                    while(xmlReader.Read()) //This while loop allows the entire document to be read to ensure full validation.
                    {

                    }
                }
            }
            catch (Exception exception)
            {
                errors.Add($"XML formatting error (line {exception.LineNumber}, pos {exception.LinePosition}): {exception.message}"); //This catch block catches any XML with incorrect formatting.
            }
            catch (Exception exception)
            {
                errors.Add($"Validation failed: {exception.Message}"); //Any other exception except the aforementioned is caught here.
            }
        }
        public static string Xml2Json(string xmlUrl) //This is the XML to JSON converter method of submission.cs.
        {
            string xmlText = Fetch(xmlUrl); //This stores the XML text.
            var xmlDocument = XDocument.Parse(xml, LoadOptions.None); //This stores the XML as an XDocument.
            var hotelsList = new List<object>(); //This stores each Hotel element.
            foreach(var hotel in doc.Root.Elements("Hotel")) //This for loop will iterate for all Hotel elements.
            {
                var hotelObject = new Dictionary<string, object>(); //This is the JSON object of a Hotel element.
                string hotelName = hotel.Element("Name")?.Value?.Trim() ?? ""; //This is the Hotel's name.
                var hotelPhoneNumbers = hotel.Elements("Phone").Select(p => (object)(p.Value?.Trim() ?? "")).ToList(); //This is the hotel's phone number(s), represented by an array.
                var hotelAddress = hotel.Element("Address"); //This is the hotel's address and below are its elements.
                var addressObject = new Dictionary<string, object>();
                if(hotelAddress != null)
                {
                    addressObject["Number"] = hotelAddress.Element("Number")?.Value?.Trim() ?? ""; //This is the hotel's address's number.
                    addressObject["Street"] = hotelAddress.Element("Street")?.Value?.Trim() ?? ""; //This is the hotel's address's street.
                    addressObject["City"] = hotelAddress.Element("City")?.Value?.Trim() ?? ""; //This is the hotel's address's city.
                    addressObject["State"] = hotelAddress.Element("State")?.Value?.Trim() ?? ""; //This is the hotel's address's state.
                    addressObject["Zip"] = hotelAddress.Element("Zip")?.Value?.Trim() ?? ""; //This is the hotel's address's zip code.
                    var nearestAirport = hotelAddress.Attribute("NearestAirport")?.Value; //This is the nearest airport to the hotel.
                    if(string.IsNullOrEmpty(nearestAirport) == false) //If there is a nearest airport in the variable, add it to the address array.
                    {
                        addressObj["_NearestAirport"] = nearestAirport;
                    } 
                }
                hotelObject["Name"] = hotelName; //This adds the hotel name to the JSON object.
                hotelObject["Phone"] = hotelPhoneNumbers; //This adds the hotel phone number to the JSON object.
                hotelObject["Address"] = hotelAddress; //This adds the hotel address array to the JSON object.
                var hotelRating = hotel.Attribute("Rating")?.Value; //This stores the hotel rating, if there is one.
                if(string.IsNullOrEmpty(hotelRating) == false) //This adds the rating to the JSON object.
                {
                    hotelObject["_Rating"] = hotelRating;
                }    
                hotelsList.Add(hotelObject); //Add the hotel object to the hotel list.
            }
            var hotelsRoot = new Dictionary<string, object> //This adds the Hotels root.
            {
                ["Hotels"] = new Dictionary<string, object>
                {
                    ["Hotel"] = hotelsList
                }
            };
            return ToJson(hotelsRoot); //This serializes the nested Dictionary to a JSON string.
        }
    }
}