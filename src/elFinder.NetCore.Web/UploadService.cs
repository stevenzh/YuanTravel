using elFinder.NetCore.Web.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.ServiceModel;

namespace elFinder.NetCore.Web
{
    public class UploadService : IUploadService
    {

        private readonly ILogger<UploadService> _logger = null;

        public UploadService(ILogger<UploadService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        public UploadFileResponse UploadFile(UploadFileRequest inputModel)
        {
            UploadFileResponse result = new UploadFileResponse { RetCode = 0 };

            try
            {
                string root = Path.Combine("Files", inputModel.VirtualPath);
                string folder = Startup.MapPath("~/" + root);
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    _logger.LogError("文件路径创建出错:" + ex.ToString());
                }
                string targetPath = Path.Combine(folder, inputModel.FileName);

                if (File.Exists(targetPath)) File.Delete(targetPath);
                using (FileStream fs = File.Open(targetPath, FileMode.Append))
                {
                    _logger.LogInformation("已经上传文件长度" + fs.Length);
                    SaveFileByByte(inputModel.FileStream, fs);
                    fs.Close();

                    result.RetCode = 1;
                    result.FilePath = root.Replace(Path.DirectorySeparatorChar, '/') + "/";
                    result.FileName = inputModel.FileName;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("上传" + inputModel.FileName + "出错：" + e.ToString());
                result.RetCode = 0;
                throw;
            }
            finally
            {
            }
            return result;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public UploadFileResponse UploadPicture(UploadFileRequest request)
        {
            UploadFileResponse result = new UploadFileResponse { RetCode = 0 };
            try
            {
                string root = Path.Combine("Pictures", request.VirtualPath);
                string folder = Startup.MapPath(root);
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    _logger.LogError("文件路径创建出错:" + ex.ToString());
                }
                string targetPath = Path.Combine(folder, request.FileName);

                if (File.Exists(targetPath)) File.Delete(targetPath);
                using (FileStream fs = File.Open(targetPath, FileMode.Append))
                {
                    _logger.LogInformation("已经上传文件长度" + fs.Length);
                    SaveFileByByte(request.FileStream, fs);
                    fs.Close();

                    result.RetCode = 1;
                    result.FilePath = root.Replace(Path.DirectorySeparatorChar, '/') + "/";
                    result.FileName = request.FileName;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("上传" + request.FileName + "出错：" + e.ToString());
                result.RetCode = 0;
                throw;
            }
            finally
            {
            }
            return result;
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public UploadFileResponse CreateFolder(UploadFileRequest request)
        {
            UploadFileResponse result = new UploadFileResponse { RetCode = 0 };
            try
            {
                string root = Path.Combine("Files", request.VirtualPath);
                string folder = Startup.MapPath("~/" + root);
                try
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    result.RetCode = 1;
                    result.FilePath = root.Replace(Path.DirectorySeparatorChar, '/') + "/";
                }
                catch (Exception ex)
                {
                    _logger.LogError("文件路径创建出错:" + ex.ToString());
                }
            }
            catch (Exception e)
            {
                _logger.LogError("上传" + request.FileName + "出错：" + e.ToString());
                result.RetCode = 0;
                throw;
            }
            finally
            {
            }
            return result;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public long GetFileSize(string sourceFile)
        {
            try
            {
                string targetPath = Startup.MapPath(sourceFile);
                if (File.Exists(targetPath))
                    using (var fs = new FileStream(targetPath, FileMode.Open, FileAccess.Read))
                    {
                        return fs.Length;
                    }
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void DelelteFileByFileName(string fileName)
        {
            try
            {
                string targetPath = Startup.MapPath(fileName);

                if (File.Exists(targetPath))
                    File.Delete(targetPath);
            }
            catch (Exception ex)
            {
                _logger.LogError("删除文件" + fileName + "出错：" + ex.ToString());
            }
        }


        private void SaveFileByByte(byte[] FileByte, FileStream fs)
        {
            fs.Write(FileByte, 0, FileByte.Length);
        }
    }

    [ServiceContract]
    public interface IUploadService
    {
        [OperationContract]
        UploadFileResponse UploadFile(UploadFileRequest inputModel);
        [OperationContract]
        public UploadFileResponse UploadPicture(UploadFileRequest request);
        [OperationContract]
        public UploadFileResponse CreateFolder(UploadFileRequest request);
        [OperationContract]
        public long GetFileSize(string sourceFile);
        [OperationContract]
        public void DelelteFileByFileName(string fileName);
    }
}