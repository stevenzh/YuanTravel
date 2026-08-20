using Arch.Common.Utils;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz;
using Lvy.Visa.Dao;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Visa.Biz
{
    /// <summary>
    /// 签证后台使用
    /// </summary>
    public class ProductBiz : BaseBiz
    {
        private VisaProductDao _dao = new VisaProductDao();
        private VisaCategoryDao _categoryDao = new VisaCategoryDao();
        private HistoryDao _historyDao = new HistoryDao();
        private VisaDataDao _dataDao = new VisaDataDao();
        private VisaDataFileDao _fileDao = new VisaDataFileDao();

        /// <summary>
        /// 后台产品查询
        /// </summary>
        /// <param name="qModel"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public VisaInformationQModel GetInfoByCondition(VisaInformationQModel qModel)
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.*, bdd.Value VTypeValue, bdd1.Value VisaTypeValue, bdd2.Value StateValue, bdd3.Value ContinentValue, bdd4.Value VisaAreaValue
from Visa_Information vi left join CrmCustomer cc on cc.Code = vi.SupplierCode
inner join BaseDictionaryDetail bdd on vi.VType=bdd.`Key` and bdd.Name='VisaVTypeEnum'
inner join BaseDictionaryDetail bdd1 on vi.VisaType=bdd1.`Key` and bdd1.Name='VisaTypeEnum'
inner join BaseDictionaryDetail bdd2 on vi.State=bdd2.`Key` and bdd2.Name='VisaStateEnum'
inner join BaseDictionaryDetail bdd3 on vi.Continent=bdd3.`Key` and bdd3.Name='ContinentEnum'
inner join BaseDictionaryDetail bdd4 on vi.VisaArea=bdd4.`Key` and bdd4.Name='VisaAreaEnum'
where vi.IsValid=1 AND vi.OwnerCode=@0 ", qModel.OwnerCode);

            if (qModel.Info != null)
            {
                if (!qModel.Info.InformationCode.IsNullOrEmpty())
                {
                    sql.Append(" and vi.InformationCode like @0", AnsiLike(qModel.Info.InformationCode.Trim()));
                }
                if (!qModel.Info.TeamID.IsNullOrEmpty())
                {
                    sql.Append(" and vi.TeamID=@0", qModel.Info.TeamID);
                }
                if (!qModel.Info.InformationName.IsNullOrEmpty())
                {
                    sql.Append(" and vi.InformationName like @0", AnsiLike(qModel.Info.InformationName.Trim()));
                }
                if (qModel.Info.VisaType != default(int) && qModel.Info.VisaType != -1)
                {
                    sql.Append(" and vi.VisaType=@0", qModel.Info.VisaType);
                }
                if (qModel.Info.State != default(int) && qModel.Info.State != -1)
                {
                    sql.Append(" and vi.State=@0", qModel.Info.State);
                }
                if (qModel.Info.VType != default(int) && qModel.Info.VType != -1)
                {
                    sql.Append(" and vi.VType=@0", qModel.Info.VType);
                }
                if (qModel.Info.Continent != default(int) && qModel.Info.Continent != -1)
                {
                    sql.Append(" and vi.Continent=@0", qModel.Info.Continent);
                }
                if (!qModel.Info.VisaCountry.IsNullOrEmpty())
                {
                    sql.Append(" and vi.VisaCountry=@0", qModel.Info.VisaCountry);
                }
                if (!qModel.Info.VisaArea.IsNullOrEmpty() && qModel.Info.VisaArea != "-1")
                {
                    sql.Append(" and vi.VisaArea=@0", qModel.Info.VisaArea);
                }
                if (!qModel.Info.SupplierName.IsNullOrEmpty())
                {
                    sql.Append(" and cc.Name like @0", AnsiLike(qModel.Info.SupplierName));
                }
                if (!qModel.Info.CreateByName.IsNullOrEmpty())
                {
                    sql.Append(" and vi.CreateByName like @0", AnsiLike(qModel.Info.CreateByName));
                }
                if (!qModel.Info.PManageUserName.IsNullOrEmpty())
                {
                    sql.Append(" and vi.PManageUserName like @0", AnsiLike(qModel.Info.PManageUserName));
                }
            }

            sql.Append(" ORDER BY vi.CreateTime DESC ");

            qModel.VisaInformationList = _dao.Pager(qModel.VisaInformationList.PageIndex, qModel.VisaInformationList.PageSize, sql.SQL, sql.Arguments);

            return qModel;
        }

        public string CopyProduct(string InformationCode,  CrmAccountModel userInfo)
        {
            //被复制的产品
            VisaInformationModel oldProductObj = _dao.GetVisaInfoByCode(InformationCode);

            //复制上线或者是下线产品
            //if (oldProductObj.State == 5 || oldProductObj.State == 6)
            //{
            //新产品
            VisaInformationModel newProductObj = new VisaInformationModel();
            oldProductObj.InformationCode = "V" + oldProductObj.VType + DBTools.GetProductSeqNoByVisaInfo();
            oldProductObj.IsValid = 1;
            oldProductObj.State = 1;
            oldProductObj.CreateBy = userInfo.Code;
            oldProductObj.Createtime = DateTime.Now;
            _dao.Insert(oldProductObj);

            //签证分类
            if (oldProductObj.IsCategory == 1)
            {
                var oldCategoryList = GetCategroyList(InformationCode);
                if (oldCategoryList != null && oldCategoryList.Count() > 0)
                {
                    // EntitySet<Visa_Category> newCategroys = new EntitySet<Visa_Category>();
                    foreach (var categroy in oldCategoryList)
                    {
                        VisaCategoryModel newCategroy = new VisaCategoryModel();
                        newCategroy.InformationCode = newProductObj.InformationCode;
                        newCategroy.CategoryName = categroy.CategoryName;
                        newCategroy.CategoryCode = "V" + DBTools.GetSeqNo("Visa_Category");
                        _categoryDao.Insert(newCategroy);

                        //分类下面的材料

                        var oldVisaDataList = GetVisaDataList(InformationCode, categroy.CategoryCode);
                        if (oldVisaDataList != null && oldVisaDataList.Count() > 0)
                        {
                            //EntitySet<Visa_Data> visaDatas = new EntitySet<Visa_Data>();
                            foreach (var visadata in oldVisaDataList)
                            {
                                VisaDataModel newVisaData = new VisaDataModel();
                                newVisaData.DataCode = "V" + DBTools.GetSeqNo("Visa_Data");
                                newVisaData.CategoryCode = newCategroy.CategoryCode;
                                newVisaData.DataName = visadata.DataName;
                                newVisaData.InformationCode = newProductObj.InformationCode;
                                newVisaData.IsNeed = visadata.IsNeed;
                                newVisaData.IsTemplate = visadata.IsTemplate;
                                newVisaData.DataExplain = visadata.DataExplain;
                                newVisaData.IsOriginal = visadata.IsOriginal;
                                newVisaData.DataCount = visadata.DataCount;
                                newVisaData.IsBack = visadata.IsBack;
                                newVisaData.Number = visadata.Number;
                                newVisaData.CreateBy = userInfo.Code;
                                newVisaData.Createtime = DateTime.Now;
                                _dataDao.Insert(newVisaData);

                                //材料附件
                                var oldFileList = GetVisaMaterialFileList(InformationCode, visadata.DataCode);
                                if (oldFileList != null && oldFileList.Count() > 0)
                                {
                                    // EntitySet<Visa_DataFiles> newFiles = new EntitySet<Visa_DataFiles>();
                                    foreach (var dataFile in oldFileList)
                                    {
                                        VisaDataFileModel newFile = new VisaDataFileModel();
                                        newFile.FilesCode = "V" + DBTools.GetSeqNo("Visa_DataFiles");
                                        newFile.FileName = dataFile.FileName;
                                        newFile.FileUrl = dataFile.FileUrl;
                                        newFile.DataCode = newVisaData.DataCode;
                                        newFile.InformationCode = newProductObj.InformationCode;
                                        newFile.CreateBy = userInfo.Code;
                                        newFile.Createtime = DateTime.Now;

                                        _fileDao.Insert(newFile);
                                    }
                                    //if (newFiles.Count() > 0)
                                    //    newVisaData.Visa_DataFiles = newFiles;
                                }

                                // visaDatas.Add(newVisaData);
                            }
                            //if (visaDatas.Count() > 0)
                            //    newCategroy.Visa_Data = visaDatas;
                        }

                        //newCategroys.Add(newCategroy);

                        //if (newCategroys.Count() > 0)
                        //    newProductObj.Visa_Category = newCategroys;
                    }
                }
            }
            else
            {
                var oldVisaDataList = GetVisaDataList(InformationCode);
                if (oldVisaDataList != null && oldVisaDataList.Count() > 0)
                {
                    //EntitySet<Visa_Data> visaDatas = new EntitySet<Visa_Data>();
                    foreach (var visaData in oldVisaDataList)
                    {
                        VisaDataModel newData = new VisaDataModel();
                        newData.DataCode = "V" + DBTools.GetSeqNo("Visa_Data");
                        newData.InformationCode = newProductObj.InformationCode;
                        newData.DataName = visaData.DataName;
                        newData.DataExplain = visaData.DataExplain;
                        newData.IsNeed = visaData.IsNeed;
                        newData.IsTemplate = visaData.IsTemplate;
                        newData.CreateBy = userInfo.Code;
                        newData.Createtime = DateTime.Now;
                        _dataDao.Insert(newData);

                        var oldFileList = GetVisaMaterialFileList(InformationCode, visaData.DataCode);
                        if (oldFileList != null && oldFileList.Count() > 0)
                        {
                            // EntitySet<Visa_DataFiles> newFiles = new EntitySet<Visa_DataFiles>();
                            foreach (var dataFile in oldFileList)
                            {
                                VisaDataFileModel newFile = new VisaDataFileModel();
                                newFile.FilesCode = "V" + DBTools.GetSeqNo("Visa_DataFiles");
                                newFile.FileName = dataFile.FileName;
                                newFile.FileUrl = dataFile.FileUrl;
                                newFile.DataCode = newData.DataCode;
                                newFile.InformationCode = newProductObj.InformationCode;
                                newFile.CreateBy = userInfo.Code;
                                newFile.Createtime = DateTime.Now;
                                _fileDao.Insert(newFile);
                            }
                            //if (newFiles.Count() > 0)
                            //    newData.Visa_DataFiles = newFiles;
                        }

                        //visaDatas.Add(newData);
                    }
                    //if (visaDatas.Count() > 0)
                    //    newProductObj.Visa_Data = visaDatas;
                }
                //}
                //_session.Add(newProductObj);
            }

            return oldProductObj.InformationCode;
        }

        public bool IsExitVisaData(VisaDataModel visadata)
        {
            int row = GetVisaDataList(visadata.InformationCode, visadata.CategoryCode).Count();
            return row > 0 ? true : false;
        }

        /// <summary>
        /// 设置产品当前状态
        /// </summary>
        /// <param name="model"></param>      
        public void SetState(VisaInformationModel model, CrmAccountModel userInfo, string ip)
        {
            if (model.State == 3)
            {
                _dao.Update("SET State=@1, PManageUser=@2, PManageUserName=@3, PManageDate=now() WHERE InformationCode=@0", model.InformationCode, model.State, userInfo.Code, userInfo.Name);
            }
            else if (model.State == 5)
            {
                _dao.Update("SET State=@1, OnlineUser=@2, OnlineUserName=@3, OnlineDate=now() WHERE InformationCode=@0", model.InformationCode, model.State, userInfo.Code, userInfo.Name);
            }
            else
            {
                _dao.Update("SET State=@1 WHERE InformationCode=@0", model.InformationCode, model.State, userInfo.Code, userInfo.Name);
            }

            //记录产品操作历史
            switch (model.State)
            {
                case 2:
                    AddVisaOperateHistory(model.InformationCode, "提交审核", userInfo, ip);
                    break;
                case 3:
                    AddVisaOperateHistory(model.InformationCode, "开始审核", userInfo, ip);
                    break;
                case 4:
                    AddVisaOperateHistory(model.InformationCode, "审核不通过" + (string.IsNullOrEmpty(model.Remarks) ? "" : "," + model.Remarks), userInfo, ip);
                    break;
                case 5:
                    AddVisaOperateHistory(model.InformationCode, "设置产品上线", userInfo, ip);
                    break;
                case 6:
                    AddVisaOperateHistory(model.InformationCode, "设置产品下线" + (string.IsNullOrEmpty(model.Remarks) ? "" : "," + model.Remarks), userInfo, ip);
                    break;
            }
        }

        public void AddProductBaseInfo(VisaInformationModel model, CrmAccountModel currentUser, string ip)
        {
            model.InformationCode = "V" + model.VType + DBTools.GetProductSeqNoByVisaInfo();
            model.State = 1;
            model.IsValid = 1;
            model.Createtime = DateTime.Now;
            model.IsCategory = 1;
            _dao.Insert(model);

            //默认添加五个常用分类
            _categoryDao.AddVisaCategoryBath(model.InformationCode);
            var hisRemarks = "添加新产品";
            //添加操作历史
            AddYlOperateHistory(model.InformationCode, hisRemarks, currentUser , ip);
        }

        public void UpdateProductBaseInfo(VisaInformationModel model, CrmAccountModel currentUser, string ip)
        {
            if (model.IsHurry == 0)
            {
                model.IsHurryName = "不可以";
            }
            else
            {
                model.IsHurryName = "可以";
            }
            _dao.Update(model);
            var hisRemarks = "修改产品基本信息 code=" + model.InformationCode;
            //添加操作历史
            AddYlOperateHistory(model.InformationCode, hisRemarks, currentUser, ip);
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
            productModel = _dao.GetVisaInfoByCode(InformationCode);

            //添加产品操作历史记录
            VisaInformationOperateHistoryModel entity = new VisaInformationOperateHistoryModel();
            entity.HistoryCode = "V" + DBTools.GetSeqNo("Visa_History");
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

        public bool CheckProductNameIsExists(string informationName, string informationCode)
        {
            int row = 0;
            if (informationCode.IsNullOrEmpty())
            {
                row = _dao.ExecuteScalar<Int32>("select count(*) from Visa_Information where InformationName=@0 and IsValid=1 and State<>6 ", informationName);
            }
            else
            {
                row = _dao.ExecuteScalar<Int32>("select count(*) from Visa_Information where InformationName=@0 and IsValid=1 and State<>6 and InformationCode<>@1 ", informationName, informationCode);
            }
            return (row > 0 ? true : false);
        }

        public string SaveVisaCategroy(VisaCategoryModel model, CrmAccountModel userInfo, string ip)
        {
            model.CategoryCode = "V" + DBTools.GetSeqNo("Visa_Category");
            _categoryDao.Insert(model);

            AddVisaOperateHistory(model.InformationCode, "添加分类 【" + model.CategoryName + "】", userInfo, ip);

            return model.CategoryCode;
        }

        /// <summary>
        /// 添加产品操作历史
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="remarks"></param>
        private void AddVisaOperateHistory(string InformationCode, string remarks, CrmAccountModel userInfo, string ip)
        {
            //根据产品Code获取产品基本信息
            VisaInformationModel productModel = new VisaInformationModel();
            productModel = _dao.GetVisaInfoByCode(InformationCode);

            //添加产品操作历史记录
            VisaInformationOperateHistoryModel operateHistoryModel = new VisaInformationOperateHistoryModel();
            operateHistoryModel.ObjectCode = productModel.InformationCode;
            operateHistoryModel.ObjectName = productModel.InformationName;
            operateHistoryModel.Remark = remarks;
            operateHistoryModel.VState = productModel.State;
            operateHistoryModel.HistoryCode = "V" + DBTools.GetSeqNo("Visa_History");
            operateHistoryModel.Operator = userInfo.Name;
            operateHistoryModel.OperatorLoginCode = userInfo.Code;
            operateHistoryModel.OperatorIp = ip;
            operateHistoryModel.OperatorTime = DateTime.Now;

            _historyDao.Insert(operateHistoryModel);
        }

        public IList<VisaInformationOperateHistoryModel> SearchInforOperateHistorys(YlInformationOperateHistoryQModel model)
        {
            return _historyDao.Fetch(@"select vioh.*, bdd.Value VStateValue from Visa_Information_OperateHistory vioh
inner join BaseDictionaryDetail bdd on vioh.VState = bdd.`Key` and bdd.Name='VisaStateEnum'
where vioh.ObjectCode=@0 ORDER BY vioh.OperatorTime DESC ", model.InformationCode);
        }

        public List<VisaCategoryModel> GetCategroyList(string InformationCode)
        {
            return _categoryDao.Fetch("select * from Visa_Category where InformationCode=@0", InformationCode);
        }

        public VisaCategoryModel GetNameByCode(string categoryCode)
        {
            return _categoryDao.FirstOrDefault("select * from Visa_Category where CategoryCode=@0", categoryCode);
        }

        public VisaInformationModel GetProductBaseInfoByCode(string InformationCode)
        {
            return _dao.FirstOrDefault("select * from Visa_Information where InformationCode=@0", InformationCode);
        }

        public VisaInformationModel GetProductByCode(string code)
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.*, cc.Name VisaIssuePlaceName, bdd.Value InterviewTypeValue, bdd1.Value VisaTypeValue, bdd2.Value ContinentValue, bdd3.Value VisaAreaValue
from  Visa_Information vi inner join BaseDestination cc on cc.ParentStr = vi.VisaIssuePlace
inner join BaseDictionaryDetail bdd on vi.InterviewType=bdd.`Key` and bdd.Name='InterviewTypeEnum'
inner join BaseDictionaryDetail bdd1 on vi.VisaType=bdd1.`Key` and bdd1.Name='VisaTypeEnum'
inner join BaseDictionaryDetail bdd2 on vi.Continent=bdd2.`Key` and bdd2.Name='ContinentEnum'
inner join BaseDictionaryDetail bdd3 on vi.VisaArea=bdd3.`Key` and bdd3.Name='VisaAreaEnum'
where vi.InformationCode=@0 ", code);

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得签证所有文件附件
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<VisaDataFileModel> GetVisaMaterialFileList(string code)
        {
            return _fileDao.Fetch("select * from Visa_DataFiles where InformationCode=@0 ", code);
        }

        public List<VisaDataFileModel> GetVisaMaterialFileList(string code, string dataCode)
        {
            return _fileDao.Fetch("select * from Visa_DataFiles where InformationCode=@0 and DataCode=@1 ", code, dataCode);
        }

        /// <summary>
        /// 取得签证材料说明【全部人群】
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<VisaDataModel> GetVisaDataList(string code)
        {
            return _dataDao.Fetch("select * from Visa_Data where InformationCode=@0 ", code);
        }

        public List<VisaDataModel> GetVisaDataList(string code, string category)
        {
            return _dataDao.Fetch("select * from Visa_Data where InformationCode=@0 and CategoryCode=@1 ", code, category);
        }
    }
}