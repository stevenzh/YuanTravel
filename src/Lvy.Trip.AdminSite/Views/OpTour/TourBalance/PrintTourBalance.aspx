<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Lvy.VModels.Tour.TourBalanceVModel>" %>

<%@ Import Namespace="Arch.Common.Utils" %>
<%@ Import Namespace="Lvy.VModels.Tour" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Title</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <script src="/WebRes/scripts/jquery-1.12.4.min.js"></script>
</head>

<body>

    <script runat="server">

        private void Page_Load(Object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TourBalanceVModel model = TempData["OrderConfirmPrintVModel"] as TourBalanceVModel;

                // 团号
                ReportParameter para1 = new ReportParameter("TourNo", model.Balance.TourNo);
                // 线路名称
                ReportParameter para2 = new ReportParameter("LineName", model.Balance.ProductName);
                // 毛利率
                ReportParameter para3 = new ReportParameter("OutDate", model.Balance.OutDate.ToDateFormat());
                // 天数
                ReportParameter para4 = new ReportParameter("DayNum", model.Balance.TravelDays.ToString());
                // 游客人数
                ReportParameter para5 = new ReportParameter("TouristCount", model.Balance.Num.ToString());
                // 导游蚺属
                ReportParameter para6 = new ReportParameter("LeaderCount", "1");
                // 导游名称
                ReportParameter para7 = new ReportParameter("LeaderName", (string.IsNullOrEmpty(model.Balance.GuideName) ? "无" : model.Balance.GuideName));
                // 成人人数
                ReportParameter para8 = new ReportParameter("AuditPax", model.Balance.AuditPax.ToString());
                // 儿童人数
                ReportParameter para9 = new ReportParameter("ChildPax", model.Balance.ChildPax.ToString());
                // 老人人数
                ReportParameter para10 = new ReportParameter("OldPax", model.Balance.OldPax.ToString());

                // 收入
                ReportParameter para11 = new ReportParameter("TotalPrice", model.Balance.YingShou.ToString());
                // 已收
                ReportParameter para12 = new ReportParameter("YiShou", model.Balance.YiShou.ToString());
                // 成本
                ReportParameter para13 = new ReportParameter("TotalCost", model.Balance.TotalCost.ToString());
                // 毛利
                ReportParameter para14 = new ReportParameter("MaoLi", model.Balance.MaoLi.ToString());
                //
                ReportParameter para15 = new ReportParameter("GrossProfit", model.Balance.MaoLi.ToString());
                //// 审核人
                //ReportParameter para15 = new ReportParameter("CWAuditBy", model.Balance.CWAuditBy);
                //// 审核时间
                //ReportParameter para16 = new ReportParameter("CWAuditTime", model.Balance.CWAuditTime.ToString());
                //// 制单人
                //ReportParameter para17 = new ReportParameter("CreatedBy", model.Balance.CreatedBy);

                ReportViewer1.LocalReport.SetParameters(new ReportParameter[] {para1, para2, para3, para4, para5, para6, para7,
                    para8, para9, para10, para11, para12, para13, para14, para15 });
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CostDataSet", model.CostList));
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("OrderDataSet", model.Orders));
                //ReportViewer1.LocalReport.Refresh();
            }
        }
    </script>

    <form id="form1" runat="server">

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="100%" Width="100%" ProcessingMode="Local"
            AsyncRendering="False" InteractiveDeviceInfos="(集合)"
            WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" SizeToReportContent="True" ClientIDMode="AutoID">
            <LocalReport ReportPath="Views\RDLC\LineBalance.rdlc" EnableExternalImages="true">
            </LocalReport>
        </rsweb:ReportViewer>

    </form>
    <script type="text/javascript">
        window.onload = function () {

            var filename = '<%=Model.Balance.TourNo + "_单团核算_" +  Model.Balance.OutDate %>';
            var classID = $("div[id^='P']").first().attr("id").substr(1, 32);
            //console.log($(this));
            location.href = "/Reserved.ReportViewerWebControl.axd?Culture=2052&CultureOverrides=True&UICulture=2052&UICultureOverrides=True&ReportStack=1&ControlID=" + classID + "&Mode=true&OpType=Export&FileName=" + filename + "&ContentDisposition=OnlyHtmlInline&Format=PDF";
        }
    </script>

</body>
</html>
