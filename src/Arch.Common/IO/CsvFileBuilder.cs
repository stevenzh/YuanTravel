using System;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Arch.Common.IO
{
    public class CsvFileBuilder
    {
        public byte[] AsBytes(IEnumerable modelList)
        {
            var models = modelList as IList;

            StringBuilder sb = new StringBuilder();
            BuildHeaders(models, sb);
            BuildRows(models, sb);

            return sb.AsBytes();
        }

        public string AsString(IEnumerable modelList)
        {
            var models = modelList as IList;

            StringBuilder sb = new StringBuilder();
            BuildHeaders(models, sb);
            BuildRows(models, sb);

            return sb.ToString();
        }


        private void BuildRows(IList modelList, StringBuilder sb)
        {
            foreach (object modelItem in modelList)
            {
                BuildRowData(modelList, modelItem, sb);
                sb.NewLine();
            }
        }

        private void BuildRowData(IList modelList, object modelItem, StringBuilder sb)
        {
            foreach (PropertyInfo info in modelList[0].GetType().GetProperties())
            {
                object value = info.GetValue(modelItem, new object[0]);
                sb.AppendFormat("{0},", value);
            }
        }

        private void BuildHeaders(IList modelList, StringBuilder sb)
        {
            foreach (PropertyInfo property in modelList[0].GetType().GetProperties())
            {
                sb.AppendFormat("{0},", property.Name);
            }
            sb.NewLine();
        }
    }



}
