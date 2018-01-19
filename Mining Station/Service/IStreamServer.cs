using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFileServer" in both code and config file together.
    [ServiceContract]
    public interface IStreamServer
    {
        [OperationContract]
        Task<UploadResponse> UploadFileAsync(FileRequest request);
        [OperationContract]
        Task<UploadResponse> UpdateApplication(FileRequest request);
        [OperationContract]
        Task<StreamUploadResponse> UpdateWorkers(StreamUploadRequest request);
        [OperationContract]
        Task<StreamUploadResponse> UpdateWtmSettings(StreamUploadRequest request);
        [OperationContract]
        Task<StreamDownloadResponse> DownloadStream(StreamDownloadRequest request);
        [OperationContract]
        Task<StreamDownloadResponse> GetWorkersAsync();
        [OperationContract]
        Task<StreamDownloadResponse> GetWtmLocalDataAsync();
        [OperationContract]
        Task<StreamDownloadResponse> GetPriceHistoryAsync(StreamDownloadRequest request);
    }

    [MessageContract]
    public class FileRequest : IDisposable
    {
        [MessageHeader(MustUnderstand = true)]
        public string CallerHostName;

        [MessageHeader(MustUnderstand = true)]
        public string FileName;

        [MessageHeader(MustUnderstand = true)]
        public long Length;

        [MessageBodyMember(Order = 1)]
        public System.IO.Stream FileStream;

        public void Dispose()
        {
            if (FileStream != null)
            {
                FileStream.Close();
                FileStream = null;
            }
        }
    }

    [MessageContract]
    public class UploadResponse
    {
        [MessageHeader(MustUnderstand = true)]
        public bool ResponseFlag;

        [MessageHeader(MustUnderstand = true)]
        public string Error;
    }

    [MessageContract]
    public class StreamUploadRequest
    {
        [MessageHeader(MustUnderstand = true)]
        public char[] ProxyPassword;

        [MessageBodyMember(Order = 1)]
        public System.IO.Stream Stream;
    }

    [MessageContract]
    public class StreamUploadResponse
    {
        [MessageHeader(MustUnderstand = true)]
        public bool ResponseFlag;

        [MessageHeader(MustUnderstand = true)]
        public DateTime Date;

        [MessageHeader(MustUnderstand = true)]
        public string Error;
    }

    [MessageContract]
    public class StreamDownloadRequest
    {
        [MessageHeader(MustUnderstand = true)]
        public int Period;
    }

    [MessageContract]
    public class StreamDownloadResponse
    {
        [MessageHeader(MustUnderstand = true)]
        public string Error;

        [MessageBodyMember(Order = 1)]
        public System.IO.Stream Stream;
    }
}
