using System.Collections.Generic;

namespace Surly.Core.Structure {
    public class Relation {
        public List<ColumnAttributes> Columns { get; private set; }
        public List<Row> Rows { get; set; }

        public Relation(List<ColumnAttributes> newColumns) {
            Columns = newColumns;
            Rows = new List<Row>();
        }
    }
}
