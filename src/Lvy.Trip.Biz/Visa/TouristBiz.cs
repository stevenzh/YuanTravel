using Lvy.Trip.Biz;
using Lvy.Visa.Dao;
using Lvy.Visa.Models;
using Lvy.Web.Common;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.Biz
{
    public class TouristBiz : BaseBiz
    {
        private ApplicanterDao _touristDao = new ApplicanterDao();

        public List<VisaApplicanterModel> GetTouristList(string orderCode)
        {
            return _touristDao.Fetch("select * from Visa_Applicanter where OrderCode=@0", orderCode);
        }

        public VisaApplicanterModel GetTouristById(int id)
        {
            return _touristDao.GetById(id);
        }

        public VisaApplicanterModel GetTouristInfo(string orderCode, int applicanterID)
        {
            return _touristDao.FirstOrDefault("select * from Visa_Applicanter where OrderCode=@0 and ID=@1", orderCode, applicanterID);
        }

        public void UpdateTourist(VisaApplicanterModel model)
        {
            _touristDao.Update(model);
        }

        public void DeleteTourist(int id)
        {
            var entity = _touristDao.GetById(id);
            if (entity.IsValid == 1)
                entity.IsValid = 0;
            else
                entity.IsValid = 1;

            _touristDao.Update(entity);
        }

        public void AddTourist(VisaApplicanterModel model)
        {
            _touristDao.Insert(model);
        }

        //private void AddBathApplyGuests(IList<VisaApplicanterModel> triplist, String orderCode)
        //{
        //    if (null != triplist && triplist.Count > 0)
        //    {
        //        foreach (var trip in triplist)
        //        {
        //            //申请人
        //            var tripObj = new VisaApplicanterModel();

        //            tripObj.ApplicanterCode = "V" + DBTools.GetSeqNo("12");
        //            trip.ApplicanterCode = tripObj.ApplicanterCode;
        //            tripObj.OrderCode = orderCode;
        //            tripObj.Pinyin = ""; // OperateCommon.ConvertHanZiToPinYin(tripObj.Name);//获取拼音的方法
        //            tripObj.Status = 0;
        //            tripObj.Isvalid = 1;
        //            tripObj.CreatDate = DateTime.Now;
        //            tripObj.MidifyDate = DateTime.Now;
        //            applicanterDao.Insert(tripObj);
        //        }
        //    }
        //}
    }
}