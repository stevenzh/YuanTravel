using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Contract;
using Lvy.VModels.Contract;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Contract
{
    public class ContractInfoBiz : BaseBiz
    {
        public ContractInfoDao _contractDao = new ContractInfoDao();

        public ContractFilesDao _fileDao = new ContractFilesDao();

        public ContractTouristDao touristDao = new ContractTouristDao();

        public ContractShoppingDao shoppingDao = new ContractShoppingDao();

        public ContractPayItemDao payItemDao = new ContractPayItemDao();

        public ContractAdditionsDao _additionDao = new ContractAdditionsDao();

        public PagedList<ContractInfo> GetPageList(ConditionVModel vModel)
        {
            var sql = new Sql();
            //var userInfo = GlobalContext.Current.UserInfo;

            sql.Append(" SELECT a.* FROM ContractInfo a WHERE 1=1  ");

            if (!string.IsNullOrEmpty(vModel.contractNumber))
            {
                sql.Append(@" AND a.contractNumber like @0 ", AnsiLike(vModel.contractNumber));
            }
            if (!string.IsNullOrEmpty(vModel.orderCode))
            {
                sql.Append(@" AND a.orderCode like @0 ", AnsiLike(vModel.orderCode));
            }
            //产品名称
            if (!string.IsNullOrEmpty(vModel.routeName))
            {
                sql.Append(@" AND a.routeName like @0", AnsiLike(vModel.routeName));
            }

            sql.Append(" order by a.createTime");

            var list = _contractDao.Pager(vModel.PagedList.PageIndex, vModel.PagedList.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        public ContractVModel GetContractDetails(string orderCode)
        {
            ContractVModel vModel = new ContractVModel();
            string sql = "select * from ContractInfo a where a.orderCode=@0 ";
            vModel.Contract = _contractDao.FirstOrDefault(sql, orderCode);
            if (vModel.Contract != null)
            {
                vModel.FilesList = _fileDao.Fetch("select * from ContractFiles where contractId=@0 ", vModel.Contract.id);
                vModel.TouristList = touristDao.Fetch("select * from ContractTourist where contractId=@0 ", vModel.Contract.id);
                vModel.ShoppingList = shoppingDao.Fetch("select * from ContractShopping where contractId=@0 ", vModel.Contract.id);
                vModel.PayItemList = payItemDao.Fetch("select * from ContractPayItem where contractId=@0 ", vModel.Contract.id);
            }
            else
            {
                vModel.Contract = new ContractInfo();
            }
            return vModel;
        }

        public ContractInfo GetContractInfo(string contractNumber)
        {
            string sql = "select * from ContractInfo a where a.contractNumber=@0 ";
            return _contractDao.FirstOrDefault(sql, contractNumber);
        }

        public bool UpdateContractState(string contractNumber, string state, string message)
        {
            string sql = "update ContractInfo set status=@0,receiveMsg=@1 where contractNumber=@2 ";
            return _contractDao.Execute(sql, state, message, contractNumber) > 0;
        }

        /// <summary>
        /// 保存或修改合同数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public bool SaveUpdateContract(ContractVModel model, CrmAccountModel currentUser)
        {
            #region 合同数据

            string sql = "select * from ContractInfo where orderCode=@0 ";
            ContractInfo queryInfo = _contractDao.FirstOrDefault(sql, model.Contract.orderCode);
            ContractInfo contractInfo = CreateContractInfo(model, queryInfo, currentUser);

            #endregion 合同数据

            if (queryInfo != null)
            {
                int count = _contractDao.Update(contractInfo);
                if (model.FilesList != null && model.FilesList.Count > 0)
                {
                    int i = _fileDao.Execute(" where contractId=@0", queryInfo.id);
                    AddFiles(model.FilesList, contractInfo.id);
                }
                if (model.PayItemList != null && model.PayItemList.Count > 0)
                {
                    int i = payItemDao.Delete(" where contractId=@0 ", queryInfo.id);
                    AddPayItem(model.PayItemList, contractInfo.id);
                }
                if (model.ShoppingList != null && model.ShoppingList.Count > 0)
                {
                    int i = shoppingDao.Delete(" where contractId=@0 ", queryInfo.id);
                    AddShopping(model.ShoppingList, contractInfo.id);
                }
                if (model.TouristList != null && model.TouristList.Count > 0)
                {
                    touristDao.Delete(" where contractId=@0 ", queryInfo.id);
                    AddTourist(model.TouristList, contractInfo.id);
                }
                return true;
            }
            else
            {
                contractInfo.createBy = currentUser.Code;
                contractInfo.createTime = DateTime.Now;
                contractInfo.status = "0";
                var id = Convert.ToInt32(_contractDao.Insert(contractInfo));
                if (id > 0)
                {
                    AddFiles(model.FilesList, id);
                    AddPayItem(model.PayItemList, id);
                    AddShopping(model.ShoppingList, id);
                    AddTourist(model.TouristList, id);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 根据订单编号修改合同
        /// </summary>
        /// <returns></returns>
        public bool UpdateContractByOrderCode(ContractVModel model)
        {
            string sql = "select * from ContractInfo where orderCode=@0 ";
            ContractInfo queryInfo = _contractDao.FirstOrDefault(sql, model.Contract.orderCode);
            if (queryInfo == null) return false;
            queryInfo.qrCodeURL = model.Contract.qrCodeURL;
            queryInfo.signingURL = model.Contract.signingURL;
            queryInfo.fileURL = model.Contract.fileURL;
            queryInfo.viewURL = model.Contract.viewURL;
            queryInfo.contractNumber = model.Contract.contractNumber;
            queryInfo.status = "1";
            return _contractDao.Update(queryInfo) > 0;
        }

        public List<ContractAdditions> GetAdditionsList(string content)
        {
            Sql sql = new Sql();
            sql.Append("select * from ContractAdditions a where 1=1 ");
            if (!string.IsNullOrEmpty(content))
            {
                sql.Append(" and a.content like @0 ", AnsiLike(content));
            }
            var list = _additionDao.Fetch(sql.SQL, sql.Arguments);
            return list;
        }

        public ContractAdditions GetAdditionsDetails(int id)
        {
            return _additionDao.GetById(id);
        }

        public bool SaveUpdateAdditions(ContractAdditions model)
        {
            if (model.id > 0)
            {
                return _additionDao.Update(model) > 0;
            }
            return Convert.ToInt32(_additionDao.Insert(model)) > 0;
        }

        public bool DeleteAdditions(int id)
        {
            return _additionDao.Execute("delete from ContractAdditions where id=@0", id) > 0;
        }

        private ContractInfo CreateContractInfo(ContractVModel vModel, ContractInfo queryInfo, CrmAccountModel currentUser)
        {
            ContractInfo contractInfo = new ContractInfo();
            if (queryInfo != null)
            {
                queryInfo.additionContent = vModel.Contract.additionContent;
                queryInfo.agencyName = vModel.Contract.agencyName;
                queryInfo.agreeToBuy = vModel.Contract.agreeToBuy;
                queryInfo.arrivalCity = vModel.Contract.arrivalCity;
                queryInfo.agencyAddress = vModel.Contract.agencyAddress;
                queryInfo.attraction = vModel.Contract.attraction;
                queryInfo.company = vModel.Contract.company;
                queryInfo.compensationPeriod = vModel.Contract.compensationPeriod;
                queryInfo.contactEmail = vModel.Contract.contactEmail;
                queryInfo.contactName = vModel.Contract.contactName;
                queryInfo.contactPhone = vModel.Contract.contactPhone;
                queryInfo.contractNumber = vModel.Contract.contractNumber;
                queryInfo.coverage = vModel.Contract.coverage;
                queryInfo.createTime = DateTime.Now;
                queryInfo.creditCode = vModel.Contract.creditCode;
                queryInfo.deadline = vModel.Contract.deadline;
                queryInfo.deadlineDescription = vModel.Contract.deadlineDescription;
                queryInfo.deductStandardA = vModel.Contract.deductStandardA;
                queryInfo.deductStandardB = vModel.Contract.deductStandardB;
                queryInfo.departureCity = vModel.Contract.departureCity;
                queryInfo.disputeResolution = vModel.Contract.disputeResolution;
                queryInfo.endCity = vModel.Contract.endCity;
                queryInfo.endDate = vModel.Contract.endDate;
                queryInfo.entrustAgency = vModel.Contract.entrustAgency;
                queryInfo.formulaA = vModel.Contract.formulaA;
                queryInfo.freeNumber = vModel.Contract.freeNumber;
                queryInfo.freeTime = vModel.Contract.freeTime;
                queryInfo.groupId = vModel.Contract.groupId;
                queryInfo.groupMode = vModel.Contract.groupMode;
                queryInfo.guideServiceCost = vModel.Contract.guideServiceCost;
                queryInfo.hasShopping = vModel.Contract.hasShopping;
                queryInfo.isMultiSign = vModel.Contract.isMultiSign;
                queryInfo.leastCustomerNumbe = vModel.Contract.leastCustomerNumbe;
                queryInfo.licenseCode = vModel.Contract.licenseCode;
                queryInfo.localAgencyAddress = vModel.Contract.localAgencyAddress;
                queryInfo.localAgencyContact = vModel.Contract.localAgencyContact;
                queryInfo.localAgencyName = vModel.Contract.localAgencyName;
                queryInfo.localAgencyPhone = vModel.Contract.localAgencyPhone;
                queryInfo.localStandard = vModel.Contract.localStandard;
                queryInfo.localTransport = vModel.Contract.localTransport;
                queryInfo.longStandard = vModel.Contract.longStandard;
                queryInfo.longTransport = vModel.Contract.longTransport;
                queryInfo.mealNumber = vModel.Contract.mealNumber;
                queryInfo.mealStandard = vModel.Contract.mealStandard;
                queryInfo.orderCode = vModel.Contract.orderCode;
                queryInfo.otherLiability = vModel.Contract.otherLiability;
                queryInfo.otherPaymethod = vModel.Contract.otherPaymethod;
                queryInfo.otherResolution = vModel.Contract.otherResolution;
                queryInfo.overdueDaysA = vModel.Contract.overdueDaysA;
                queryInfo.overdueDaysA2 = vModel.Contract.overdueDaysA2;
                queryInfo.overdueDaysB = vModel.Contract.overdueDaysB;
                queryInfo.overdueDaysB2 = vModel.Contract.overdueDaysB2;
                queryInfo.overduePenaltyDayRatio = vModel.Contract.overduePenaltyDayRatio;
                queryInfo.passCity = vModel.Contract.passCity;
                queryInfo.payMethod = vModel.Contract.payMethod;
                queryInfo.premium = vModel.Contract.premium;
                queryInfo.productName = vModel.Contract.productName;
                queryInfo.routeName = vModel.Contract.routeName;
                queryInfo.seriousCompensateRatio = vModel.Contract.seriousCompensateRatio;
                queryInfo.servicePhone = vModel.Contract.servicePhone;
                queryInfo.shoppingViewSpot = vModel.Contract.shoppingViewSpot;
                queryInfo.signingAddress = vModel.Contract.signingAddress;
                queryInfo.signingEmail = vModel.Contract.signingEmail;
                queryInfo.signingFax = vModel.Contract.signingFax;
                queryInfo.signingIsJoin = vModel.Contract.signingIsJoin;
                queryInfo.signingMode = vModel.Contract.signingMode;
                queryInfo.signingName = vModel.Contract.signingName;
                queryInfo.signingPassNo = vModel.Contract.signingPassNo;
                queryInfo.signingPassType = vModel.Contract.signingPassType;
                queryInfo.signingPhone = vModel.Contract.signingPhone;
                queryInfo.signingPlace = vModel.Contract.signingPlace;
                queryInfo.signingSex = vModel.Contract.signingSex;
                queryInfo.signingTouristType = vModel.Contract.signingTouristType;
                queryInfo.solutionA = vModel.Contract.solutionA;
                queryInfo.solutionB = vModel.Contract.solutionB;
                queryInfo.startDate = vModel.Contract.startDate;
                queryInfo.otherAgency = vModel.Contract.otherAgency;
                queryInfo.timeLimitA = vModel.Contract.timeLimitA;
                queryInfo.timeLimitA2 = vModel.Contract.timeLimitA2;
                queryInfo.timeLimitB = vModel.Contract.timeLimitB;
                queryInfo.timeLimitB2 = vModel.Contract.timeLimitB2;
                queryInfo.totalCost = vModel.Contract.totalCost;
                queryInfo.totalGuideServiceCost = vModel.Contract.totalGuideServiceCost;
                queryInfo.tourGuideService = vModel.Contract.tourGuideService;
                queryInfo.transactorName = vModel.Contract.transactorName;
                queryInfo.transactorPhone = vModel.Contract.transactorPhone;
                queryInfo.travelItinerary = vModel.Contract.travelItinerary;
                queryInfo.tribunalName = vModel.Contract.tribunalName;
                queryInfo.templateId = vModel.Contract.templateId;
                queryInfo.modifyBy = currentUser.Code;
                queryInfo.modifyTime = DateTime.Now;
                return queryInfo;
            }
            else
            {
                contractInfo.additionContent = vModel.Contract.additionContent;
                contractInfo.agencyName = vModel.Contract.agencyName;
                contractInfo.agreeToBuy = vModel.Contract.agreeToBuy;
                contractInfo.arrivalCity = vModel.Contract.arrivalCity;
                contractInfo.agencyAddress = vModel.Contract.agencyAddress;
                contractInfo.attraction = vModel.Contract.attraction;
                contractInfo.company = vModel.Contract.company;
                contractInfo.compensationPeriod = vModel.Contract.compensationPeriod;
                contractInfo.contactEmail = vModel.Contract.contactEmail;
                contractInfo.contactName = vModel.Contract.contactName;
                contractInfo.contactPhone = vModel.Contract.contactPhone;
                contractInfo.contractNumber = vModel.Contract.contractNumber;
                contractInfo.coverage = vModel.Contract.coverage;
                contractInfo.createTime = DateTime.Now;
                contractInfo.creditCode = vModel.Contract.creditCode;
                contractInfo.deadline = vModel.Contract.deadline;
                contractInfo.deadlineDescription = vModel.Contract.deadlineDescription;
                contractInfo.deductStandardA = vModel.Contract.deductStandardA;
                contractInfo.deductStandardB = vModel.Contract.deductStandardB;
                contractInfo.departureCity = vModel.Contract.departureCity;
                contractInfo.disputeResolution = vModel.Contract.disputeResolution;
                contractInfo.endCity = vModel.Contract.endCity;
                contractInfo.endDate = vModel.Contract.endDate;
                contractInfo.entrustAgency = vModel.Contract.entrustAgency;
                contractInfo.formulaA = vModel.Contract.formulaA;
                contractInfo.freeNumber = vModel.Contract.freeNumber;
                contractInfo.freeTime = vModel.Contract.freeTime;
                contractInfo.groupId = vModel.Contract.groupId;
                contractInfo.groupMode = vModel.Contract.groupMode;
                contractInfo.guideServiceCost = vModel.Contract.guideServiceCost;
                contractInfo.hasShopping = vModel.Contract.hasShopping;
                contractInfo.isMultiSign = vModel.Contract.isMultiSign;
                contractInfo.leastCustomerNumbe = vModel.Contract.leastCustomerNumbe;
                contractInfo.licenseCode = vModel.Contract.licenseCode;
                contractInfo.localAgencyAddress = vModel.Contract.localAgencyAddress;
                contractInfo.localAgencyContact = vModel.Contract.localAgencyContact;
                contractInfo.localAgencyName = vModel.Contract.localAgencyName;
                contractInfo.localAgencyPhone = vModel.Contract.localAgencyPhone;
                contractInfo.localStandard = vModel.Contract.localStandard;
                contractInfo.localTransport = vModel.Contract.localTransport;
                contractInfo.longStandard = vModel.Contract.longStandard;
                contractInfo.longTransport = vModel.Contract.longTransport;
                contractInfo.mealNumber = vModel.Contract.mealNumber;
                contractInfo.mealStandard = vModel.Contract.mealStandard;
                contractInfo.orderCode = vModel.Contract.orderCode;
                contractInfo.otherLiability = vModel.Contract.otherLiability;
                contractInfo.otherPaymethod = vModel.Contract.otherPaymethod;
                contractInfo.otherResolution = vModel.Contract.otherResolution;
                contractInfo.overdueDaysA = vModel.Contract.overdueDaysA;
                contractInfo.overdueDaysA2 = vModel.Contract.overdueDaysA2;
                contractInfo.overdueDaysB = vModel.Contract.overdueDaysB;
                contractInfo.overdueDaysB2 = vModel.Contract.overdueDaysB2;
                contractInfo.overduePenaltyDayRatio = vModel.Contract.overduePenaltyDayRatio;
                contractInfo.passCity = vModel.Contract.passCity;
                contractInfo.payMethod = vModel.Contract.payMethod;
                contractInfo.premium = vModel.Contract.premium;
                contractInfo.productName = vModel.Contract.productName;
                contractInfo.routeName = vModel.Contract.routeName;
                contractInfo.seriousCompensateRatio = vModel.Contract.seriousCompensateRatio;
                contractInfo.servicePhone = vModel.Contract.servicePhone;
                contractInfo.shoppingViewSpot = vModel.Contract.shoppingViewSpot;
                contractInfo.signingAddress = vModel.Contract.signingAddress;
                contractInfo.signingEmail = vModel.Contract.signingEmail;
                contractInfo.signingFax = vModel.Contract.signingFax;
                contractInfo.signingIsJoin = vModel.Contract.signingIsJoin;
                contractInfo.signingMode = vModel.Contract.signingMode;
                contractInfo.signingName = vModel.Contract.signingName;
                contractInfo.signingPassNo = vModel.Contract.signingPassNo;
                contractInfo.signingPassType = vModel.Contract.signingPassType;
                contractInfo.signingPhone = vModel.Contract.signingPhone;
                contractInfo.signingPlace = vModel.Contract.signingPlace;
                contractInfo.signingSex = vModel.Contract.signingSex;
                contractInfo.signingTouristType = vModel.Contract.signingTouristType;
                contractInfo.solutionA = vModel.Contract.solutionA;
                contractInfo.solutionB = vModel.Contract.solutionB;
                contractInfo.startDate = vModel.Contract.startDate;
                contractInfo.otherAgency = vModel.Contract.otherAgency;
                contractInfo.timeLimitA = vModel.Contract.timeLimitA;
                contractInfo.timeLimitA2 = vModel.Contract.timeLimitA2;
                contractInfo.timeLimitB = vModel.Contract.timeLimitB;
                contractInfo.timeLimitB2 = vModel.Contract.timeLimitB2;
                contractInfo.totalCost = vModel.Contract.totalCost;
                contractInfo.totalGuideServiceCost = vModel.Contract.totalGuideServiceCost;
                contractInfo.tourGuideService = vModel.Contract.tourGuideService;
                contractInfo.transactorName = vModel.Contract.transactorName;
                contractInfo.transactorPhone = vModel.Contract.transactorPhone;
                contractInfo.travelItinerary = vModel.Contract.travelItinerary;
                contractInfo.tribunalName = vModel.Contract.tribunalName;
                contractInfo.templateId = vModel.Contract.templateId;
            }
            return contractInfo;
        }

        private bool AddTourist(List<ContractTourist> list, int contractId)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    ContractTourist touristInfo = new ContractTourist();
                    touristInfo.contractId = contractId;
                    touristInfo.address = item.address;
                    touristInfo.age = item.age;
                    touristInfo.gender = item.gender;
                    touristInfo.health = item.health;
                    touristInfo.isChildren = item.isChildren;
                    touristInfo.isSign = item.isSign;
                    touristInfo.name = item.name;
                    touristInfo.nation = item.nation;
                    touristInfo.nationality = item.nationality;
                    touristInfo.passNo = item.passNo;
                    touristInfo.passType = item.passType;
                    touristInfo.phone = item.phone;
                    touristDao.Insert(touristInfo);
                }
            }
            return true;
        }

        private bool AddShopping(List<ContractShopping> list, int contractId)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    ContractShopping shoppingInfo = new ContractShopping();
                    shoppingInfo.contractId = contractId;
                    shoppingInfo.date = item.date;
                    shoppingInfo.goods = item.goods;
                    shoppingInfo.memo = item.memo;
                    shoppingInfo.place = item.place;
                    shoppingInfo.shoppingPlace = item.shoppingPlace;
                    shoppingInfo.stayDuration = item.stayDuration;
                    shoppingDao.Insert(shoppingInfo);
                }
            }
            return true;
        }

        private bool AddPayItem(List<ContractPayItem> list, int contractId)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    ContractPayItem payItemInfo = new ContractPayItem();
                    payItemInfo.contractId = contractId;
                    payItemInfo.date = item.date;
                    payItemInfo.fee = item.fee;
                    payItemInfo.item = item.item;
                    payItemInfo.memo = item.memo;
                    payItemInfo.place = item.place;
                    payItemInfo.stayDuration = item.stayDuration;
                    payItemDao.Insert(payItemInfo);
                }
            }
            return true;
        }

        private bool AddFiles(List<ContractFiles> list, int contractId)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    ContractFiles fileInfo = new ContractFiles();
                    fileInfo.contractId = contractId;
                    fileInfo.fileName = item.fileName;
                    fileInfo.filePath = item.filePath;
                    fileInfo.createTime = DateTime.Now;
                    _fileDao.Insert(fileInfo);
                }
            }
            return true;
        }
    }
}