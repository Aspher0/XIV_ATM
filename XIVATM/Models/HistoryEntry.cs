using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVATM.Models;

public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string Entry { get; set; }

    public HistoryEntry(string entry)
    {
        Timestamp = DateTime.Now;
        Entry = entry;
    }
}
