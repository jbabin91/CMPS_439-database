using System;
using System.Collections.Generic;
using Surly.Core.Structure;

namespace WebGUI.Models {
    public class Tables {
        public IEnumerable<Tuple<string, Relation>> MyTables { get; set; }
    }
}