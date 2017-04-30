using System;

namespace Surly.Core.Structure {
    public class Row {
        public Object[] Cells { get; set; }

        public Row(int size) {
            Cells = new Object[size];
        }
    }
}
