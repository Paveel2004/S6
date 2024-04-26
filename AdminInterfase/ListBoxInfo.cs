using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminInterfase
{
    class ListBoxInfo
    {
        public string Text { get; set; }
        public ObservableCollection<string> Buttons { get; set; }
    }
}
