using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_collection
{
    public static class JsonHelper
    {
        public static string ConvertDictionaryToJson(List<Dictionary<string, string>> data) => JsonConvert.SerializeObject(data);
        public static List<Dictionary<string,string>> DeserializeJsonToListOfDictionaries(string jsonString) => JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonString);
        public static string ConvertListToJson<T>(List<T> list) => JsonConvert.SerializeObject(list, Formatting.Indented);
        public static List<T> DeserializeJsonToList<T>(string jsonString) => JsonConvert.DeserializeObject<List<T>>(jsonString);



    }


}
