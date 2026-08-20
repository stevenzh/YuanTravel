using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Trip.Biz;
using Lvy.Visa.Dao;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.Web.Common;
using PetaPoco;
using System;

namespace Lvy.Visa.Biz
{
    public class QuestionBiz : BaseBiz
    {
        private QuestionDao _dao = new QuestionDao();

        /// <summary>
        /// 根据条件分页查询
        /// </summary>
        /// <param name="qmodel"></param>
        /// <returns></returns>
        public PagedList<VisaCountryQuestionModel> PagSearchQuestionList(VisaCountryQuestionQModel qmodel)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM Visa_CountryQuestion WHERE OwnerCode=@0 ", qmodel.OwnerCode);

            if (qmodel.QuestionModel != null)
            {
                if (!qmodel.QuestionModel.CountryName.IsNullOrEmpty())
                    sql.Append(" AND CountryName LIKE @0", AnsiLike(qmodel.QuestionModel.CountryName));
            }

            return _dao.Pager(qmodel.QuetionList.PageIndex, qmodel.QuetionList.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据编号查询详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public VisaCountryQuestionModel SearchQuestionDetail(string code)
        {
            return _dao.FirstOrDefault("SELECT * FROM Visa_CountryQuestion WHERE QuestionCode=@0", code);
        }

        /// <summary>
        /// 添加问题
        /// </summary>
        /// <param name="model"></param>
        public void AddQuestion(VisaCountryQuestionModel model)
        {
            model.QuestionCode = "V" + DBTools.GetSeqNo("Visa_Question");
            model.CreateDate = DateTime.Now;

            _dao.Insert(model);
        }

        /// <summary>
        /// 保存问题
        /// </summary>
        /// <param name="model"></param>
        public void SaveQuestion(VisaCountryQuestionModel model)
        {
            _dao.Execute("Update Visa_CountryQuestion set CountryCode=@1, CountryName=@2, Question=@3, Answer=@4,ModifyDate=now(), ModifyBy=@4 where QuestionCode=@0 ", model.QuestionCode,
                 model.CountryCode, model.CountryName, model.Question, model.Answer, model.ModifyBy);
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <param name="model"></param>
        public void Deletequestion(VisaCountryQuestionModel model)
        {
            _dao.Execute(" DELETE FRM Visa_CountryQuestion WHERE QuestionCode=@0", model.QuestionCode);
        }
    }
}