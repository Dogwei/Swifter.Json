using Swifter.Benchmarks.Formatters;
using Swifter.Benchmarks.Tests;
using Swifter.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Swifter.Benchmarks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string col_Time = "Time";
        const string col_Name = "Name";
        const string col_Mode = "Mode";

        static readonly Regex CellRegex = new Regex(@"^(?<Time>[0-9\.]+)\((?<Unit>[a-zA-z_0-9]+)\)\*(?<Number>[0-9]+)$");

        const int millisecondsTimeout = 15000;
        const int maxNumber = 8000000;
        const int minNumber = 3;

        readonly List<GOneInfo> formatters;
        readonly List<GOneInfo> tests;
        readonly List<DataGridRow> dg_result_rows;
        readonly DataTable dt_result;
        readonly DispatcherTimer timer;
        readonly ConcurrentQueue<Action> actions;

        Task task_test;

        void AddFormatter<TMeta>(IFormatter<TMeta> formatter)
        {
            formatters.Add(new GOneInfo { Instance = formatter, Type = typeof(IFormatter<>), GenericType = typeof(TMeta) });
        }

        void AddTest<TData>(ITest<TData> test)
        {
            tests.Add(new GOneInfo { Instance = test, Type = typeof(ITest<>), GenericType = typeof(TData) });
        }

        void AddFormatterName<TMeta>(IFormatter<TMeta> formatter)
        {
            dt_result.Columns.Add(formatter.FormatterName, typeof(string));
        }

        void Test<TData>(ITest<TData> test)
        {
            var r_deser = dt_result.NewRow();
            var r_ser = dt_result.NewRow();

            var t_ser = Task.Run(ser);
            var t_deser = Task.Run(deser);

            void deser()
            {
                AddToResult(r_deser);

                SetCellValue(r_deser, col_Time, DateTime.Now.ToString("HH:mm:ss"));
                SetCellValue(r_deser, col_Name, test.Name);
                SetCellValue(r_deser, col_Mode, nameof(deser));

                EachFormatters(test_foramtter);

                void test_foramtter<T>(IFormatter<T> formatter)
                {
                    string result = "Failed";

                    try
                    {
                        var meta = formatter.ConvertFromJson(test.GetJson());

                        var firstRun = Task.Run(() =>
                        {
                            var data = formatter.Deser<TData>(meta);

                            test.VerifyData(data);
                        });

                        if (!firstRun.Wait(millisecondsTimeout))
                        {
                            throw new TimeoutException("Timeout");
                        }

                        formatter.Deser<TData>(meta);

                        var stopwatch = Stopwatch.StartNew();

                        formatter.Deser<TData>(meta);

                        var ms = stopwatch.ElapsedTicks;

                        var times = Math.Max(maxNumber / Math.Max(ms, 1), minNumber);

                        stopwatch.Restart();

                        for (int i = 0; i < times; i++)
                        {
                            formatter.Deser<TData>(meta);
                        }

                        var elapsed = stopwatch.ElapsedTicks / (double)times;

                        if (elapsed > 10000)
                        {
                            result = $"{(elapsed / TimeSpan.TicksPerMillisecond).ToString("0.000")}(ms)*{times}";
                        }
                        else
                        {
                            result = $"{elapsed.ToString("0.000")}(ticks)*{times}";
                        }
                    }
                    catch (TimeoutException)
                    {
                        result = "Timeout";
                    }
                    catch (Exception)
                    {
                        result = "Exception";
                    }
                    finally
                    {
                        SetCellValue(r_deser, formatter.FormatterName, result);
                    }
                }
            }

            void ser()
            {
                AddToResult(r_ser);

                SetCellValue(r_ser, col_Time, DateTime.Now.ToString("HH:mm:ss"));
                SetCellValue(r_ser, col_Name, test.Name);
                SetCellValue(r_ser, col_Mode, nameof(ser));

                EachFormatters(test_foramtter);

                void test_foramtter<T>(IFormatter<T> formatter)
                {
                    string result = "Failed";

                    try
                    {
                        var data = test.GetData();

                        var firstRun = Task.Run(() =>
                        {
                            var meta = formatter.Ser(data);

                            test.VerifyJson(formatter.ToJsonString(meta));
                        });

                        if (!firstRun.Wait(millisecondsTimeout))
                        {
                            throw new TimeoutException("Timeout");
                        }

                        formatter.Ser(data);

                        var stopwatch = Stopwatch.StartNew();

                        formatter.Ser(data);

                        var ms = stopwatch.ElapsedTicks;

                        var times = Math.Max(maxNumber / Math.Max(ms, 1), minNumber);

                        stopwatch.Restart();

                        for (int i = 0; i < times; i++)
                        {
                            formatter.Ser(data);
                        }

                        var elapsed = stopwatch.ElapsedTicks / (double)times;

                        if (elapsed > 10000)
                        {
                            result = $"{(elapsed / TimeSpan.TicksPerMillisecond).ToString("0.000")}(ms)*{times}";
                        }
                        else
                        {
                            result = $"{elapsed.ToString("0.000")}(ticks)*{times}";
                        }
                    }
                    catch (TimeoutException)
                    {
                        result = "Timeout";
                    }
                    catch (Exception)
                    {
                        result = "Exception";
                    }
                    finally
                    {
                        SetCellValue(r_ser, formatter.FormatterName, result);
                    }
                }
            }

            Task.WaitAll(t_ser, t_deser);
        }

        private void AddToResult(DataRow row)
        {
            actions.Enqueue(() =>
            {
                dt_result.Rows.Add(row);
            });
        }

        private void SetCellValue(DataRow row, string name, object value)
        {
            actions.Enqueue(() =>
            {
                row[name] = value;

                switch (name)
                {
                    case col_Name:
                    case col_Mode:
                    case col_Time:
                        return;
                }

                var dg_name_column = dg_result.Columns[dt_result.Columns.IndexOf(col_Name)];
                var dg_mode_column = dg_result.Columns[dt_result.Columns.IndexOf(col_Mode)];
                var dg_time_column = dg_result.Columns[dt_result.Columns.IndexOf(col_Time)];

                var dg_row = dg_result_rows.FirstOrDefault(item =>
                    dg_name_column.GetCellContent(item) is TextBlock txt_name && !string.IsNullOrEmpty(txt_name.Text) && Equals(txt_name.Text, Convert.ToString(row[col_Name])) &&
                    dg_mode_column.GetCellContent(item) is TextBlock txt_mode && !string.IsNullOrEmpty(txt_mode.Text) && Equals(txt_mode.Text, Convert.ToString(row[col_Mode])) &&
                    dg_time_column.GetCellContent(item) is TextBlock txt_time && !string.IsNullOrEmpty(txt_time.Text) && Equals(txt_time.Text, Convert.ToString(row[col_Time])));

                if (dg_row != null)
                {
                    var dg_current_column = dg_result.Columns[dt_result.Columns.IndexOf(name)];

                    if (dg_current_column.GetCellContent(dg_row) is TextBlock txt_current && txt_current.Parent is DataGridCell cell_current)
                    {
                        txt_current.Text = Convert.ToString(value);

                        var cells = new List<(string, DataGridCell)>();

                        foreach (DataColumn item in dt_result.Columns)
                        {
                            switch (item.ColumnName)
                            {
                                case col_Name:
                                case col_Mode:
                                case col_Time:
                                    continue;
                            }

                            if (dg_result.Columns[item.Ordinal].GetCellContent(dg_row) is TextBlock txt_item && !string.IsNullOrEmpty(txt_item.Text) && txt_item.Parent is DataGridCell cell_item)
                            {
                                cells.Add((txt_item.Text, cell_item));
                            }
                        }

                        UpdateCells(cells);
                    }
                }
            });
        }

        private void UpdateCells(List<(string value, DataGridCell cell)> cells)
        {
            double min = int.MaxValue;
            double max = int.MinValue;

            var color_error = Color.FromRgb(255, 0, 0);

            for (int i = 0; i < cells.Count; i++)
            {

                var item = cells[i];

                var match = CellRegex.Match(item.value);

                if (match.Success)
                {
                    var value = GetTime(match);

                    min = Math.Min(min, value);
                    max = Math.Max(max, value);

                    cells[i] = (value.ToString(), item.cell);
                }
                else
                {
                    item.cell.Background = new SolidColorBrush(color_error);

                    cells[i] = default;
                }
            }

            max = Math.Max(min * 3, 5);

            var color_start = Color.FromRgb(0x30, 0xFF, 0x30);
            var color_end = Color.FromRgb(0xFF, 0xFF, 0x00);

            foreach (var (value, cell) in cells)
            {
                if (value != null && cell != null)
                {
                    var val = Convert.ToDouble(value);

                    var muly = (Math.Min(max, val) - min) / (max - min);

                    cell.Background = new SolidColorBrush(color_start + ((color_end - color_start) * (float)muly));
                }
            }

            static double GetTime(Match match)
            {
                var unit = match.Groups["Unit"].Value;
                var time = Convert.ToDouble(match.Groups["Time"].Value);

                switch (unit)
                {
                    case "ms":
                        return time * TimeSpan.TicksPerMillisecond;
                    case "ticks":
                        return time;
                    default:
                        throw new NotSupportedException($"unit : {unit}");
                }
            }
        }

        void EachFormatters(Action<IFormatter<object>> action)
        {
            var methodInfo = action.Method.GetGenericMethodDefinition();

            foreach (var item in formatters)
            {
                methodInfo.MakeGenericMethod(item.GenericType).Invoke(action.Target, new object[] { item.Instance });
            }
        }

        void EachTests(Action<ITest<object>> action)
        {
            var methodInfo = action.Method.GetGenericMethodDefinition();

            foreach (var item in tests)
            {
                methodInfo.MakeGenericMethod(item.GenericType).Invoke(action.Target, new object[] { item.Instance });
            }
        }

        public MainWindow()
        {
            formatters = new List<GOneInfo>();
            tests = new List<GOneInfo>();
            dt_result = new DataTable();
            dg_result_rows = new List<DataGridRow>();

            timer = new DispatcherTimer();

            actions = new ConcurrentQueue<Action>();

            AddFormatter(new NewtonsoftFormatter());
            AddFormatter(new LitJsonFormatter());
            AddFormatter(new ServiceStackFormatter());
            AddFormatter(new FastJSONFormatter());
            AddFormatter(new NetJSONFormatter());
            AddFormatter(new JilFormatter());
            AddFormatter(new Utf8JsonUtf8Formatter());
            AddFormatter(new Utf8JsonUtf16Formatter());
            AddFormatter(new SpanJsonUtf8Formatter());
            AddFormatter(new SpanJsonUtf16Formatter());
            AddFormatter(new SystemTextJsonUtf16Formatter());
            AddFormatter(new SystemTextJsonUtf8Formatter());
            AddFormatter(new SwifterFormatter());

            //AddTest(new CommonModeText());
            //AddTest(new ShortStringTest());
            //AddTest(new LongStringTest());
            //AddTest(new BooleanArrayTest());
            //AddTest(new Int32ArrayTest());
            //AddTest(new Int64ArrayTest());
            //AddTest(new UInt32ArrayTest());
            //AddTest(new UInt64ArrayTest());
            //AddTest(new SingleArrayTest());
            //AddTest(new DoubleArrayTest());
            //AddTest(new ByteArrayTest());
            //AddTest(new CharArrayTest());
            AddTest(new DictionaryTest());

            InitializeComponent();

            dt_result.Columns.Add(col_Time, typeof(string));
            dt_result.Columns.Add(col_Name, typeof(string));
            dt_result.Columns.Add(col_Mode, typeof(string));

            EachFormatters(AddFormatterName);

            dg_result.DataContext = dt_result;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(100);

            timer.Start();

            dg_result.LoadingRow += Dg_result_LoadingRow;
        }

        private void Dg_result_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            dg_result_rows.Add(e.Row);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            while (actions.TryDequeue(out var action))
            {
                action();
            }
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (task_test!=null)
            {
                MessageBox.Show("Test is running.");

                return;
            }

            task_test = Task.Run(() =>
            {
                EachTests(Test);

                task_test = null;
            });
        }

        private void Marge(object sender, RoutedEventArgs e)
        {

        }
    }
}