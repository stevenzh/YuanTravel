<%@ Page Language="C#" %>

<%@ Import Namespace="Lvy.VModels.Op" %>
<%@ Import Namespace="Lvy.VModels.Order" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Title</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
</head>

<body>

    <script runat="server">

        private void Page_Load(Object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                List<PrintTravellerVModel> TpTravellerList = TempData["TourTraveller"] as List<PrintTravellerVModel>;

                OutBandHeadModel model = TempData["TourHeadInfo"] as OutBandHeadModel;
                if (model == null)
                {
                    model = new OutBandHeadModel();
                }
                //组团社序号。
                ReportParameter GroupNum2 = new ReportParameter("GroupNum2", model.ZuTuanNo);
                //  团队编号
                ReportParameter LineNameSign = new ReportParameter("GroupNum1", model.LineNameSign);
                //年份
                ReportParameter Years = new ReportParameter("Year", model.Years);
                //领队名称
                ReportParameter GuideName = new ReportParameter("LZName1", "");
                //领队证号
                ReportParameter GuideNo = new ReportParameter("LZNums1", "");
                if (model.IsContainsLingDui)
                {
                    //领队名称
                    GuideName = new ReportParameter("LZName1", model.GuideName);
                    //领队证号
                    GuideNo = new ReportParameter("LZNums1", model.GuideNo);
                }


                //设置出境年月日期
                ;
                if (!string.IsNullOrEmpty(model.OutDate))
                {
                    string dd = DateTime.ParseExact(model.OutDate.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                    DateTime dtOutDate = Convert.ToDateTime(dd);//.ToDateTime();
                    model.LeaveYear = dtOutDate.Year.ToString();
                    model.LeaveMonth = dtOutDate.Month.ToString().PadLeft(2, '0');
                    model.LeaveDay = dtOutDate.Day.ToString();
                }
                //设置入境年月日期
                if (model.EntryDate != null)
                {
                    model.EnterYear = model.EntryDate.Value.Year.ToString();
                    model.EnterMonth = model.EntryDate.Value.Month.ToString().PadLeft(2, '0');
                    model.EnterDay = model.EntryDate.Value.Day.ToString();
                }
                //出境日期
                ReportParameter Date1 = new ReportParameter("Date1", model.LeaveYear + "   " + model.LeaveMonth + "   " + model.LeaveDay);
                //出境口岸
                ReportParameter City1 = new ReportParameter("City1", model.PortOfExit);

                //入境日期
                ReportParameter Date2 = new ReportParameter("Date2", model.EnterYear + "   " + model.EnterMonth + "   " + model.EnterDay);
                //入境口岸
                ReportParameter City2 = new ReportParameter("City2", model.PortOfExit);
                //取消出入境日期及口岸。
                if (model.IsContainsEnterDateAndPosition)
                {
                    //出境日期
                    Date1 = new ReportParameter("Date1", "");
                    //出境口岸
                    City1 = new ReportParameter("City1", "");

                    //入境日期
                    Date2 = new ReportParameter("Date2", "");
                    //入境口岸
                    City2 = new ReportParameter("City2", "");
                }



                ReportParameter RenCount = new ReportParameter("RenCount", model.TravellerCount.ToString());
                ReportParameter ManCount = new ReportParameter("ManCount", model.ManCount.ToString());
                ReportParameter MenCount = new ReportParameter("MenCount", model.WomenCount.ToString());
                ReportParameter LineName = new ReportParameter("LineName", model.LineName);

                //组团社名称及电话
                ReportParameter ZuTuanName = new ReportParameter("ZTSName", model.ZuTuanName);
                ReportParameter ZuTuanContact = new ReportParameter("Tel1", model.ZuTuanContact);

                //接待社名称及电话
                ReportParameter ReceptionName = new ReportParameter("JTSName", model.ReceptionName);
                ReportParameter ReceptionContact = new ReportParameter("Tel2", model.ReceptionContact);

                List<OutBandTravellerModel> travellerList = new List<OutBandTravellerModel>();
                foreach (var m in TpTravellerList)
                {
                    OutBandTravellerModel travellerModel = new OutBandTravellerModel();
                    travellerModel.TravellerName = m.Name;
                    travellerModel.TravellerSpell = m.PinYin;
                    travellerModel.TravellerSex = m.Sex;
                    travellerModel.TravellerBirthday = (m.DateOfBirth == null) ? "" : m.DateOfBirth.Value.ToString("yyyy/MM/dd");
                    travellerModel.Birthplace = m.PlaceOfBirth;
                    travellerModel.CardNum = m.PassNo;
                    travellerModel.IssueAt = m.PlaceOfIssue;// + "   " + (m.DateOfIssue == null ? "" : m.DateOfIssue.Value.ToString("yyyy/MM/dd"));
                    travellerModel.IssueDate = (m.DateOfIssue == null ? "" : m.DateOfIssue.Value.ToString("yyyy/MM/dd"));
                    if (m.IsLeader)
                    {
                        travellerList.Insert(0, travellerModel);
                    }
                    else
                    {
                        travellerList.Add(travellerModel);
                    }
                }


                ReportViewer1.LocalReport.SetParameters(new ReportParameter[] {GroupNum2,LineNameSign,Years,GuideName,GuideNo,Date1,City1,Date2,City2,
                       RenCount,ManCount,MenCount,LineName,ZuTuanName,ZuTuanContact,ReceptionName,ReceptionContact
                   });
                ReportViewer1.LocalReport.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource("myds", travellerList));
                // ReportViewer1.LocalReport.Refresh();
            }


            //     ReportParameter GroupNum2 = new ReportParameter("GroupNum2", "bbbbb");
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] {GroupNum2
            //        });

        }

    </script>

    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="600px" Width="921px" ProcessingMode="Local"
            AsyncRendering="False" Font-Names="Verdana" Font-Size="8pt" InteractiveDeviceInfos="(集合)"
            WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" SizeToReportContent="True">
            <LocalReport ReportPath="Views\RDLC\出境名单.rdlc">
            </LocalReport>
        </rsweb:ReportViewer>

    </form>
</body>
</html>
