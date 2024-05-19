using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass
{
    public class ProcessInfo
    {
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        public double MemoryUsageMB { get; set; }

        public ProcessInfo(string processName, string windowTitle, double memoryUsageMB)
        {
            ProcessName = processName;
            WindowTitle = windowTitle;
            MemoryUsageMB = memoryUsageMB;
        }

        public override string ToString()
        {
            return $"Процесс: {ProcessName}\nНазвание окна: {WindowTitle}\nОперативная память: {MemoryUsageMB} МB\n";
        }
    }

}
