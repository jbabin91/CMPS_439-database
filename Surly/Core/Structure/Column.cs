namespace Surly.Core.Structure {
    public class ColumnAttributes {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Length { get; set; }
        public bool Nullable { get; set; }

        public ColumnAttributes(string name, string type, int length, bool nullable) {
            this.Name = name;
            this.Type = type;
            this.Length = length;
            this.Nullable = nullable;
        }
    }
}
