<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Lvy.VModels.Order.OrderConfirmPrintVModel>" %>

<%@ Import Namespace="Arch.Common.Utils" %>
<%@ Import Namespace="Lvy.VModels.Order" %>
<%@ Import Namespace="Lvy.Trip.Biz" %>
<%@ Import Namespace="ZXing" %>
<%@ Import Namespace="ZXing.Common" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Import Namespace="Lvy.Web.Common.FileUpload " %>
<%@ Import Namespace="Lvy.Web.Common" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Title</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <script type="text/javascript" src="../../../Scripts/jquery-2.2.4.min.js"></script>
</head>

<body>

    <script runat="server">

        private void Page_Load(Object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                OrderConfirmPrintVModel model = TempData["OrderConfirmPrintVModel"] as OrderConfirmPrintVModel;

                var amount = model.OrderModel.TolYsPrice;
                if (model.OrderModel.RebateInBill == false)
                {
                    amount = model.OrderModel.InvoiceAmount;
                }
                var rmb = new RmbTools().ToDaxie(amount);

                // 团队编号
                ReportParameter para1 = new ReportParameter("TourName", model.TourPlan.TourNo);
                // 出团日期
                ReportParameter para2 = new ReportParameter("OutDate", model.OrderModel.OutDate.ToDateFormat());
                // 产品名称
                ReportParameter para3 = new ReportParameter("RouteName", model.LineModel.LineName);
                // 账单付款时限
                ReportParameter para4 = new ReportParameter("LastDate", Arch.Common.Utils.StringUtils.DateFormat(model.OrderModel.BillDeadline));
                // 行程天数
                ReportParameter para5 = new ReportParameter("RouteDays", model.LineModel.TravelDays.ToString());
                // 地接社
                ReportParameter para6 = new ReportParameter("ReceivingAgency", (model.LineModel.IsSelfGroup ? "自组团" : model.LineModel.CustomerName));
                // 本期付款金额
                ReportParameter para7 = new ReportParameter("Payment", Model.OrderModel.BillAmount.ToString());
                // 大写
                ReportParameter para8 = new ReportParameter("PaymentChina", new RmbTools().ToDaxie(Model.OrderModel.BillAmount));
                // 联系人信息
                ReportParameter para9 = new ReportParameter("ContactName", model.OrderModel.Managers);
                // 客户名称
                ReportParameter para10 = new ReportParameter("CustomerName", model.OrderModel.SettleCustomerName);
                // 参考航班
                ReportParameter para11 = new ReportParameter("RefFlights", "参考航班");
                // 费用不包含
                ReportParameter para12 = new ReportParameter("NoSellingContain", model.LineModel.PriceNoContain);
                // 费用包含
                ReportParameter para13 = new ReportParameter("SellingContain", model.LineModel.PriceContain);
                // 项目说明与特别约定
                ReportParameter para14 = new ReportParameter("PriceNotes", model.OrderModel.BillOffers);


                MultiFormatWriter mutiWriter = new MultiFormatWriter();
                BitMatrix bm = mutiWriter.encode(model.OrderModel.OrderCode, BarcodeFormat.CODE_39, 363, 150);
                Bitmap img = new BarcodeWriter().Write(bm);
                var path = Server.MapPath("/Files/Temp/");
                var filePath = path + model.OrderModel.OrderCode + ".png";
                img.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                // 条码文件路径
                ReportParameter para15 = new ReportParameter("BarcodePath", "file://" + filePath);
                // 总计金额
                ReportParameter para16 = new ReportParameter("InvoiceAmount", amount.ToString());
                // 总计金额大写
                ReportParameter para17 = new ReportParameter("InvoiceAmountChina", rmb);
                // 已付金额
                ReportParameter para18 = new ReportParameter("TolPaid", model.OrderModel.TolPaid.ToString());
                // 尚欠金额
                ReportParameter para19 = new ReportParameter("TolDebt", (amount - model.OrderModel.TolPaid).ToString());


                ReportViewer1.LocalReport.SetParameters(new ReportParameter[] {para1, para2, para3, para4, para5, para6, para7,
                    para8, para9, para10, para11, para12, para13, para14, para15, para16,para17, para18, para19 });
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("RouteDataSet", model.LineRoutes));
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("PersonDataSet", model.PersonModels));
                // ReportViewer1.LocalReport.Refresh();
                #region 账单文件保存方法
                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string extension;

                byte[] bytes = ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);
                string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + ".pdf";
                Lvy.Web.Common.FileUpload.UploadFileRequest request = new Lvy.Web.Common.FileUpload.UploadFileRequest();
                request.FileName = filename;

                request.FileStream = bytes;// Toolkit.Image.StreamToBytes();  //; Toolkit.Image.StreamToBytes(ms);

                // 所属客户code\文件类型
                request.VirtualPath = string.Format(@"order\{0}", model.OrderModel.OrderCode);

                //上传到指定的文件路径
                Lvy.Web.Common.FileUpload.UploadSoapClient client = new Lvy.Web.Common.FileUpload.UploadSoapClient();
                Lvy.Web.Common.FileUpload.UploadFileResponse response = client.UploadFile(request);


                string filePath2 = response.FilePath + response.FileName;

                #region 将路径信息写入到数据库表
                Lvy.Models.OrderDB.TpOrderFileModel model2 = new Lvy.Models.OrderDB.TpOrderFileModel();
                model2.KeyId = 0;
                model2.OrderCode = model.OrderModel.OrderCode;
                model2.FileName = filename;
                model2.FilePath = filePath2;
                model2.CreatedTime = DateTime.Now;
                model2.Remark = "";
                model2.IsDel = 0;
                model2.CreatedBy = Lvy.Web.Common.GlobalContext.Current.UserInfo.Code;
                model2.MediaType = "document";
                model2.SourceType = "3"; //账单
                Lvy.Trip.Biz.Order.OrderBiz _orderBiz = new Lvy.Trip.Biz.Order.OrderBiz();
                int c = _orderBiz.GetOrderFileModelVersion(model.OrderModel.OrderCode, "3");
                model2.Revision = c + 1;//设置版本号
                _orderBiz.AddOrderFile(model2);

                LogBiz.WriteOrderLog(model.OrderModel.OrderCode, "", GlobalContext.Current.UserInfo.Code, "新帐单制作 版本号:" + model2.Revision, "打印账单", 0);

                // 通知销售
                Lvy.Trip.Biz.Crm.AccountBiz _accountBiz = new Lvy.Trip.Biz.Crm.AccountBiz();
                var sales = _accountBiz.GetAccountCustomer(model.OrderModel.SalerCode);
                var remarks = "客户：" + model.OrderModel.CustomerName;
                if (!String.IsNullOrEmpty(sales.OpenID))
                {
                    string first = string.Format("{0} 账单制作或变更", sales.Name);
                    Lvy.Trip.Common.SendMessagClient.SendTemplateMessage(sales.OpenID, "8i7VY_GnnYnvTfmDRmntS079TzfJK2KmXV3LUOeOHM0", first,
                       model.OrderModel.OrderCode, model.LineModel.LineName, model.OrderModel.LineId.ToString(), "", "", remarks);
                }


                #endregion
                #endregion
            }
        }
    </script>

    <form id="form1" runat="server">

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="100%" Width="100%" ProcessingMode="Local"
            AsyncRendering="False" InteractiveDeviceInfos="(集合)"
            WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" SizeToReportContent="True" ClientIDMode="AutoID">
            <LocalReport ReportPath="Views\RDLC\Bill.rdlc" EnableExternalImages="true">
            </LocalReport>
        </rsweb:ReportViewer>

    </form>
    <script type="text/javascript">
        window.onload = function () {

            var filename = '<%=Model.OrderModel.BookingCustomer + "_账单_" +  Model.OrderModel.TourId %>';
            var classID = $("div[id^='P']").first().attr("id").substr(1, 32);
            //console.log($(this));
            location.href = "/Reserved.ReportViewerWebControl.axd?Culture=2052&CultureOverrides=True&UICulture=2052&UICultureOverrides=True&ReportStack=1&ControlID=" + classID + "&Mode=true&OpType=Export&FileName=" + filename + "&ContentDisposition=OnlyHtmlInline&Format=PDF";
        }
    </script>

</body>
</html>
