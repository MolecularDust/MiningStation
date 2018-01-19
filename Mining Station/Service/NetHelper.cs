using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mining_Station
{
    public static class NetHelper
    {
        public static int Port { get; set; }

        public static MemoryStream SerializeToStream<T>(T obj)
        {
            MemoryStream stream = new MemoryStream();
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            XmlDictionaryWriter binaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
            serializer.WriteObject(binaryWriter, obj);
            binaryWriter.Flush();
            stream.Position = 0;
            return stream;
        }

        public static T DeserializeFromStream<T>(MemoryStream stream)
        {
            stream.Position = 0;
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            XmlDictionaryReader binaryReader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
            T obj = (T)serializer.ReadObject(binaryReader);
            return obj;
        }

        public static string CloseChannel(object channel)
        {
            var client = channel as IClientChannel;
            if (client == null)
                throw new InvalidCastException("Failed to interpret argument as IClientChannel.");

            try
            {
                client.Close();
                return null;
            }
            catch (CommunicationException e)
            {
                client.Abort();
                return e.Message;
            }
            catch (TimeoutException e)
            {
                client.Abort();
                return e.Message;
            }
            catch (Exception e)
            {
                client.Abort();
                return e.Message;
            }
        }
    }
}
