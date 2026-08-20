using Arch.Common.Utils;
using log4net;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Product
{
    /// <summary>
    /// 旅游线路
    /// </summary>
    public class TpLineBiz : BaseBiz
    {
        private readonly TpLineDao _lineDao = new TpLineDao();
        private readonly TpLineFileDao _fileDao = new TpLineFileDao();
        private readonly TpProductDao _productDao = new TpProductDao();
        private readonly TpLineRouteDao routeDao = new TpLineRouteDao();
        private readonly TpLineTrafficDao _trafficDao = new TpLineTrafficDao();
        private readonly TpLineBusPointDao lineBusPointDao = new TpLineBusPointDao();
        private readonly TpLineAdminDao lineAdminDao = new TpLineAdminDao();
        private readonly TpLineSuiteDao packageDao = new TpLineSuiteDao();
        private readonly TpLineTagMapDao tagMapDao = new TpLineTagMapDao();
        private readonly TpTourPlanDao tourDao = new TpTourPlanDao();

        private readonly BaseTagBiz baseTagBiz = new BaseTagBiz();
        private readonly DestinationBiz destBiz = new DestinationBiz();
        private readonly TpLineRouteBiz routBiz = new TpLineRouteBiz();
        private readonly TpLineBusPointBiz lineBusPointBiz = new TpLineBusPointBiz();
        private readonly PhotoBiz _photoBiz = new PhotoBiz();

        private ILog logger = LogManager.GetLogger(typeof(TpLineBiz));

        /// <summary>
        /// 获取线路列表
        /// </summary>
        /// <param name="vModel">线路列表视图实体</param>
        /// <returns></returns>
        public TpLineVModel GetLineList(TpLineVModel vModel)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM TpLine WHERE IsValid=1 AND OwnerCode=@0", Ansi(vModel.OwnerCode));

            #region 组织查询条件
            if (!vModel.IsImport.IsNullOrEmpty())
                sql.Append(@" AND IsImport=@0", (vModel.IsImport == "1" ? true : false));

            if (vModel.OwnerCode != vModel.CustomerCode)
                sql.Append(@" AND CustomerCode=@0", Ansi(vModel.CustomerCode));

            if (!vModel.LineId.IsNullOrEmpty())
            {
                sql.Append(@" AND LineId=@0", Ansi(vModel.LineId));
            }
            if (!vModel.LineName.IsNullOrEmpty())
            {
                sql.Append(@" AND LineName LIKE @0", AnsiLike(vModel.LineName));
            }
            if (!vModel.ArriveDest.IsNullOrEmpty())
            {
                sql.Append(@" AND ArriveDest LIKE @0", AnsiLeftLike(vModel.ArriveDest));
            }
            if (!vModel.LineScope.IsNullOrEmpty())
            {
                sql.Append(@" AND LineScope=@0", Ansi(vModel.LineScope));
            }
            if (!vModel.LineType.IsNullOrEmpty())
            {
                sql.Append(@" AND LineType=@0", Ansi(vModel.LineType));
            }
            if (!vModel.CustomerName.IsNullOrEmpty())
            {
                sql.Append(@" AND CustomerName LIKE @0", AnsiLike(vModel.CustomerName));
            }
            if (!vModel.TravelDays.IsNullOrEmpty())
            {
                sql.Append(@" AND TravelDays=@0", Ansi(vModel.TravelDays));
            }


            //分组条件查询
            if (!vModel.CrmTeamId.IsNullOrEmpty())
            {
                sql.Append(@" and TeamID=@0 ", vModel.CrmTeamId);
            }

            #endregion 组织查询条件

            sql.Append(@" ORDER BY ModifiedTime DESC");

            vModel.LineList = _lineDao.Pager(vModel.LineList.PageIndex, vModel.LineList.PageSize, sql.SQL, sql.Arguments);
            /*
             * 碍于分页跨表查询限制，暂时作如下处理：
             * 1.在查询出线路数据；
             * 2.遍历线路，通过一次请求将相关专管员信息取出；
             * 3.将专管员与线路匹配
            */
            if (vModel.LineList.Items != null && vModel.LineList.Items.Count > 0)
            {
                var sql4Admin = new Sql();
                var lines = vModel.LineList.Items;
                sql4Admin.Append(@"SELECT * FROM TpLineAdmin WHERE LineId IN (@0)", lines.Select(t => t.LineId).ToArray());
                var admins = lineAdminDao.Fetch(sql4Admin.SQL, sql4Admin.Arguments);
                if (admins != null && admins.Count > 0)
                {
                    vModel.LineList.Items.ForEach(p => p.Admins = admins.FindAll(m => m.LineId == p.LineId));
                }
            }

            return vModel;
        }

        /// <summary>
        /// 根据线路Id获取线路信息
        /// </summary>
        /// <param name="lineId">线路Id</param>
        /// <returns></returns>
        public TpLineModel GetLineById(string lineId)
        {
            return _lineDao.FirstOrDefault(@"SELECT tl.*, ct.TeamName FROM TpLine tl, CrmTeam ct WHERE tl.TeamID=ct.TeamID and tl.LineID=@0", lineId);
        }

        /// <summary>
        /// 根据团计划Id获取线路信息
        /// </summary>
        /// <param name="tourId">线路Id</param>
        /// <returns></returns>
        public TpLineModel GetLineByTour(int tourId)
        {
            return _lineDao.FirstOrDefault(@"SELECT TpLine.* FROM TpLine INNER JOIN TpTourPlan ON TpTourPlan.LineId=TpLine.LineId WHERE TpTourPlan.Id=@0", tourId);
        }

        #region 新增线路

        #region 新增初始化

        /// <summary>
        /// 根据目的地编号获取目的地名称
        /// </summary>
        /// <param name="arriveDest"></param>
        /// <returns></returns>
        private string GetArriveDestName(string arriveDest)
        {
            var dest = destBiz.GetByStr(arriveDest);
            return dest == null ? "" : dest.Name;
        }

        /// <summary>
        /// 获得子产品列表
        /// </summary>
        /// <param name="teamCode"></param>
        /// <returns></returns>
        //public List<TpProductModel> GetPrudoctItem(string teamCode)
        //{
        //    return _productDao.Query(@"SELECT * FROM TpProducts WHERE TeamCode=@0", teamCode).ToList();
        //}

        /// <summary>
        /// 获取新增线路视图对象
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public AddLineVModel GetAddLineVModel(string lineId)
        {
            var model = new AddLineVModel
            {
                TpLine = GetLineById(lineId)
            };
            model.ArriveDestName = GetArriveDestName(model.TpLine.ArriveDest);
            //model.LineAdmin = lineAdminBiz.GetByLineId(lineId) ?? new TpLineAdminModel();
            model.LineItemList = new TpProductBiz().GetProductByTeam(model.TpLine.TeamID);
            // model.Team = new TeamBiz().GetTeam(model.TpLine.TeamID);

            return model;
        }

        #endregion 新增初始化

        #region 新增提交

        /// <summary>
        /// 新增线路
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns>新产生的Id</returns>
        public string AddLine(AddLineVModel vModel, CrmAccountModel userInfo)
        {
            var lineWithName = GetLineByName(vModel.TpLine.LineName, userInfo.OwnerCode);
            if (lineWithName != null && lineWithName.Count > 0)
                return "";  // 同名处理

            var modifiedTime = DateTime.Now;

            #region 逻辑处理

            var model = vModel.TpLine;                          //线路
            SetNewLineDefaultValue(model, modifiedTime, userInfo);
            var routes = new List<TpLineRouteModel>();          //行程
            for (var i = 1; i <= model.TravelDays; i++)
            {
                routes.Add(new TpLineRouteModel
                {
                    Days = i,
                    ModifiedBy = userInfo.Code,
                    ModifiedTime = modifiedTime
                });
            }
            var maps = new List<TpLineTagMapModel>();
            model.Themes = (vModel.ThemeIds == null) ? "" : vModel.ThemeIds.Join("|");
            var themeIds = vModel.ThemeIds ?? new string[] { }; //所选标签
            var tags = baseTagBiz.GetTags(userInfo.OwnerCode);                    //系统标签
            maps = (from id in themeIds                         //线路-标签Map
                    where !id.IsNullOrEmpty()
                    let tagModel = tags.FirstOrDefault(p => p.TagName == id)
                    where null != tagModel
                    select new TpLineTagMapModel
                    {
                        TagId = tagModel.Id,
                        TagName = tagModel.TagName
                    }).ToList();

            // 默认将创建者作为线路专管员
            var lineAdmin = new TpLineAdminModel
            {
                AccountCode = userInfo.Code,
                IsPrimary = 1,
                Department = (userInfo.CustomerCode == userInfo.OwnerCode) ? 1 : 0
            };

            #endregion 逻辑处理


            #region 执行

            using (var scope = new TransactionScope())
            {
                _lineDao.Insert(model);

                foreach (var route in routes)
                {
                    route.LineId = model.LineId;
                    routeDao.Insert(route);
                }

                foreach (var map in maps)
                {
                    map.LineId = model.LineId;
                    tagMapDao.Insert(map);
                }

                lineAdmin.LineId = model.LineId;
                lineAdminDao.Insert(lineAdmin);

                //添加默认的套餐.
                var packageModel = new TpLineSuiteModel();
                packageModel.LineId = model.LineId;
                packageModel.PackageDescr = "默认套餐";
                packageDao.Insert(packageModel);

                scope.Complete();
            }

            //baseTagBiz.AsynAddHitMulti(themeIds);

            #endregion 执行

            return model.LineId;
        }

        /// <summary>
        /// 为新增线路赋予默认值
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modifiedTime"></param>
        ///
        private void SetNewLineDefaultValue(TpLineModel model, DateTime modifiedTime, CrmAccountModel userInfo)
        {
            model.LineId = "L" + DBTools.GetLineSeqNo();
            model.LineState = 2;        //默认下线
            model.IsValid = 1;          //默认有效
            model.IsSelfGroup = true;   //默认自组团
            model.CustomerCode = userInfo.CustomerCode;
            model.CustomerName = userInfo.CustomerName;
            model.CreatedBy = userInfo.Code;
            model.CreatedTime = modifiedTime;
            model.ModifiedBy = userInfo.Code;
            model.ModifiedTime = modifiedTime;
            model.OwnerCode = userInfo.OwnerCode;
            // model.TrafficType = 3;      //交通类型设置为3飞机

            //命名限制 暂时关闭
            if (userInfo.IsOwnerUser)
            {
                string DepartDestName = ""; //出发地
                string ArriveDestName = ""; //目的地
                var depart = DictionaryBiz.GetEnumsBy(Enums.OutCityEnum).Where(a => a.Key == model.DepartDest).FirstOrDefault();
                if (depart != null)
                {
                    DepartDestName = depart.Value;
                }
                var destModel = destBiz.GetByStr(model.ArriveDest);
                if (destModel != null)
                {
                    ArriveDestName = destModel.Name;
                }
                string BrandName = DictionaryBiz.GetCachedBrand(model.BrandCode).Name;
                model.LineName = string.Format("{0}{1}{2}{3}直飞", DepartDestName, ArriveDestName, model.Night + "晚" + model.TravelDays + "天", "[" + BrandName + "]");
            }
        }

        #endregion 新增提交

        #endregion 新增线路

        #region 复制线路

        /// <summary>
        /// 复制线路
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public string CopyLine(AddLineVModel vModel, CrmAccountModel userInfo)
        {
            var list = GetLineByName(vModel.TpLine.LineName, userInfo.OwnerCode);
            if (list != null && list.Count > 0)
                return "";

            var model = vModel.TpLine;
            var sourceLineId = model.LineId;                //被Copy的线路Id
            var modifiedTime = DateTime.Now;

            #region 逻辑处理

            //SetNewLineDefaultValue(model, modifiedTime);
            var copyLine = GetLineById(sourceLineId);
            model.Id = 0;
            model.LineId = "L" + DBTools.GetLineSeqNo();
            model.LineState = 2; // 默认下线
            model.IsValid = copyLine.IsValid;
            model.IsSelfGroup = copyLine.IsSelfGroup;
            model.CustomerCode = userInfo.CustomerCode;
            model.CustomerName = userInfo.CustomerName;
            model.CreatedBy = userInfo.Code;
            model.CreatedTime = modifiedTime;
            model.ModifiedBy = userInfo.Code;
            model.ModifiedTime = modifiedTime;
            model.OwnerCode = userInfo.OwnerCode;

            var routeList = new List<TpLineRouteModel>();             //行程
            if (model.TravelDays == copyLine.TravelDays)
            {
                routeList = routBiz.GetRouteListByLineId(sourceLineId);
            }
            else
            {
                for (var i = 1; i <= model.TravelDays; i++)
                {
                    routeList.Add(new TpLineRouteModel
                    {
                        Days = i,
                        ModifiedBy = userInfo.Code,
                        ModifiedTime = modifiedTime
                    });
                }
            }

            var lineBusPoints = lineBusPointBiz.GetBusPointsByLineId(sourceLineId); //上车点

            // 默认将创建者作为线路专管员
            var lineAdmin = new TpLineAdminModel
            {
                AccountCode = userInfo.Code,
                IsPrimary = 1,
                Department = (userInfo.CustomerCode == userInfo.OwnerCode) ? 1 : 0
            };

            #endregion 逻辑处理


            #region 执行

            using (var scope = new TransactionScope())
            {
                _lineDao.Insert(model);
                foreach (var route in routeList)
                {
                    route.Id = 0;
                    route.LineId = model.LineId;
                    route.ModifiedBy = userInfo.Code;
                    route.ModifiedTime = modifiedTime;
                    routeDao.Insert(route);
                }
                if (lineBusPoints != null && lineBusPoints.Count > 0)
                {
                    foreach (var item in lineBusPoints)
                    {
                        item.Id = 0;
                        item.LineId = model.LineId;
                        item.ModifiedBy = userInfo.Code;
                        item.ModifiedTime = modifiedTime;
                        lineBusPointDao.Insert(item);
                    }
                }
                lineAdmin.LineId = model.LineId;
                lineAdminDao.Insert(lineAdmin);

                //添加默认的套餐.
                var packageModel = new TpLineSuiteModel();
                packageModel.LineId = model.LineId;
                packageModel.PackageDescr = "默认套餐";
                packageDao.Insert(packageModel);

                scope.Complete();
            }

            #endregion 执行

            return model.LineId;
        }

        #endregion 复制线路

        #region 线路编辑

        /// <summary>
        /// 保存更新
        /// </summary>
        /// <param name="vModel"></param>
        public void EditLine(AddLineVModel vModel, CrmAccountModel userInfo)
        {
            var modifiedTime = DateTime.Now;
            var model = vModel.TpLine;
            var entity = GetLineById(model.LineId);

            // 线路专管员
            //var lineAdmin = lineAdminBiz.GetByLineId(model.Id) ?? new TpLineAdminModel();
            //if (lineAdmin.AccountCode != vModel.LineAdmin.AccountCode)
            //{
            //    lineAdmin.AccountCode = vModel.LineAdmin.AccountCode;
            //}
            //else
            //{
            //    lineAdmin = null;
            //}

            #region 将页面的值赋予entity

            entity.LineNamePostfix = model.LineNamePostfix;
            entity.CustomerCode = model.CustomerCode;
            entity.CustomerName = model.CustomerName;
            entity.MoveUpDays = model.MoveUpDays;
            entity.LineType = model.LineType;
            entity.LineScope = model.LineScope;
            entity.TrafficType = model.TrafficType;
            entity.AirlineCode = model.AirlineCode;
            entity.DepartDest = model.DepartDest;
            entity.ArriveDest = model.ArriveDest;
            entity.YingJiPhone = model.YingJiPhone;
            entity.LineSpecial = model.LineSpecial;
            //entity.FootNotes = model.FootNotes;
            //entity.Shopping = model.Shopping;
            //entity.OpDesc = model.OpDesc;
            entity.IsSelfGroup = model.IsSelfGroup;
            entity.MutliDest = (vModel.SelectedMutliDest == null) ? "" : vModel.SelectedMutliDest.Join(",");
            entity.TeamID = model.TeamID;
            //entity.LineState = 2;//默认编辑后将线路作下线处理 20130104
            entity.BrandCode = model.BrandCode;//品牌
            entity.Night = model.Night;
            entity.Themes = (vModel.ThemeIds == null) ? "" : vModel.ThemeIds.Join("|");

            if (vModel.LockName == 1) // 线路名锁定
            {
                string DepartDestName = ""; //出发地
                string ArriveDestName = ""; //目的地
                var depart = DictionaryBiz.GetEnumsBy(Enums.OutCityEnum).Where(a => a.Key == model.DepartDest).FirstOrDefault();
                if (depart != null)
                {
                    DepartDestName = depart.Value;
                }
                var destModel = destBiz.GetByStr(model.ArriveDest);
                if (destModel != null)
                {
                    ArriveDestName = destModel.Name;
                }
                string BrandName = "";
                if (!model.BrandCode.IsNullOrEmpty())
                {
                    var brandModel = DictionaryBiz.GetCachedBrand(model.BrandCode);
                    if (brandModel != null)
                    {
                        BrandName = "[" + brandModel.Name + "]";
                    }
                }

                model.LineName = string.Format("{0}{1}{2}{3}{4}{5}", DepartDestName, ArriveDestName, model.Night + "晚" + model.TravelDays + "天", BrandName, model.AirlineName, model.LineNamePostfix);
            }

            if (entity.LineName != model.LineName)
            {
                entity.LineName = model.LineName;
            }

            #endregion 将页面的值赋予entity

            var oldRoutes = new TpLineRouteBiz().GetRouteListByLineId(entity.LineId);
            var routes = new List<TpLineRouteModel>();
            if (entity.TravelDays < model.TravelDays)  // 修改了天数
            {
                for (var i = entity.TravelDays + 1; i <= model.TravelDays; i++)
                {
                    routes.Add(new TpLineRouteModel
                    {
                        LineId = entity.LineId,
                        Days = i,
                        ModifiedBy = userInfo.Code,
                        ModifiedTime = modifiedTime
                    });
                }
            }

            using (var scope = new TransactionScope())
            {
                // 删除减少的天数
                if (entity.TravelDays > model.TravelDays)
                {
                    for (var i = model.TravelDays + 1; i <= entity.TravelDays; i++)
                    {
                        // 先删除关联
                        _trafficDao.Delete("WHERE LineRouteId IN (SELECT Id FROM TpLineRoute WHERE LineId=@0 AND Days=@1)", entity.LineId, i);
                       
                        routeDao.Delete(@"WHERE LineId=@0 and Days=@1", entity.LineId, i);
                    }
                }

                entity.TravelDays = model.TravelDays;
                _lineDao.Update(entity);

                // 天数变化将清除所有行程  //TODO
                if (routes.Count > 0)
                {
                    //routeDao.Delete(@"WHERE LineId=@0", entity.Id);
                    //oldRoutes.ForEach(p => routeDao.Delete(p));
                    routes.ForEach(p => routeDao.Insert(p));
                }

                //if (lineAdmin != null)
                //{
                //    //针对旧数据处理
                //    if (lineAdmin.Id == 0)
                //    {
                //        lineAdmin.LineId = entity.Id;
                //        lineAdminDao.Insert(lineAdmin);
                //    }
                //    else
                //    {
                //        lineAdminDao.Update(lineAdmin);
                //    }
                //}

                scope.Complete();
            }
        }

        #endregion 线路编辑

        /// <summary>
        /// 更新线路
        /// </summary>
        /// <param name="model">线路实体</param>
        /// <returns>受影响的行数</returns>
        public int UpdateLine(TpLineModel model)
        {
            return _lineDao.Update(model);
        }

        public int UpdateLineState(string lineId, int state)
        {
            var sql = new Sql();
            sql.Append(" UPDATE TpLine SET LineState=@1 WHERE LineId=@0 ", lineId, state);

            return _lineDao.Execute(sql.SQL, sql.Arguments);
        }

        public int UpdateImportState(TpLineModel model)
        {
            var sql = new Sql();
            sql.Append(" UPDATE TpLine SET ImportState=@0 ", model.ImportState);
            //if (model.ImportState == 4)
               // sql.Append(" ,LineState=3 ");  // 上线
            //else 
            if (model.ImportState == 2)
                sql.Append(" ,LineState=2 ");  // 下线

            sql.Append(" WHERE LineId=@0 ", model.LineId);
            return _lineDao.Execute(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据线路名称获取线路
        /// </summary>
        /// <param name="lineName"></param>
        /// <returns></returns>
        public List<TpLineModel> GetLineByName(string lineName, string ownerCode)
        {
            return _lineDao.Fetch("SELECT LineId FROM TpLine WHERE IsValid=1 AND LineName=@0 AND OwnerCode=@1 ", lineName, ownerCode);
        }

        public TpLineModel CheckLineName(string name, string lineId)
        {
            Sql sql = new Sql();
            sql.Append("SELECT * FROM TpLine WHERE LineName=@0 AND IsValid=1 ", Ansi(name));

            if (!string.IsNullOrEmpty(lineId))
            {
                sql.Append(" AND LineId<>@0", lineId);
            }

            return _lineDao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据产品类型获取产品Id 供订单列表以线路类型查询订单
        /// </summary>
        /// <param name="lineType">线路类型</param>
        /// <returns></returns>
        public List<TpLineModel> GetIdsByLineType(string lineType, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(@" select DISTINCT LineId from TpLine ")
                .Append(@" where IsValid=1 and OwnerCode=@0 ", userInfo.OwnerCode);
            if (!lineType.IsNullOrEmpty())
                sql.Append(@" AND LineType=@0 ", Ansi(lineType));
            return _lineDao.Query<TpLineModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 获取【巴士】所有的线路Id
        /// </summary>
        /// <returns></returns>
        public List<TpLineModel> GetBusLineIds(string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" select DISTINCT LineId from TpLine ")
                .Append(@" where IsValid=1 and OwnerCode=@0 and TrafficType=1 ", ownerCode);
            return _lineDao.Query<TpLineModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 根据多个产品类型获取产品Id供导出接送单导出对应线路类型的订单
        /// </summary>
        /// <param name="lineTypeIds"></param>
        /// <returns></returns>
        public List<TpLineModel> GetIdsByLineTypes(string lineTypeIds, CrmAccountModel userInfo)
        {
            var sql = new Sql();
            sql.Append(" select DISTINCT LineId from TpLine ")
                .Append(@" where IsValid=1 and OwnerCode=@0 ", userInfo.OwnerCode);
            if (userInfo.OwnerCode != userInfo.CustomerCode)
                sql.Append(@" AND CustomerCode=@0", Ansi(userInfo.CustomerCode));
            if (!lineTypeIds.IsNullOrEmpty())
                sql.Append(@" AND LineType in ( " + lineTypeIds + " ) ");
            return _lineDao.Query<TpLineModel>(sql.SQL, sql.Arguments).ToList();
        }

        #region 线路附件相关

        public List<TpLineFileModel> GetLineFileList(string lineId)
        {
            var sql = new Sql();
            sql.Append(" SELECT * from TpLineFiles where LineId=@0 and IsDel=0 ", lineId);

            return _fileDao.Query(sql.SQL, sql.Arguments).ToList();
        }

        public object AddLineFile(TpLineFileModel model)
        {
            return _fileDao.Insert(model);
        }

        public int UpdateLineFile(TpLineFileModel model)
        {
            return _fileDao.Update(model);
        }

        /// <summary>
        /// 取得线路附件
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TpLineFileModel GetLineFileModel(int Id)
        {
            var sql = new Sql();
            sql.Append(" select * from TpLineFiles where Id=@0 ", Id);

            return _fileDao.Query(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        /// <summary>
        /// 取得线路附件列表
        /// </summary>
        /// <param name="LineId"></param>
        /// <returns></returns>
        public TpLineFileModel GetLineFileModelByLineId(int LineId)
        {
            var sql = new Sql();
            sql.Append(" select * from TpLineFiles where LineId=@0 and IsDel=0 ", LineId);

            return _fileDao.Query(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        /// <summary>
        /// 删除线路附件
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public int DeleteLineFile(int Id)
        {
            var sql = new Sql();
            sql.Append("  update TpLineFiles set IsDel=1, DeleteTime=now() where Id=@0 ", Id);

            return _fileDao.Execute(sql.SQL, sql.Arguments);
        }

        public int SetPrimaryPic(string lineId, string path)
        {
            var sql = new Sql();
            sql.Append(" update TpLine set LogoPath=@1 where LineId=@0 ", lineId, path);

            return _lineDao.Execute(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 查询城市图片
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public PagedList<PhotoInfoModel> SearchCityImages(string parentStr, int pagedIndex)
        {
            var photosList = _photoBiz.SearchCityImages(parentStr, pagedIndex);
            foreach (var photomodel in photosList.Items)
            {
                SearchImageWidthAndHeight(photomodel);
            }
            return photosList;
        }

        /// <summary>
        /// 获取图片的大小
        /// </summary>
        /// <param name="model"></param>
        private void SearchImageWidthAndHeight(PhotoInfoModel model)
        {
            try
            {
                if (!model.Url.IsNullOrEmpty())
                {
                    System.Net.HttpWebRequest hwreq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(model.Url);
                    System.Net.HttpWebResponse hwrep1 = (System.Net.HttpWebResponse)hwreq.GetResponse();
                    System.Drawing.Image originalImage = System.Drawing.Image.FromStream(hwrep1.GetResponseStream());
                    model.PhotoWidth = originalImage.Width;
                    model.PhotoHeight = originalImage.Height;
                }
            }
            catch (Exception err)
            {
                logger.Error("", err);
            }
        }

        #endregion 线路附件相关

        public List<KeyValueBean> GetLineListBean(string ownerCode)
        {
            var sql = new Sql();
            //查询上线的线路信息.
            sql.Append(@" SELECT LineId as `Key`, LineName as [Value] FROM TpLine WHERE IsValid=1 AND OwnerCode=@0 and LineState=3 ", Ansi(ownerCode));

            return _lineDao.Query<KeyValueBean>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 保存预订须知
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int SaveYuDingXuZhi(AddLineVModel model)
        {
            var entity = GetLineById(model.TpLine.LineId);
            entity.BookingNodes = model.TpLine.BookingNodes;
            entity.PriceContain = model.TpLine.PriceContain;
            entity.PriceNoContain = model.TpLine.PriceNoContain;
            entity.VisaNote = model.TpLine.VisaNote;
            return _lineDao.Update(entity);
        }

        public int SaveLineDesc(AddLineVModel model)
        {
            return _lineDao.Update(" SET LineDesc=@1 WHERE LineId=@0 ", model.TpLine.LineId, model.TpLine.LineDesc);
        }

    }
}