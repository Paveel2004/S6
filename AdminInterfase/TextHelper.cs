using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
namespace AdminInterfase
{
    internal static class TextHelper
    {
        public static string DictionaryToText(string message)
        {
            var Information = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            StringBuilder TextMessage = new StringBuilder();
            foreach (var item in Information)//оптимизировать
            {
                TextMessage.AppendLine($"{item.Key}: {item.Value}");
            }
            return TextMessage.ToString();
        }
        public static string DeserializeJsonArrayAndReturnElements(string jsonArray)
        {
            // Десериализация JSON массива в List<string>
            List<string> list = JsonConvert.DeserializeObject<List<string>>(jsonArray);

            // Возвращение элементов списка в виде строки через запятую
            return string.Join(", ", list);
        }

    }
}
