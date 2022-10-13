[XmlRoot("root", Namespace = "http://www.example.com/schema#")]
public sealed class RootElement
{
    [XmlChoiceIdentifier(nameof(ElementType))]
    [XmlElement("Type1", Namespace = "http://www.example.com/schema#")]
    [XmlElement("Type2", Namespace = "http://www.example.com/schema#")]
    [XmlElement("Type3", Namespace = "http://www.example.com/schema#")]
    public ChildElement Child { get; set; }

    [XmlIgnore]
    public ChildElementType ElementType { get; set; }
}

public sealed class ChildElement
{
}

public enum ChildElementType
{
    [XmlEnum("http://www.example.com/schema#:Type1")]
    Type1,
    [XmlEnum("http://www.example.com/schema#:Type2")]
    Type2,
    [XmlEnum("http://www.example.com/schema#:Type3")]
    Type3,
}