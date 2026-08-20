using Arch.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Lvy.Web.Common
{

    public struct UploadType
    {
        public const string Image = "images";
        public const string Video = "videos";
        public const string Document = "documents";
        public const string Documentpdf = "pdf";
    }

    /// <summary>
    /// FTP 传输文件 （弃用）
    /// </summary>
    public sealed class FtpClient
    {
        #region Internal Members

        private NetworkCredential _certificate;

        public readonly string _ftpRoot = AppSetting.Get("ResServerVPath");
        private readonly string _userName = AppSetting.Get("UserName");
        private readonly string _password = AppSetting.Get("PassWord");
        private readonly string _uploadType;
        public string FtpUri
        {
            get { return _ftpRoot + @"/upload/" + _uploadType; }

        }

        #endregion

        public string ConvertFptPathByHttp(string path)
        {
            return path.Replace("http://", AppSetting.Get("ResServerIp"));
        }

        /// <summary>
        /// 构造函数，提供初始化数据的功能，打开Ftp站点
        /// </summary>
        /// <param name="uploadType">上传类型</param>
        public FtpClient(string uploadType)
        {
            _uploadType = uploadType;
            _certificate = new NetworkCredential(_userName, _password);
        }
        /// <summary>
        /// 构造函数，提供初始化数据的功能，打开Ftp站点
        /// </summary>
        /// <param name="uploadType"></param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public FtpClient(string uploadType, string userName, string password)
        {
            _uploadType = uploadType;
            _userName = userName;
            _password = password;
            _certificate = new NetworkCredential(_userName, _password);
        }

        /// <summary>
        /// 创建FTP请求
        /// </summary>
        /// <param name="uri">ftp://myserver/upload.txt</param>
        /// <param name="method">Upload/Download</param>
        /// <returns></returns>
        private FtpWebRequest CreateFtpWebRequest(Uri uri, string method)
        {
            FtpWebRequest ftpClientRequest = (FtpWebRequest)WebRequest.Create(uri);

            ftpClientRequest.Proxy = null;
            ftpClientRequest.Credentials = _certificate;
            ftpClientRequest.KeepAlive = true;
            ftpClientRequest.UseBinary = true;
            ftpClientRequest.UsePassive = true;
            ftpClientRequest.Method = method;

            //ftpClientRequest.Timeout = -1;

            return ftpClientRequest;
        }

        public bool UploadFile(Stream streamFile, string modPath, string fileName)
        {
            return UploadFile(streamFile, modPath, fileName, WebRequestMethods.Ftp.UploadFile);
        }
        public bool UploadFile(Stream streamFile, string modPath, string fileName, string ftpMethod)
        {
            try
            {

                Uri destinationPath = new Uri(AppSetting.Get("vacationPictureFtpUrl") + " / upload/images" + modPath + "/" + fileName);

                FtpWebRequest request = null;

                MakeDir(AppSetting.Get("vacationPictureFtpUrl") + @" / upload/" + _uploadType, modPath);


                request = CreateFtpWebRequest(destinationPath, ftpMethod);

                Stream requestStream = request.GetRequestStream();//需要获取文件的流

                //CreateFolders(destinationPath.AbsolutePath);

                CopyDataToDestination(streamFile, requestStream, 0);
                WebResponse response = request.GetResponse();

                response.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }

            return true;
        }

        public bool UploadImage(Stream imageFile, string fileName)
        {
            try
            {
                const string ftpMethod = WebRequestMethods.Ftp.UploadFile;
                Uri destinationPath = new Uri(AppSetting.Get("vacationPictureFtpUrl") + " / upload/images" + "/" + fileName);

                FtpWebRequest request = null;

                MakeDir(AppSetting.Get("vacationPictureFtpUrl") + @" / upload/" + _uploadType, "");


                request = CreateFtpWebRequest(destinationPath, ftpMethod);

                Stream requestStream = request.GetRequestStream();//需要获取文件的流

                //CreateFolders(destinationPath.AbsolutePath);

                CopyDataToDestination(imageFile, requestStream, 0);
                WebResponse response = request.GetResponse();

                response.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }

            return true;
        }

        public bool UploadImage1(Stream imageFile, string fileName)
        {
            try
            {
                const string ftpMethod = WebRequestMethods.Ftp.UploadFile;
                Uri destinationPath = new Uri(AppSetting.Get("vacationPictureFtpUrl") + "/upload/images/huodong" + "/" + fileName);

                FtpWebRequest request = null;

                MakeDir(AppSetting.Get("vacationPictureFtpUrl") + @" / upload/" + _uploadType, "");


                request = CreateFtpWebRequest(destinationPath, ftpMethod);

                Stream requestStream = request.GetRequestStream();//需要获取文件的流

                //CreateFolders(destinationPath.AbsolutePath);

                CopyDataToDestination(imageFile, requestStream, 0);
                WebResponse response = request.GetResponse();

                response.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }

            return true;
        }
        public bool UploadDocument(Stream streamFile, string fileName)
        {
            try
            {
                const string ftpMethod = WebRequestMethods.Ftp.UploadFile;
                Uri destinationPath = new Uri(AppSetting.Get("vacationPictureFtpUrl") + "/upload/documents" + "/" + fileName);

                FtpWebRequest request = null;

                MakeDir(AppSetting.Get("vacationPictureFtpUrl") + @"/upload/" + _uploadType, "");


                request = CreateFtpWebRequest(destinationPath, ftpMethod);

                Stream requestStream = request.GetRequestStream();//需要获取文件的流

                //CreateFolders(destinationPath.AbsolutePath);

                CopyDataToDestination(streamFile, requestStream, 0);
                WebResponse response = request.GetResponse();

                response.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }

            return true;
        }
        public bool UploadTravel(Stream streamFile, string modPath, string fileName)
        {
            try
            {
                const string ftpMethod = WebRequestMethods.Ftp.UploadFile;
                Uri destinationPath = new Uri(AppSetting.Get("vacationPictureFtpUrl") + " / upload/documents" + modPath + "/" + fileName);

                FtpWebRequest request = null;

                MakeDir(AppSetting.Get("vacationPictureFtpUrl") + @" / upload/" + _uploadType, modPath);


                request = CreateFtpWebRequest(destinationPath, ftpMethod);

                Stream requestStream = request.GetRequestStream();//需要获取文件的流

                //CreateFolders(destinationPath.AbsolutePath);

                CopyDataToDestination(streamFile, requestStream, 0);
                WebResponse response = request.GetResponse();

                response.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }

            return true;
        }
        /// <summary>
        /// 前台网站上传pdf 电子合同，报名意向书 wanglg 2011-10-20
        /// </summary>
        /// <param name="streamFile"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool UploadDocumentPdf(Stream streamFile, string fileName)
        {
            try
            {
                const string ftpMethod = WebRequestMethods.Ftp.UploadFile;
                Uri destinationPath = new Uri(AppSetting.Get("vacationPictureFtpUrl") + " / upload/" + _uploadType + "" + "/" + fileName);

                FtpWebRequest request = null;

                MakeDir(AppSetting.Get("vacationPictureFtpUrl") + @" / upload/" + _uploadType, "");


                request = CreateFtpWebRequest(destinationPath, ftpMethod);

                Stream requestStream = request.GetRequestStream();//需要获取文件的流

                //CreateFolders(destinationPath.AbsolutePath);

                CopyDataToDestination(streamFile, requestStream, 0);
                WebResponse response = request.GetResponse();

                response.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }

            return true;
        }

        private int CopyDataToDestination(Stream sourceStream, Stream destinationStream, int offSet)
        {
            try
            {
                int sourceLength = (int)sourceStream.Length;
                int length = sourceLength - offSet;
                byte[] buffer = new byte[length + offSet];
                sourceStream.Position = 0;
                int bytesRead = sourceStream.Read(buffer, offSet, length);
                while (bytesRead != 0)
                {
                    destinationStream.Write(buffer, 0, bytesRead);
                    bytesRead = sourceStream.Read(buffer, 0, length);
                    length = length - bytesRead;
                    offSet = (bytesRead == 0) ? 0 : (sourceLength - length);//(length - bytesRead);
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return offSet;
            }
            finally
            {
                destinationStream.Close();
                sourceStream.Close();
            }
            return offSet;
        }


        public void MakeDir(string perPath, string modePath)
        {
            if (modePath.IsNullOrEmpty())
                return;
            string[] temps = null;
            if (modePath.Substring(0, 1) == "/")
                temps = modePath.Substring(1).Split('/');
            else
                temps = modePath.Split('/');

            string dirName = perPath + "/" + temps[0];

            if (!DirectoryExist(perPath, temps[0]))
            {

                FtpWebRequest request = null;

                request = (FtpWebRequest)FtpWebRequest.Create(dirName);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;

                request.UseBinary = true;
                request.Credentials = _certificate;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }

            modePath = "";
            for (int i = 1; i < temps.Length; i++)
            {
                if (i == temps.Length - 1)
                    modePath += temps[i];
                else
                    modePath += temps[i] + "/";
            }

            if (modePath.IsNullOrEmpty() || modePath.Equals("/"))
                return;
            MakeDir(dirName, modePath);
        }
        /// <summary>
        ///  删除文件
        /// </summary>
        /// <param name="fullPath">全路径含文件名</param>
        public void RemoveFile(string fullPath)
        {
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(fullPath));
            reqFTP.Credentials = _certificate;
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            string result = String.Empty;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            long size = response.ContentLength;
            Stream datastream = response.GetResponseStream();
            StreamReader sr = new StreamReader(datastream);
            result = sr.ReadToEnd();
            sr.Close();
            datastream.Close();
            response.Close();
        }

        /// <summary>  
        /// 判断当前目录下指定的子目录是否存在  
        /// </summary>  
        /// <param name="remoteDirectoryName">指定的目录名</param>  
        public bool DirectoryExist(string remoteDirectoryName, string folder)
        {
            string[] dirList = GetAllList(remoteDirectoryName);
            foreach (string str in dirList)
            {
                if (str.Trim() == folder.Trim())
                {
                    return true;
                }
            }
            return false;
        }
        public bool DirectoryExist(string folder)
        {
            return GetAllList(folder).Length > 0;
        }
        /// <summary>  
        /// 获取FTP文件列表包括文件夹  
        /// </summary>  
        /// <returns></returns>  
        private string[] GetAllList(string url)
        {
            List<string> list = new List<string>();
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(new Uri(url));
            req.Credentials = _certificate;
            req.Method = WebRequestMethods.Ftp.ListDirectory;
            req.UseBinary = true;
            req.UsePassive = true;
            try
            {
                using (FtpWebResponse res = (FtpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {

                        string s;
                        while ((s = sr.ReadLine()) != null)
                        {
                            list.Add(s);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new string[0];
            }
            return list.ToArray();
        }

        /// <summary>
        /// 通用上传，前端指定文件路径，区别于（UploadFile方法 images路径）
        /// </summary>
        /// <param name="streamFile"></param>
        /// <param name="modPath"></param>
        /// <param name="fileName"></param>
        /// <param name="ftpMethod"></param>
        /// <returns></returns>
        public bool CommonUploadFile(Stream streamFile, string modPath, string fileName)
        {
            try
            {
                Uri destinationPath = new Uri(AppSetting.Get("vacationPictureFtpUrl") + " / " + modPath + "/" + fileName);
                FtpWebRequest request = null;
                MakeDir(AppSetting.Get("vacationPictureFtpUrl") + @" / ", modPath);
                request = CreateFtpWebRequest(destinationPath, WebRequestMethods.Ftp.UploadFile);
                Stream requestStream = request.GetRequestStream();//需要获取文件的流
                CopyDataToDestination(streamFile, requestStream, 0);
                WebResponse response = request.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
            return true;
        }

    }



}