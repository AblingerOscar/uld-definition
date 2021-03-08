using System.Xml.Linq;

namespace uld.definition.Serialization
{
    public interface IXLinqSerializable
    {
        XElement SerializeToXLinq();
    }
}
