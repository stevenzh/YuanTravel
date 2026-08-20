using Lvy.Models.BaseDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lvy.Trip.Biz.Product
{
    public class TpLineBusPointBiz : BaseBiz
    {
        private readonly TpLineBusPointDao _bpDao = new TpLineBusPointDao();

        /// <summary>
        /// 根据Id获取上车点
        /// </summary>
        /// <param name="id">线路上车点Id</param>
        /// <returns></returns>
        public TpLineBusPointModel GetBusPointById(int id)
        {
            return _bpDao.FirstOrDefault(@"SELECT * FROM TpLineBusPoint WHERE id=@0", id);
        }

        /// <summary>
        /// 根据线路Id获取已选上车点
        /// </summary>
        /// <param name="lineId">线路Id</param>
        /// <returns></returns>
        public List<TpLineBusPointModel> GetBusPointsByLineId(string lineId)
        {
            return _bpDao.Fetch(@"SELECT * FROM TpLineBusPoint WHERE LineId=@0", lineId);
        }

        /// <summary>
        /// 根据多个ID获取上车点列表数据
        /// </summary>
        /// <param name="strBusPoindId"></param>
        /// <returns></returns>
        public List<TpLineBusPointModel> GetBusPointByManyId(string strBusPoindId)
        {
            return _bpDao.Fetch(@" select * from TpLineBusPoint where Id in ( " + strBusPoindId + " ) ");
        }

        /// <summary>
        /// 获取选择上车点列表数据
        /// </summary>
        /// <param name="baseBusPointList">基础上车点列表</param>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public List<BusPointItemVModel> GetSelectBusPointsList(List<BaseBusPointModel> baseBusPointList, SelectBusPointVModel vModel)
        {
            var selectList = new List<BusPointItemVModel>();
            //获取所有已选上车点
            var selectedList = GetBusPointsByLineId(vModel.Line.LineId);
            string pattern = @".*" + vModel.BusPointName + ".*";

            foreach (var item in baseBusPointList)
            {
                if (!vModel.BusPointName.IsNullOrEmpty() && !Regex.IsMatch(item.BusPoint, pattern))
                    continue;
                //从已选的上车点中寻找对应的结果
                var selectedItem = selectedList.FirstOrDefault(p => p.BusPointId == item.Id);
                if (selectedItem != null)
                {
                    selectList.Add(new BusPointItemVModel()
                    {
                        Checked = true,
                        IsJie = (selectedItem.JsType == 1 || selectedItem.JsType == 3),
                        IsSong = (selectedItem.JsType == 2 || selectedItem.JsType == 3),
                        BusPointModel = selectedItem,
                        GroupId = item.GroupId,
                    });
                }
                else
                {
                    selectList.Add(new BusPointItemVModel()
                    {
                        Checked = false,
                        IsJie = (item.JsType == 1 || item.JsType == 3),
                        IsSong = (item.JsType == 2 || item.JsType == 3),
                        BusPointModel = new TpLineBusPointModel
                        {
                            LineId = vModel.Line.LineId,
                            BusPointId = item.Id,
                            BusPoint = item.BusPoint,
                            JsType = item.JsType,
                            JsTime = item.JieSongTime
                        },
                        GroupId = item.GroupId
                    });
                }
            }

            foreach (var item in selectedList)
            {
                var ss = selectList.Where(m => m.BusPointModel.BusPointId == item.BusPointId).FirstOrDefault();
                if (ss == null)
                {
                    selectList.Add(new BusPointItemVModel()
                    {
                        Checked = true,
                        IsJie = (item.JsType == 1 || item.JsType == 3),
                        IsSong = (item.JsType == 2 || item.JsType == 3),
                        BusPointModel = item,
                        //GroupId = item.GroupId,
                    });
                }
            }

            return selectList;
        }

        /// <summary>
        /// 保存线路上车点
        /// </summary>
        /// <param name="items"></param>
        /// <param name="modifiedBy"></param>
        /// <param name="ownerCode"></param>
        public void SaveBusPoint(IList<BusPointItemVModel> items, string modifiedBy, string ownerCode)
        {
            // todo: 代码需要改进。
            var modifiedTime = DateTime.Now;
            var add = new List<TpLineBusPointModel>();
            var update = new List<TpLineBusPointModel>();
            var delete = new List<TpLineBusPointModel>();

            if (items == null || items.Count == 0)
                return;
            foreach (var item in items)
            {
                if (item.Checked && item.BusPointModel.Id == 0)
                {
                    #region 新增

                    int jsType = 0;
                    if (item.IsJie && item.IsSong)
                    {
                        jsType = 3;
                    }
                    else if (!item.IsJie && item.IsSong)
                    {
                        jsType = 2;
                    }
                    else if (item.IsJie && !item.IsSong)
                    {
                        jsType = 1;
                    }
                    else
                    {
                        jsType = 0;
                    }
                    add.Add(new TpLineBusPointModel()
                    {
                        LineId = item.BusPointModel.LineId,
                        BusPointId = item.BusPointModel.BusPointId,
                        BusPoint = item.BusPointModel.BusPoint,
                        PlaceOfReturn = item.BusPointModel.PlaceOfReturn,
                        JiePrice = item.IsJie ? item.BusPointModel.JiePrice : 0,
                        SongPrice = item.IsSong ? item.BusPointModel.SongPrice : 0,
                        JsType = jsType,
                        Remarks = "",
                        JsTime = item.BusPointModel.JsTime,
                        ModifiedBy = modifiedBy,
                        ModifiedTime = modifiedTime
                    });

                    #endregion 新增
                }
                else if (item.Checked && item.BusPointModel.Id > 0)
                {
                    #region 更新

                    var entity = GetBusPointById(item.BusPointModel.Id);
                    entity.JiePrice = item.IsJie ? item.BusPointModel.JiePrice : 0;
                    entity.SongPrice = item.IsSong ? item.BusPointModel.SongPrice : 0;
                    entity.JsTime = item.BusPointModel.JsTime;
                    entity.PlaceOfReturn = item.BusPointModel.PlaceOfReturn;
                    entity.Remarks = item.BusPointModel.Remarks;
                    if (item.IsJie && item.IsSong)
                    {
                        entity.JsType = 3;
                    }
                    else if (!item.IsJie && item.IsSong)
                    {
                        entity.JsType = 2;
                    }
                    else if (item.IsJie && !item.IsSong)
                    {
                        entity.JsType = 1;
                    }
                    else
                    {
                        entity.JsType = 0;
                    }
                    update.Add(entity);

                    #endregion 更新
                }
                else if (!item.Checked && item.BusPointModel.Id > 0)
                {
                    //删除
                    delete.Add(GetBusPointById(item.BusPointModel.Id));
                }
            }

            using (var tran = _bpDao.GetTransaction())
            {
                add.ForEach(p => _bpDao.Insert(p));
                update.ForEach(p => _bpDao.Update(p));
                delete.ForEach(p => _bpDao.Delete(p));

                tran.Complete();
            }
        }

        /// <summary>
        /// 新增线路上车点
        /// </summary>
        /// <param name="models"></param>
        public void InsertLineBusPoint(List<TpLineBusPointModel> models)
        {
            using (var tran = _bpDao.GetTransaction())
            {
                foreach (var record in models)
                {
                    _bpDao.Insert(record);
                }
                tran.Complete();
            }
        }
    }
}