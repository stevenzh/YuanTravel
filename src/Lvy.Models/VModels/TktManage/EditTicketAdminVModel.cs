using System.Collections.Generic;

namespace Lvy.VModels.Ticket
{
    public class EditTicketAdminVModel
    {
        /// <summary>
        /// 门票编号
        /// </summary>
        public string TicketId { get; set; }

        /// <summary>
        /// 用户列表
        /// </summary>
        public List<TicketAdminVModel> Admins { get; set; }
    }

    public class TicketAdminVModel
    {
        /// <summary>
        /// 是否选择 0：未选；1：选择
        /// </summary>
        public int Checked { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 专管员编号
        /// </summary>
        public int TktAdminId { get; set; }

        /// <summary>
        /// 专管员账号
        /// </summary>
        public string AccountCode { get; set; }
    }
}