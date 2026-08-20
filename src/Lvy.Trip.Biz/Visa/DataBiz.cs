using Arch.Common.Utils;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz;
using Lvy.Visa.Dao;
using Lvy.Visa.Models;
using Lvy.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.Visa.Biz
{
    /// <summary>
    /// 签证材料
    /// </summary>
    public class DataBiz : BaseBiz
    {
        private VisaDataDao _dao = new VisaDataDao();
        private VisaCategoryDao _categoryDao = new VisaCategoryDao();
        private VisaDataFileDao _fileDao = new VisaDataFileDao();
        private VisaProductDao _productDao = new VisaProductDao();
        private HistoryDao _historyDao = new HistoryDao();

        public IList<VisaDataModel> GetVisaDataList(VisaDataModel model)
        {
            var list = _dao.Fetch("select * from Visa_Data where CategoryCode=@0 and InformationCode=@1 ", model.CategoryCode, model.InformationCode);
            foreach (var item in list)
            {
                item.DataFilesList = _fileDao.Fetch("select  * from Visa_DataFiles where InformationCode=@0 and DataCode=@1", item.InformationCode, item.DataCode);
            }
            return list;
        }

        public int SearchVisaDatasCount(VisaDataModel model)
        {
            return _dao.ExecuteScalar<Int32>("select count(*) from Visa_Data where CategoryCode=@1 and InformationCode=@0", model.InformationCode, model.CategoryCode);
        }

        public int GetCountVisaDataSTemp(string informationCode)
        {
            return _dao.ExecuteScalar<Int32>("select count(*) from Visa_Data where IsTemplate=1 and InformationCode=@0", informationCode);
        }

        public VisaDataModel GetVisaDataByCode(string dataCode)
        {
            var item = _dao.FirstOrDefault("select * from Visa_Data where DataCode=@0 ", dataCode);
            item.DataFilesList = _fileDao.Fetch("select  * from Visa_DataFiles where InformationCode=@0 and DataCode=@1", item.InformationCode, item.DataCode);
            return item;
        }

        public void SaveVisaDataSingle(VisaDataModel model, CrmAccountModel currentUser, string ip)
        {
            StringBuilder sbStr = new StringBuilder("【" + GetCategoryByCode(model.CategoryCode).CategoryName + "】");
            if (!string.IsNullOrEmpty(model.DataCode))
            {
                var entity = _dao.GetByCode(model.InformationCode, model.DataCode);
                entity.DataName = model.DataName;
                entity.DataExplain = model.DataExplain;
                entity.IsNeed = model.IsNeed;
                entity.IsTemplate = model.IsTemplate;
                entity.IsOriginal = model.IsOriginal;
                entity.IsBack = model.IsBack;
                entity.DataCount = model.DataCount;
                entity.ModifyBy = currentUser.Code;
                entity.Modifytime = DateTime.Now;
                _dao.Update(entity);
                sbStr.Append("修改了｛" + model.DataName + "｝的材料数据");
            }
            else
            {
                int count = _dao.GetCountByCategoryCode(model.CategoryCode, model.InformationCode);
                model.Number = Convert.ToInt32(count + 1);
                model.DataCode = "V" + DBTools.GetSeqNo("Visa_Data");
                model.CreateBy = currentUser.Code;
                model.Createtime = DateTime.Now;

                _dao.Insert(model);
                sbStr.Append("新增一条｛" + model.DataName + "｝的材料数据");
            }

            //材料附件
            if (model.DataFilesList != null)
            {
                foreach (var file in model.DataFilesList)
                {
                    if (!string.IsNullOrEmpty(file.FilesCode))
                    {
                        //修改
                        file.InformationCode = model.InformationCode;
                        _fileDao.UpdateVisaDataFile(file);
                    }
                    else
                    {
                        //新增
                        file.InformationCode = model.InformationCode;
                        file.DataCode = model.DataCode;
                        if (string.IsNullOrEmpty(file.FileUrl))
                        {
                            file.FileUrl = "";
                        }
                        file.FilesCode = "V" + DBTools.GetSeqNo("Visa_Data");
                        file.CreateBy = model.ModifyBy;
                        file.Createtime = DateTime.Now;

                        _fileDao.Insert(file);
                    }
                }
            }
            AddYlOperateHistory(model.InformationCode, sbStr.ToString(), currentUser, ip);
        }

        public IList<VisaDataModel> GetVisaDatasTemplate(string informationCode)
        {
            var list = _dao.Fetch("select * from Visa_Data where IsTemplate=1 and InformationCode=@0 ", informationCode);
            foreach (var item in list)
            {
                item.DataFilesList = _fileDao.Fetch("select  * from Visa_DataFiles where InformationCode=@0 and DataCode=@1", item.InformationCode, item.DataCode);
            }
            return list;
        }

        /// <summary>
        /// 根据材料编码字符串保存材料数据 并返回添加好的材料列表
        /// </summary>
        /// <param name="VisaDataStr">拼接材料字符串</param>
        /// <returns></returns>
        public void AddVisaDatasByCodeStr(string visaDatasStr, string CategoryCode, CrmAccountModel currentUser, string ip)
        {
            string InformationCode = "";
            StringBuilder sbStr = new StringBuilder();
            string CategoryName = GetCategoryByCode(CategoryCode).CategoryName;
            sbStr.Append("【" + CategoryName + "】添加引用了模版");

            var filemodel = new VisaDataFileModel();
            var result = GetVisaDataByCodeStr(visaDatasStr);
            int i = 1;
            string dataName = "";
            int count = 0;
            foreach (var data in result)
            {
                if (i != result.Count())
                {
                    dataName += data.DataName + ",";
                }
                else
                {
                    InformationCode = data.InformationCode;
                    dataName += data.DataName;
                }
                filemodel.DataCode = data.DataCode;
                filemodel.InformationCode = data.InformationCode;
                data.DataFilesList = _fileDao.GetDataFilesList(filemodel);//原模板的附件列表

                count = _dao.GetCountByCategoryCode(CategoryCode, data.InformationCode);

                var entity = new VisaDataModel
                {
                    InformationCode = data.InformationCode,
                    CategoryName = CategoryName,
                    DataName = data.DataName,
                    IsBack = data.IsBack,
                    IsOriginal = data.IsOriginal,
                    DataExplain = data.DataExplain,
                    DataCount = data.DataCount,
                    DataCode = "V" + DBTools.GetSeqNo("Visa_Data"),
                    IsNeed = 1,
                    IsTemplate = 0,
                    CategoryCode = CategoryCode,
                    CreateBy = currentUser.Code,
                    Createtime = DateTime.Now,
                    Number = Convert.ToInt32(count + 1)
                };
                _dao.Insert(entity);

                filemodel.DataCode = entity.DataCode;//新材料编码
                if (data.DataFilesList != null)
                {
                    foreach (var file in data.DataFilesList)
                    {
                        file.DataCode = filemodel.DataCode;
                        _fileDao.Insert(file);
                    }
                }
                i++;
            }
            sbStr.Append("｛" + dataName + "｝的材料数据");
            AddYlOperateHistory(InformationCode, sbStr.ToString(), currentUser, ip);
        }

        /// <summary>
        /// 根据材料编码字符串查询材料列表
        /// </summary>
        /// <param name="VisaDataStr">拼接材料字符串</param>
        /// <returns></returns>
        public IList<VisaDataModel> GetVisaDataByCodeStr(string VisaDataStr)
        {
            return _dao.Fetch("select * from Visa_Data where DataCode in(@0) ", VisaDataStr.Split(','));
        }

        /// <summary>
        /// 添加操作历史
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="remarks"></param>
        private void AddYlOperateHistory(string InformationCode, string remarks, CrmAccountModel currentUser, string ip)
        {
            //根据产品Code获取产品基本信息
            VisaInformationModel productModel = new VisaInformationModel();
            productModel = _productDao.GetVisaInfoByCode(InformationCode);

            //添加产品操作历史记录
            VisaInformationOperateHistoryModel entity = new VisaInformationOperateHistoryModel();
            entity.HistoryCode = "V" + DBTools.GetSeqNo("Visa_Data");
            entity.Operator = currentUser.Name;
            entity.OperatorLoginCode = currentUser.Code;
            entity.OperatorIp = ip;
            entity.OperatorTime = DateTime.Now;
            entity.ObjectCode = productModel.InformationCode;
            entity.ObjectName = productModel.InformationName;
            entity.Remark = remarks;
            entity.VState = productModel.State;
            _historyDao.Insert(entity);
        }

        public VisaCategoryModel GetCategoryByCode(string code)
        {
            return _categoryDao.FirstOrDefault("select * from Visa_Category where CategoryCode=@0 ", code);
        }
    }
}