using System;
using System.Collections.Generic;

namespace Lvy.VModels.Contract
{
    /// <summary>
    /// 封装合同接口参数接收实体
    /// </summary>
    public class ContractApiModel
    {
        public string ERPContractId { get; set; }

        public string templateId { get; set; }

        public string contractNumber { get; set; }

        public string callbackURL { get; set; }

        public bool isMultiSignatory { get; set; }

        public string supplementaryClause { get; set; }

        public List<int> publicAttachments { get; set; }

        public List<Attachments> attachments { get; set; }

        public TravelAgency travelAgency { get; set; }

        public Itinerary itinerary { get; set; }

        public TouristsInfo touristsInfo { get; set; }

        public Signatory signatory { get; set; }

        public EntrustedTravelAgency entrustedTravelAgency { get; set; }

        public Cost cost { get; set; }

        public List<LocalTravelAgencies> localTravelAgencies { get; set; }

        public Insurance insurance { get; set; }

        public Dispute dispute { get; set; }

        public List<Activities> activities { get; set; }

        public bool hasShopping { get; set; }

        public string shoppingViewSpot { get; set; }

        public List<Shoppings> shoppings { get; set; }

        public GroupAgreement groupAgreement { get; set; }
    }

    public class Attachments
    {
        public string url { get; set; }

        public string type { get; set; }
    }

    public class TravelAgency
    {
        public string transactorName { get; set; }
        public string transactorPhone { get; set; }

        public string agencyName { get; set; }

        public string travelAgencyLicenseNumber { get; set; }
        public string businessLicenseNumber { get; set; }
        public AgencyAddress agencyAddress { get; set; }
        public string contactName { get; set; }
        public string contactPhone { get; set; }
        public string servicePhone { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
    }

    public class AgencyAddress
    {
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string zip { get; set; }
        public string description { get; set; }
    }

    public class Itinerary
    {
        public int groupMode { get; set; }

        public DescriptionA departureCity { get; set; }

        public DescriptionA passCity { get; set; }

        public DescriptionA arrivalCity { get; set; }

        public DescriptionA endCity { get; set; }

        public DescriptionA attraction { get; set; }

        public DescriptionA hotel { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public string groupId { get; set; }

        public DescriptionB longTransport { get; set; }

        public DescriptionB localTransport { get; set; }

        public DescriptionC freeTime { get; set; }

        public DescriptionC meal { get; set; }

        public string tourGuideServiceType { get; set; }

        public string routeName { get; set; }

        public List<Routes> routes { get; set; }

        public TouristsInfo touristsInfo { get; set; }

        public Signatory signatory { get; set; }

        public EntrustedTravelAgency entrustedTravelAgency { get; set; }
    }

    public class DescriptionA
    {
        public string description { get; set; }
    }

    public class DescriptionB
    {
        public string description { get; set; }

        public string standard { get; set; }
    }

    public class DescriptionC
    {
        public string description { get; set; }

        public int number { get; set; }
    }

    public class Routes
    {
        public int day { get; set; }

        public int stop { get; set; }

        public Departure departure { get; set; }

        public string description { get; set; }

        public Transport transport { get; set; }

        public List<Meals> meals { get; set; }
    }

    public class Departure
    {
        public string country { get; set; }

        public string state { get; set; }

        public string city { get; set; }

        public string description { get; set; }
    }

    public class Transport
    {
        public string type { get; set; }

        public string number { get; set; }

        public string driverName { get; set; }
        public bool hasAC { get; set; }

        public string standard { get; set; }
    }

    public class Meals
    {
        public string type { get; set; }

        public string place { get; set; }

        public string standard { get; set; }
    }

    public class TouristsInfo
    {
        public int totalNumber { get; set; }

        public int adultNumber { get; set; }

        public int childNumber { get; set; }

        public List<Tourists> tourists { get; set; }
    }

    public class Tourists
    {
        public int number { get; set; }

        public bool isSigner { get; set; }

        public string name { get; set; }

        public string gender { get; set; }

        public int age { get; set; }

        public bool isChild { get; set; }

        public string nationality { get; set; }

        public string race { get; set; }

        public string phone { get; set; }

        public string health { get; set; }

        public string address { get; set; }

        public IDCard ID { get; set; }
    }

    public class IDCard
    {
        public string IDType { get; set; }

        public string IDNumber { get; set; }
    }

    public class Signatory
    {
        public string signingPlace { get; set; }

        public string name { get; set; }
        public IDCard ID { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string fax { get; set; }
        public int mode { get; set; }
    }

    public class EntrustedTravelAgency
    {
        public string agencyName { get; set; }
    }

    public class Cost
    {
        public decimal guideServiceCost { get; set; }

        public decimal totalGuideServiceCost { get; set; }

        public decimal totalCost { get; set; }

        public DateTime deadline { get; set; }

        public string deadlineDescription { get; set; }

        public string paymentMethodDescription { get; set; }

        public string overduePenaltyDayRatio { get; set; }
    }

    public class LocalTravelAgencies
    {
        public string description { get; set; }
        public string agencyName { get; set; }
        public LocalAgencyAddress agencyAddress { get; set; }

        public string contactName { get; set; }

        public string contactPhone { get; set; }
    }

    public class LocalAgencyAddress
    {
        public string description { get; set; }
    }

    public class Insurance
    {
        public bool agreeToBuy { get; set; }

        public string productName { get; set; }

        public string company { get; set; }

        public decimal coverage { get; set; }

        public decimal premium { get; set; }
    }

    public class Dispute
    {
        public int resolution { get; set; }
        public string tribunalName { get; set; }
    }

    public class Activities
    {
        public DateTime date { get; set; }

        public string place { get; set; }

        public string item { get; set; }
        public decimal fee { get; set; }

        public string stayDuration { get; set; }

        public string memo { get; set; }
    }

    public class Shoppings
    {
        public DateTime date { get; set; }

        public string place { get; set; }

        public string shoppingPlace { get; set; }
        public string goods { get; set; }

        public string stayDuration { get; set; }

        public string memo { get; set; }
    }

    public class GroupAgreement
    {
        public string resolution { get; set; }

        public int leastCustomerNumber { get; set; }

        public bool agreeToChangeLine { get; set; }

        public string mergeToCompanyName { get; set; }

        public PartAPenalty partAPenalty { get; set; }

        public PartAPenalty partBPenalty { get; set; }

        public string compensationPeriod { get; set; }

        public string seriousCompensateRatio { get; set; }

        public string otherLiability { get; set; }
    }

    public class PartAPenalty
    {
        public string solution { get; set; }
        public string overdueDaysA { get; set; }
        public string overdueDaysB { get; set; }

        public Cancel cancel { get; set; }
    }

    public class Cancel
    {
        public string timeLimit { get; set; }

        public List<Standards> standards { get; set; }

        public Interrupt interrupt { get; set; }
    }

    public class Standards
    {
        public string start { get; set; }

        public string end { get; set; }
        public string ratio { get; set; }
    }

    public class Interrupt
    {
        public string timeLimit { get; set; }

        public List<string> formula { get; set; }
    }

    public class ReqHeader
    {
        public string appId { get; set; }

        /// <summary>
        /// 签名KEY
        /// </summary>
        public string signKey { get; set; }

        /// <summary>
        /// 接口地址
        /// </summary>
        public string econtractUrl { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        public string econtractVersion { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string timestamp { get; set; }

        /// <summary>
        /// 签名字符串
        /// </summary>
        public string signature { get; set; }
    }

    public class ReceiveMsg
    {
        public ReceiveMsg()
        {
            if (data == null) data = new ContratcMsg();
        }

        public string apiVersion { get; set; }

        public ContratcMsg data { get; set; }
    }

    public class ContratcMsg
    {
        public string content { get; set; }

        /// <summary>
        /// 合同状态，1:已生成，2:已签署，3:已作废，4:签署（用于多人签署），5:作废（用于多人签署）
        /// </summary>
        public string state { get; set; }

        public string contractNumber { get; set; }

        public string signatoryIDNumber { get; set; }

        public string signatoryPhone { get; set; }

        public string sign { get; set; }

        public ContractUrl url { get; set; }
    }

    public class ContractUrl
    {
        public string signingURL { get; set; }

        public string QRCodeURL { get; set; }

        public string fileURL { get; set; }
    }
}