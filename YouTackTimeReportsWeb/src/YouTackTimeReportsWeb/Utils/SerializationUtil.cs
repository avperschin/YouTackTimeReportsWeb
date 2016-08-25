using System.Xml.Linq;
using System.Xml.Serialization;

namespace YouTackTimeReportsWeb.Utils
{
    public static class SerializationUtil
    {
        public static T Deserialize<T>(this XDocument source)
        {
            if (source?.Root == null)
                return default(T);

            using (var reader = source.Root.CreateReader())
                return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
        }

        public static XDocument Serialize<T>(T value)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            XDocument doc = new XDocument();
            using (var writer = doc.CreateWriter())
            {
                xmlSerializer.Serialize(writer, value);
            }

            return doc;
        }
    }
}
