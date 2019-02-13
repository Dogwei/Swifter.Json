using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Swifter.Test
{
    public static class Helper
    {
        public static DataRow DataTableNewRow(this Control control, DataTable dataTable)
        {
            Func<DataTable, DataRow> method = (table) =>
            {
                return table.NewRow();
            };

            return (DataRow)control.Invoke(method, dataTable);
        }

        public static void SetCellValue(this Control control, DataRow dataRow, string cellName, object value)
        {
            Action<DataRow, string, object> method = (row, name, val) =>
            {
                row[name] = val;
            };

            control.Invoke(method, dataRow, cellName, value);
        }

        public static void DataTableAddRow(this Control control, DataTable dataTable, DataRow dataRow)
        {
            Action<DataTable, DataRow> method = (table, row) =>
            {
                table.Rows.Add(row);
            };

            control.Invoke(method, dataTable, dataRow);
        }
    }
}
