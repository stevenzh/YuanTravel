using Lvy.Models;
using Lvy.Models.BaseDB;
using System.Collections.Generic;

namespace Lvy.VModels.Base
{
    public class EditBusPointVModel : BaseVModel
    {
        /// <summary>
        /// 接送类型
        /// </summary>
        public List<KeyValueBean> JsTypeList
        {
            get
            {
                var beans = new List<KeyValueBean>
                                {
                                    new KeyValueBean {Key = "1", Value = "只接不送"},
                                    new KeyValueBean {Key = "2", Value = "只送不接"},
                                    new KeyValueBean {Key = "3", Value = "接/送"}
                                };
                return beans;
            }
        }

        /// <summary>
        /// 上车点
        /// </summary>
        public BaseBusPointModel BusPoint { get; set; }
    }
}