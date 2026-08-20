using Arch.Common.Utils;
using Lvy.Trip.Dao;
using Lvy.Visa.Models;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.Dao
{
    public class VisaProductDao : YuanDbRepository<VisaInformationModel>
    {
        public VisaInformationModel GetVisaInfoByCode(string informationCode)
        {
            return _repo.FirstOrDefault<VisaInformationModel>("select * from Visa_Information where InformationCode=@0", informationCode);
        }
    }

    public class VisaCategoryDao : YuanDbRepository<VisaCategoryModel>
    {
        public void AddVisaCategoryBath(string informationCode)
        {
            try
            {
                VisaCategoryModel model = new VisaCategoryModel();
                for (int i = 0; i < 5; i++)
                {
                    VisaCategoryModel cate = new VisaCategoryModel();
                    cate.CategoryCode = "V" + DBTools.GetSeqNo("Visa_Category");
                    switch (i)
                    {
                        case 0:
                            cate.CategoryName = "在职人员";
                            break;

                        case 1:
                            cate.CategoryName = "退休人员";
                            break;

                        case 2:
                            cate.CategoryName = "在校学生";
                            break;

                        case 3:
                            cate.CategoryName = "学龄前儿童";
                            break;

                        case 4:
                            cate.CategoryName = "自由职业者";
                            break;
                    }
                    cate.InformationCode = informationCode;
                    _repo.Insert(cate);
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "批量添加分类");
            }
        }
    }

    public class VisaDataDao : YuanDbRepository<VisaDataModel>
    {
        /// <summary>
        /// 获取某分类下面材料总数
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <param name="proCode"></param>
        /// <returns></returns>
        public int GetCountByCategoryCode(string categoryCode, string informationCode)
        {
            return _repo.ExecuteScalar<Int32>("select count(*) from Visa_Data where InformationCode=@0 and CategoryCode=@1", informationCode, categoryCode);
        }

        public VisaDataModel GetByCode(string informationCode, string dataCode)
        {
            return _repo.FirstOrDefault<VisaDataModel>("select * from Visa_Data where InformationCode=@0 and DataCode=@1", informationCode, dataCode);
        }
    }

    public class VisaDataFileDao : YuanDbRepository<VisaDataFileModel>
    {
        public List<VisaDataFileModel> GetDataFilesList(VisaDataFileModel filemodel)
        {
            return _repo.Fetch<VisaDataFileModel>("select * from Visa_DataFiles where InformationCode=@0 and DataCode=@1", filemodel.InformationCode, filemodel.DataCode);
        }

        public void UpdateVisaDataFile(VisaDataFileModel file)
        {
            _repo.Execute("update Visa_DataFiles set FileName=@2, FileUrl=@3 where InformationCode=@0 and FilesCode=@1", file.InformationCode, file.FilesCode, file.FileName, file.FileUrl);
        }
    }

    public class HistoryDao : YuanDbRepository<VisaInformationOperateHistoryModel> { }

    public class VisaOrderDao : YuanDbRepository<VisaOrderModel> { }

    public class ApplicanterDao : YuanDbRepository<VisaApplicanterModel> { }

    public class OperationHistoryDao : YuanDbRepository<VisaOperationHistoryModel> { }

    public class QuestionDao : YuanDbRepository<VisaCountryQuestionModel> { }

    public class DistrictDao : YuanDbRepository<VisaCountryConsularDistrictModel> { }

    public class CountryInfoDao : YuanDbRepository<VisaCountryInfoModel> { }
}