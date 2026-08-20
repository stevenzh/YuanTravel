using Arch.Common.Utils;
using Lvy.Visa.VModels;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Visa.Biz
{
    public class HotBiz
    {
        private TjHotVisaDao visaDao = new TjHotVisaDao();
        private ProductDao productDao = new ProductDao();
        private TjModuleDao moduleDao = new TjModuleDao();

        /// <summary>
        /// 根据模块编号查询线路列表
        /// </summary>
        /// <param name="ModuleCode"></param>
        /// <returns></returns>
        public IList<VisaTjHotVisaModel> SearchHotVisaList(string ModuleCode)
        {
            var list = visaDao.Fetch("SELECT * FROM Visa_TJ_HotVisa WHERE ModuleCode=@0 ", ModuleCode);
            foreach (var model in list)
            {
                model.ProductModel = productDao.GetVisaInfoByCode(model.ProductCode);
            }
            return list;
        }

        public IList<VisaCountryQModel> GetHotCountryList()
        {
            string tempSql = @"select a.Continent BanKuaiKey,b.ParentStr CountryCode,b.Name CountryName,b.Id AreaId
from Visa_Information a,BaseDestination b
where a.VisaCountryParentStr = b.ParentStr and a.VType = 1 and a.State = 5 and a.IsValid =1
group by a.Continent,b.Name,b.ParentStr,b.Id
order by b.Name ";

            List<VisaCountryQModel> countryList = visaDao.Query<VisaCountryQModel>(tempSql).ToList();

            return countryList;
        }

        /// <summary>
        /// 删除签证推荐线路
        /// </summary>
        /// <param name="HotVisaCode"></param>
        public void DeleteHotVisa(string HotVisaCode)
        {
            visaDao.Delete(" where HotVisaCode=@0 ", HotVisaCode);
        }

        public string CheckProIsExist(HotVisaQModel qmodel)
        {
            Sql sql = new Sql();
            sql.Append("select distinct ProductCode from Visa_TJ_HotVisa where  ModuleCode=@0 ", qmodel.ModuleCode);
            sql.Append(" and ProductCode in ( @0 )", qmodel.SelProCodes.Split(','));
            return string.Join(",", visaDao.Query<string>(sql.SQL, sql.Arguments));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public List<VisaProductQModel> GetHotVisaList()
        {
            var tempSql = @"select b.InformationCode ProductCode,b.InformationName ProductName,c.Name VisaIssuePlace,b.SellPrice SalePrice
,b.ImgUrl CountryImgUrl,b.VisaCountryParentStr CountryCode,d.Name CountryName
from Visa_TJ_HotVisa a,Visa_Information b,BaseDestination c,BaseDestination d
where a.ProductCode = b.InformationCode and b.VisaIssuePlace = c.ParentStr and b.VisaCountryParentStr = d.ParentStr
and a.ActiveType = 1 and b.State = 5 and b.IsValid = 1 and b.VType= 1
order by a.SortNum";

            List<VisaProductQModel> productList = visaDao.Query<VisaProductQModel>(tempSql).ToList();

            return productList;
        }

        /// <summary>
        /// 添加签证推荐线路
        /// </summary>
        /// <param name="qmodel"></param>
        public void AddProToHotVisa(HotVisaQModel qmodel)
        {
            var proArr = qmodel.SelProCodes.Substring(0, qmodel.SelProCodes.Length - 1).Split(',');
            var count = 1;
            var visamodel = visaDao.Fetch("SELECT * FROM Visa_TJ_HotVisa WHERE ModuleCode=@0 ORDER BY SortNum DESC ", qmodel.ModuleCode).FirstOrDefault();
            if (visamodel != null)
                count = visamodel.SortNum + 1;
            for (var i = 0; i < proArr.Length; i++)
            {
                var model = new VisaTjHotVisaModel();
                model.HotVisaCode = "V" + DBTools.GetSeqNo("00");
                model.ModuleCode = qmodel.ModuleCode;
                model.ProductCode = proArr[i];
                model.SortNum = count;
                model.ActiveType = qmodel.ActiveType;
                model.CreateDate = DateTime.Now;
                model.CreateBy = qmodel.CreateBy;

                visaDao.Insert(model);

                count++;
            }
        }

        public List<VisaProductQModel> GetB2bVisaList()
        {
            var tempSql = @"select b.InformationCode ProductCode,b.InformationName ProductName,c.Name VisaIssuePlace,
b.TradePrice,b.SellPrice SalePrice,b.ImgUrl CountryImgUrl,b.VisaCountryParentStr CountryCode,d.Name CountryName
 from Visa_Information b,BaseDestination c,BaseDestination d
 where b.VisaIssuePlace = c.ParentStr and b.VisaCountryParentStr = d.ParentStr
   and b.State = 5 and b.IsValid = 1 and b.VType= 2 ";

            List<VisaProductQModel> productList = visaDao.Query<VisaProductQModel>(tempSql).ToList();

            return productList;
        }

        /// <summary>
        /// 加急
        /// </summary>
        /// <returns></returns>
        public List<VisaProductQModel> GetUrgentVisaList()
        {
            var tempSql = @"select b.InformationCode ProductCode,b.InformationName ProductName,c.Name VisaIssuePlace,b.SellPrice SalePrice
,b.ImgUrl CountryImgUrl,b.VisaCountryParentStr CountryCode,d.Name CountryName
from Visa_TJ_HotVisa a,Visa_Information b,BaseDestination c,BaseDestination d
where a.ProductCode = b.InformationCode and b.VisaIssuePlace = c.ParentStr and b.VisaCountryParentStr = d.ParentStr
and a.ActiveType = 2 and b.State = 5 and b.IsValid = 1 and b.VType= 1
order by a.SortNum";

            List<VisaProductQModel> productList = visaDao.Query<VisaProductQModel>(tempSql).ToList();

            return productList;
        }

        /// <summary>
        /// 保存签证推荐线路排序
        /// </summary>
        /// <param name="qmodel"></param>
        public void SaveHotVisaSort(HotVisaQModel qmodel)
        {
            foreach (var item in qmodel.HotVisaList)
            {
                visaDao.Update("set SortNum=@1 where HotVisaCode=@0 ", item.HotVisaCode, item.SortNum);
            }
        }

        /// <summary>
        /// 查询推荐模块列表
        /// </summary>
        /// <returns></returns>
        public IList<VisaTjModuleModel> SearchModuleList()
        {
            return moduleDao.Query<VisaTjModuleModel>("select * from Visa_Tj_Module ").ToList();
        }

        /// <summary>
        /// 根据编号查询推荐模块详情
        /// </summary>
        /// <param name="moduleCode"></param>
        /// <returns></returns>
        public VisaTjModuleModel SearchModuleDetail(string moduleCode)
        {
            return moduleDao.Query<VisaTjModuleModel>(" select * from Visa_Tj_Module where ModuleCode=@0 ", moduleCode).FirstOrDefault();
        }
    }
}