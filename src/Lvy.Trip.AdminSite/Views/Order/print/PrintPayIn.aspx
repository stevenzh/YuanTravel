<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Lvy.VModels.Order.OrderPayInVModel>" %>

<%@ Import Namespace="Arch.Common.Utils" %>
<%@ Import Namespace="Lvy.VModels.Order" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html lang="zh-cn">
<head runat="server">
    <title>Title</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <script src="http://cdn.sh-cct.cn/lib/jquery/jquery.min.js"></script>
<%--    <script src="/WebRes/scripts/jquery-1.12.4.min.js"></script>--%>
</head>

<body>

    <script runat="server">

        private void Page_Load(Object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                OrderPayInVModel model = TempData["PayInPrintVModel"] as OrderPayInVModel;

                // 缴款客户名称
                ReportParameter para1 = new ReportParameter("CustomerName", model.OrderModel.CustomerName);
                // 汇款人
                ReportParameter para2 = new ReportParameter("Remitter", model.PayInModel.Remitter);
                // 税号
                ReportParameter para3 = new ReportParameter("TaxNumber", model.PayInModel.TaxNumber);
                // 线路名称
                ReportParameter para4 = new ReportParameter("LineName", model.OrderModel.LineName);
                // 团队人数
                ReportParameter para5 = new ReportParameter("Pax", model.OrderModel.TravellerCount.ToString());
                // 金额
                ReportParameter para6 = new ReportParameter("Amount", model.PayInModel.Amount.ToString());
                // 支票号
                ReportParameter para7 = new ReportParameter("JoinNo", model.PayInModel.JoinNo);
                // 支票金额
                ReportParameter para8 = new ReportParameter("PaymentType", model.PayInModel.PaymentType.ToString());
                // 备注
                ReportParameter para9 = new ReportParameter("Remark", model.PayInModel.Remark);
                // 加款
                ReportParameter para10 = new ReportParameter("AddAmount", model.PayInModel.AddAmount.ToString());

                string picroot = Arch.Common.AppSetting.Get("UploadFileRoot");

                // 付款凭证
                var pingzheng = Model.OrderFiles.Where(m => m.SourceType == "2" && m.Id == Model.PayInModel.BankFileId).FirstOrDefault();
                ReportParameter para11 = new ReportParameter("BankFilePath", "");
                if (pingzheng != null)
                {
                    para11 = new ReportParameter("BankFilePath", picroot + pingzheng.FilePath);
                }

                // 回传账单
                var zhangdan = Model.OrderFiles.Where(m => m.SourceType == "4" && m.Id == Model.PayInModel.BillFileId).FirstOrDefault();
                ReportParameter para12 = new ReportParameter("BillFilePath", "");
                if (zhangdan != null)
                {
                    para12 = new ReportParameter("BillFilePath", picroot + zhangdan.FilePath);
                }

                // 团号
                ReportParameter para13 = new ReportParameter("TourNo", model.PayInModel.TourNo);
                // 订单编号
                ReportParameter para14 = new ReportParameter("OrderCode", model.OrderModel.OrderCode);
                // 已付金额
                //ReportParameter para15 = new ReportParameter("TolPaid", model.OrderModel.TolPaid.ToString());
                // 尚欠金额
                //ReportParameter para16 = new ReportParameter("TolDebt", (amount - model.OrderModel.TolPaid).ToString());


                ReportViewer1.LocalReport.SetParameters(new ReportParameter[] {para1, para2, para3, para4, para5, para6, para7,
                    para8, para9, para10, para11, para12, para13, para14});
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", model.OrderFiles));
                //ReportViewer1.LocalReport.Refresh();

            }
        }
    </script>

    <form id="form1" runat="server">

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="100%" Width="100%" ProcessingMode="Local"
            AsyncRendering="False" InteractiveDeviceInfos="(集合)"
            WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" SizeToReportContent="True" ClientIDMode="AutoID">
            <LocalReport ReportPath="Views\RDLC\PayIn.rdlc" EnableExternalImages="true">
            </LocalReport>
        </rsweb:ReportViewer>

    </form>
    <script type="text/javascript">
        window.onload = function () {

            var filename = '<%=Model.OrderModel.BookingCustomer + "_账单_" +  Model.OrderModel.TourId %>';
            var classID = $("div[id^='P']").first().attr("id").substr(1, 32);
           // console.log("classID:" + classID);
            location.href = "/Reserved.ReportViewerWebControl.axd?Culture=2052&CultureOverrides=True&UICulture=2052&UICultureOverrides=True&ReportStack=1&ControlID=" + classID + "&Mode=true&OpType=Export&FileName=" + filename + "&ContentDisposition=OnlyHtmlInline&Format=PDF";
        }
    </script>

</body>
</html>
