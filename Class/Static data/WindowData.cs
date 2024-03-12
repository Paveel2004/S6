using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Static_data;

public class WindowData
{
    public string Title { get; set; }
    public DateTime DateTime { get; set; }
    public WindowData(string Title) 
    {
        this.Title = Title;
        this.DateTime = DateTime.Now;
    }
}
