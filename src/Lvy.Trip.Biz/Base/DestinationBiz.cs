using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Crm;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Base
{
    /// <summary>
    /// 目的地
    /// </summary>
    public class DestinationBiz : BaseBiz
    {
        private static readonly DestinationDao _dao = new DestinationDao();

        /// <summary>
        /// 获取所有的目的地
        /// </summary>
        /// <returns></returns>
        public static List<BaseDestinationModel> GetDests()
        {
            return _dao.GetDests();
        }


        /// <summary>
        /// 验证国家是否匹配存在
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public int ValidateCountryData(string code, string name)
        {
            Sql sql = new Sql();
            sql.Append("select * from BaseDestination where ParentStr=@0 and Name=@1 ", code, name);
            var entity = _dao.FirstOrDefault(sql.SQL, sql.Arguments);
            return (entity == null ? 0 : 1);
        }

        public List<BaseDestinationModel> GetAllDestination()
        {
            Sql sql = new Sql();
            sql.Append(@"select t.*, b.`Value` as LevelName from BaseDestination t
left join BaseDictionaryDetail b on CONVERT(b.`Key`,SIGNED) = t.`Level` and  b.name = 'DestLevelEnum' and b.IsValid = 1
 where t.IsValid=1 order by t.`Level` ");

            return _dao.Fetch(sql.SQL);
        }

        /// <summary>
        ///  取得所有有效省份
        /// </summary>
        /// <returns></returns>
        public List<BaseDestinationModel> GetProvinces()
        {
            string sql = "select * from BaseDestination where Level=10 and IsValid=1 ";
            return _dao.Fetch(sql);
        }

        /// <summary>
        /// 验证目的地是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public BaseDestinationModel CheckDesName(string name, int id, int parentId)
        {
            var sql = new Sql();
            sql.Append(@" select * from BaseDestination where ParentId=@0 and IsValid=1 and Name=@1 ", parentId, Ansi(name));
            if (id != default(int))
                sql.Append(" and Id<>@0 ", id);
            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        public int UpdateData()
        {
            int row = 0;
            var list = GetAllDestination();

            foreach (var item in list)
            {
                bool modify = false;

                // 检查 拼音 简拼  和 ParentStr
                //var pinyin = item.Name.ConvertPinYin();
                //if (pinyin != item.PinYin)
                //{
                //    modify = true;
                //    item.PinYin = pinyin;
                //}
                //var jpinyn = pinyin.ConvertJPinYin();
                //if (jpinyn != item.JPinYin)
                //{
                //    modify = true;
                //    item.JPinYin = jpinyn;
                //}

                if (item.ParentId != default(int))
                {
                    var parent = list.Where(t => t.Id == item.ParentId).FirstOrDefault();
                    var parentstr = parent.ParentStr + item.Id + "/";
                    if (item.ParentStr != parentstr)
                    {
                        item.ParentStr = parentstr;
                        item.ParentName = parent.Name;
                        modify = true;
                    }
                }
                else
                {
                    var parentstr = "/" + item.Id + "/";
                    if (item.ParentStr != parentstr)
                    {
                        item.ParentStr = parentstr;
                        modify = true;
                    }
                }

                if (modify)
                {
                    _dao.Update(item);
                    row++;
                }
            }

            return row;
        }

        /// <summary>
        /// 根据主键获取一条目的地
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseDestinationModel GetById(int id)
        {
            return _dao.GetById(id);
        }

        public BaseDestinationModel GetByStr(string id)
        {
            var sql = new Sql();
            sql.Append(@" select * from BaseDestination where IsValid=1 and ParentStr=@0 ", Ansi(id));
            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 添加目的地
        /// </summary>
        /// <returns>key id</returns>
        public int Add(BaseDestinationModel model)
        {
            var id = _dao.Insert(model).ToInt();
            if (model.ParentId != default(int))
            {
                var parent = _dao.GetById(model.ParentId);
                if (parent != null)
                {
                    model.ParentStr = parent.ParentStr + id + "/";
                    model.ParentName = parent.Name;
                }
            }
            else
                model.ParentStr = "/" + id + "/";

            _dao.Update(model);
            return id;
        }

        /// <summary>
        /// 更新目的地
        /// </summary>
        /// <returns></returns>
        public int Update(BaseDestinationModel model)
        {
            if (model.ParentId != default(int))
            {
                var parent = _dao.GetById(model.ParentId);
                if (parent != null)
                {
                    model.ParentStr = parent.ParentStr + model.Id + "/";
                    model.ParentName = parent.Name;
                }
            }
            else
                model.ParentStr = "/" + model.Id + "/";

            return _dao.Update(model);
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SetValidStateByDest(int id)
        {
            var obj = GetById(id);
            obj.IsValid = obj.IsValid == 0 ? 1 : 0;
            return _dao.Update(obj);
        }
    }
}