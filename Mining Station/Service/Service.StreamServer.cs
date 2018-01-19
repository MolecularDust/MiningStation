using LiteDB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AccessPoint" in both code and config file together.
    public partial class Service : IAccessPoint, IStreamServer
    {
        public async Task<UploadResponse> UploadFileAsync(FileRequest request)
        {
            try
            {
                Stream sourceStream = request.FileStream;
                string filePath = request.FileName;
                using (var outputStream = new FileStream(filePath, System.IO.FileMode.Create))
                {
                    await sourceStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }
                return new UploadResponse { ResponseFlag = true, Error = null };
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public async Task<UploadResponse> UpdateApplication(FileRequest request)
        {
            bool restartPending = false;
            try
            {
                Stream sourceStream = request.FileStream;
                string filePath = request.FileName + ".update";

                using (var outputStream = new FileStream(filePath, System.IO.FileMode.Create))
                {
                    await sourceStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }

                string appPath = Helpers.ApplicationPath();
                File.Move(appPath, appPath + ".bak"); // rename original to .bak
                File.Move(filePath, Path.GetFileName(appPath)); // rename downloded update to .exe

                restartPending = true;
                return new UploadResponse { ResponseFlag = true, Error = null };
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                if (restartPending)
                {
                    ViewModel.SetRegistryKeyValue(Constants.BaseRegistryKey, "CallerHostName", request.CallerHostName);
                    Helpers.RestartApplication(1000, 2, true);
                }
            }
        }

        public Task<StreamDownloadResponse> DownloadStream(StreamDownloadRequest request)
        {
            MemoryStream stream = NetHelper.SerializeToStream<Workers>(ViewModel.Instance.Workers);
            return Task.FromResult(new StreamDownloadResponse { Stream = stream });
        }

        public Task<StreamDownloadResponse> GetWtmLocalDataAsync()
        {
            try
            {
                using (var db = new LiteDatabase(Constants.DataBase))
                {
                    var collection = db.GetCollection<HistoricalData>(Constants.LightDB_WtmCacheCollection);
                    var cache = collection.FindAll().Last();
                    if (cache != null)
                    {
                        MemoryStream stream = NetHelper.SerializeToStream<HistoricalData>(cache);
                        return Task.FromResult(new StreamDownloadResponse { Stream = stream });
                    }

                    return Task.FromResult(new StreamDownloadResponse { Stream = null, Error = "Cache is empty." });
                }
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public Task<StreamDownloadResponse> GetWorkersAsync()
        {
            MemoryStream stream = NetHelper.SerializeToStream<Workers>(ViewModel.Instance.Workers);
            return Task.FromResult(new StreamDownloadResponse { Stream = stream });
        }


        public async Task<StreamDownloadResponse> GetPriceHistoryAsync(StreamDownloadRequest request)
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    var vm = ViewModel.Instance;
                    await vm.UpdatePriceHistory();
                    return ViewModel.ReadHistoricalData(request.Period);
                });
                var history = await task;

                MemoryStream stream = NetHelper.SerializeToStream<List<HistoricalData>>(history);
                return new StreamDownloadResponse { Stream = stream };
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public async Task<StreamUploadResponse> UpdateWorkers(StreamUploadRequest request)
        {
            var memoryStream = new MemoryStream();
            try
            {
                await request.Stream.CopyToAsync(memoryStream);
                var workers = NetHelper.DeserializeFromStream<Workers>(memoryStream);
                ViewModel.Instance.Workers = workers.Clone();
                Workers.SaveWorkers(workers);
                var date = Workers.GetWorkersLastUpdateTime();
                return new StreamUploadResponse { ResponseFlag = true, Date = date };
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

        public async Task<StreamUploadResponse> UpdateWtmSettings(StreamUploadRequest request)
        {
            var memoryStream = new MemoryStream();
            try
            {
                await request.Stream.CopyToAsync(memoryStream);
                var wtmSettings = NetHelper.DeserializeFromStream<WtmSettingsObject>(memoryStream);
                if (request.ProxyPassword.Length != 0)
                {
                    wtmSettings.ProxyPassword = request.ProxyPassword.ToSecureString();
                    wtmSettings.ProxyPasswordEncrypted = request.ProxyPassword.Encrypt();
                    Array.Clear(request.ProxyPassword, 0, request.ProxyPassword.Length);
                }
                ViewModel.Instance.WtmSettings = wtmSettings.Clone();
                ViewModel.Instance.SaveWtmSettings();
                var date = WtmSettingsObject.GetWtmSettingsLastUpdateTime();
                return new StreamUploadResponse { ResponseFlag = true, Date = date };
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                memoryStream.Dispose();
            }
        }
    }
}
