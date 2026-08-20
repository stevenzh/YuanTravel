using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Controllers
{
    /// <summary>
    /// 二维码生成
    /// </summary>
    public class QrCodeController : Controller
    {
        public static bool GetQRCode(string strContent, MemoryStream ms)
        {
            ErrorCorrectionLevel Ecl = ErrorCorrectionLevel.M; //误差校正水平
            string Content = strContent;//待编码内容
            QuietZoneModules QuietZones = QuietZoneModules.Two;  //空白区域
            int ModuleSize = 12;//大小
            var encoder = new QrEncoder(Ecl);
            QrCode qr;
            if (encoder.TryEncode(Content, out qr))//对内容进行编码，并保存生成的矩阵
            {
                var render = new GraphicsRenderer(new FixedModuleSize(ModuleSize, QuietZones));
                render.WriteToStream(qr.Matrix, ImageFormat.Png, ms);
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成线路预览二维码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Line(string id)
        {
            using (var ms = new MemoryStream())
            {
                string stringtest = "http://yuanwx.sh-cct.cn/line/details/" + id + "?tourId=0";
                GetQRCode(stringtest, ms);
                Response.ContentType = "image/Png";
                Response.OutputStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
                Response.End();
            }

            return View();
        }

    }
}