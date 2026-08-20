using System;
using System.ComponentModel;

namespace Lvy.VModels.Excel
{
    public class TourPlanExcelVModel
    {
        public string LineId { get; set; }

        [Description("团号")]
        public string TourNo { get; set; }

        [Description("路线名称")]
        public string LineName { get; set; }

        [Description("大交通")]
        public string TrafficType { get; set; }

        [Description("地接社")]
        public string Totakecommuntiy { get; set; }

        [Description("出境日期")]
        public DateTime OutDate { get; set; }

        [Description("出境时间")]
        public string DepartureTime { get; set; }

        [Description("出发班次/车次")]
        public string DepartBan { get; set; }

        [Description("出境岸口")]
        public string PortOfExit { get; set; }

        [Description("入境日期")]
        public DateTime? EntryDate { get; set; }

        [Description("入境时间")]
        public string EntryTime { get; set; }

        [Description("返回班次/车次")]
        public string Returnregular { get; set; }

        [Description("入境口岸")]
        public string PortOfEntry { get; set; }

        [Description("领队姓名")]
        public string Name { get; set; }

        [Description("领队证号")]
        public string TourCard { get; set; }

        [Description("线路行程")]
        public string Contents { get; set; }

        [Description("拼团信息及备注")]
        public string Remarks { get; set; }
    }

    public class LineRouteExcelVModel
    {
        [Description("团号")]
        public string LineName { get; set; }

        public string Title { get; set; }

        [Description("前往城市")]
        public string City { get; set; }

        [Description("前往国家/地区")]
        public string Contruny { get; set; }

        [Description("游览行程")]
        public string Contents { get; set; }

        [Description("是否过境")]
        public string IsGuoJin { get; set; }

        [Description("天数")]
        public int Days { get; set; }

        [Description("站点")]
        public string zhandian { get; set; }
    }

    public class TravellerModels
    {
        [Description("团号")]
        public string TourNo { get; set; }

        [Description("姓名")]
        public string Name { get; set; }

        [Description("英文名")]
        public string PinYin { get; set; }

        [Description("性别")]
        public string Sex { get; set; }

        [Description("生日")]
        public DateTime? DateOfBirth { get; set; }

        [Description("出生地")]
        public string PlaceOfBirth { get; set; }

        [Description("联系方式(手机)")]
        public string Phone { get; set; }

        [Description("证件类型")]
        public string PassType { get; set; }

        [Description("证件号")]
        public string PassNo { get; set; }

        [Description("签发地")]
        public string PlaceOfIssue { get; set; }

        [Description("发证日期")]
        public DateTime? DateOfIssue { get; set; }
    }

    public class TouristModels
    {
        [Description("姓名")]
        public string Name { get; set; }

        [Description("英文名")]
        public string PinYin { get; set; }

        [Description("性别")]
        public string Sex { get; set; }

        [Description("生日")]
        public DateTime? DateOfBirth { get; set; }

        [Description("出生地")]
        public string PlaceOfBirth { get; set; }

        [Description("联系方式(手机)")]
        public string Phone { get; set; }

        [Description("证件类型")]
        public string PassType { get; set; }

        [Description("证件号")]
        public string PassNo { get; set; }

        [Description("签发地")]
        public string PlaceOfIssue { get; set; }

        [Description("发证日期")]
        public DateTime? DateOfIssue { get; set; }
    }

    public class GuestModels
    {
        [Description("团号")]
        public string Id { get; set; }

        [Description("路线")]
        public string LineName { get; set; }

        [Description("姓名")]
        public string Name { get; set; }

        [Description("英文名")]
        public string PinYin { get; set; }

        [Description("性别")]
        public string Sex { get; set; }

        [Description("生日")]
        public DateTime? DateOfBirth { get; set; }

        [Description("出身地")]
        public string PlaceOfBirth { get; set; }

        [Description("客人电话")]
        public string Phone { get; set; }

        [Description("领队")]
        public string LinName { get; set; }

        [Description("代理商")]
        public string Booking { get; set; }

        [Description("护照种类")]
        public string PassType { get; set; }

        [Description("护照号")]
        public string PassNo { get; set; }

        [Description("签发日期")]
        public DateTime? DateOfIssue { get; set; }

        [Description("护照有效期")]
        public DateTime? DateOfExpiry { get; set; }

        [Description("签发地")]
        public string PlaceOfIssue { get; set; }

        [Description("护照说明")]
        public string Remark { get; set; }

        [Description("姓名")]
        public bool Name2 { get; set; }

        [Description("英文名")]
        public bool PinYin2 { get; set; }

        [Description("性别")]
        public bool Sex2 { get; set; }

        [Description("生日")]
        public bool DateOfBirth2 { get; set; }

        [Description("代理商")]
        public bool daili2 { get; set; }

        [Description("证件类型")]
        public bool PassType2 { get; set; }

        [Description("护照号")]
        public bool PassNo2 { get; set; }

        [Description("签发日期")]
        public bool DateOfIssue2 { get; set; }

        [Description("护照有效期")]
        public bool DateOfExpiry2 { get; set; }

        [Description("签发地")]
        public bool PlaceOfIssue2 { get; set; }

        [Description("护照说明")]
        public bool Remark2 { get; set; }
    }
}