using Swifter.Test.WPF.Serializers;
using Swifter.Test.WPF.Tests;
using Swifter.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Swifter.Test.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int MillisecondsTimeout = 100 * 1000;

        public const string TestName = "Test Name";
        public const string MethodName = "Method";
        public const string SerValue = "ser";
        public const string DeserValue = "deser";
        public const string RuningValue = "runing";

        readonly DataTable results;
        readonly DataView results_view;
        readonly ISerializer[] serializers;
        readonly ITest[] tests;

        DataGridColumn dgCurrentCellColumn;
        object dgCurrentCellItem;

        public static IEnumerable<ISerializer> GetSerializers()
        {
            foreach (var type in typeof(ISerializer).Assembly.GetTypes())
            {
                if (typeof(ISerializer).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                {
                    var instance = (ISerializer)Activator.CreateInstance(type);

                    var baseSerializerType = typeof(BaseSerializer<>).MakeGenericType(instance.SymbolsType);

                    if (baseSerializerType.IsInstanceOfType(instance))
                    {
                        yield return instance;
                    }
                }
            }
        }

        public static IEnumerable<ITest> GetTests()
        {
            foreach (var type in typeof(ITest).Assembly.GetTypes())
            {
                if (typeof(ITest).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                {
                    var instance = (ITest)Activator.CreateInstance(type);

                    var baseTestType = typeof(BaseTest<>).MakeGenericType(instance.ObjectType);

                    if (baseTestType.IsInstanceOfType(instance))
                    {
                        yield return instance;
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            results = new DataTable();
            results_view = results.DefaultView;

            serializers = GetSerializers().ToArray();
            tests = GetTests().ToArray();

            results.Columns.Add(TestName, typeof(string));
            results.Columns.Add(MethodName, typeof(string));

            foreach (var serializer in serializers)
            {
                results.Columns.Add(serializer.Name, typeof(object));
            }

            foreach (var test in tests)
            {
                var serRow = results.NewRow();
                var deserRow = results.NewRow();

                serRow[TestName] = test.TestName;
                serRow[MethodName] = SerValue;

                deserRow[TestName] = test.TestName;
                deserRow[MethodName] = DeserValue;

                results.Rows.Add(serRow);
                results.Rows.Add(deserRow);
            }

            dg.ItemsSource = results_view;

            results.RowChanged += Results_RowChanged;
        }

        private void Results_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            var min = double.MaxValue;
            var max = double.MinValue;

            foreach (var serializer in serializers)
            {
                if (e.Row[serializer.Name] is Result val)
                {
                    min = Math.Min(val.avg, min);
                    max = Math.Max(val.avg, max);
                }
            }

            var startColor = Color.FromRgb(0x30, 0xFF, 0x30);
            var endColor = Color.FromRgb(0xFF, 0xFF, 0x00);

            max = min * 3;

            foreach (var colummn in dg.Columns)
            {
                var cell = (DataGridCell)colummn.GetCellContent(results_view[results.Rows.IndexOf(e.Row)])?.Parent;

                if (cell != null)
                {
                    var value = e.Row[Convert.ToString(cell.Column.Header)];

                    if (value is Result result)
                    {
                        var muly = (Math.Min(max, result.avg) - min) / (max - min);

                        cell.Background = new SolidColorBrush(Color.FromRgb(
                            (byte)(((endColor.R - startColor.R) * muly) + startColor.R),
                            (byte)(((endColor.G - startColor.G) * muly) + startColor.G),
                            (byte)(((endColor.B - startColor.B) * muly) + startColor.B)
                            ));
                    }
                    else if (Equals(value, RuningValue))
                    {
                        cell.Background = Brushes.DeepSkyBlue;
                    }
                    else if (value is ExceptionResult)
                    {
                        cell.Background = Brushes.Red;
                    }
                    else
                    {
                        cell.Background = null;
                    }
                }
            }
        }

        private void NewWindow(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
        }

        private void GCCollect(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }

        private void StartSingleThread(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                foreach (var test in tests)
                {
                    foreach (var serializer in serializers)
                    {
                        Run(serializer, test, SerValue);
                    }

                    foreach (var serializer in serializers)
                    {
                        Run(serializer, test, DeserValue);
                    }
                }
            });
        }


        private void StartDoubleThread(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                foreach (var test in tests)
                {
                    foreach (var serializer in serializers)
                    {
                        Run(serializer, test, SerValue);
                    }
                }
            });

            Task.Run(() =>
            {
                foreach (var test in tests)
                {
                    foreach (var serializer in serializers)
                    {
                        Run(serializer, test, DeserValue);
                    }
                }
            });
        }


        public void Run(ISerializer serializer, ITest test, string method)
        {
            var m_RunSerialize = GetType().GetMethod(nameof(RunSerialize));
            var m_RunDeserialize = GetType().GetMethod(nameof(RunDeserialize));

            switch (method)
            {
                case SerValue:
                    m_RunSerialize.MakeGenericMethod(serializer.SymbolsType, test.ObjectType).Invoke(this, new object[] { serializer, test });
                    break;
                case DeserValue:
                    m_RunDeserialize.MakeGenericMethod(serializer.SymbolsType, test.ObjectType).Invoke(this, new object[] { serializer, test });
                    break;
            }
        }

        private void StartConcurrent(object sender, RoutedEventArgs e)
        {
            foreach (var test in tests)
            {
                Task.Run(() =>
                {
                    foreach (var serializer in serializers)
                    {
                        Run(serializer, test, SerValue);
                    }
                });

                Task.Run(() =>
                {
                    foreach (var serializer in serializers)
                    {
                        Run(serializer, test, DeserValue);
                    }
                });
            }
        }

        private void RunSelected(object sender, RoutedEventArgs e)
        {
            if (dgCurrentCellColumn != null && dgCurrentCellItem is DataRowView rowView)
            {
                var testName = rowView[TestName];
                var serializerName = dgCurrentCellColumn.Header;

                var test = tests.FirstOrDefault(item => Equals(item.TestName, testName));
                var serializer = serializers.FirstOrDefault(item => Equals(item.Name, serializerName));

                var method = Convert.ToString(rowView[MethodName]);

                if (serializer != null && test != null)
                {
                    new Thread(() =>
                    {
                        Run(serializer, test, method);
                    }).Start();
                }

                dgCurrentCellColumn = null;
                dgCurrentCellItem = null;
            }
        }

        public void RunDeserialize<TSymbols, TObject>(BaseSerializer<TSymbols> serializer, BaseTest<TObject> test)
        {
            object result = null;

            var dataRow = results.Rows.Cast<DataRow>().First(dataRow => Equals(dataRow[TestName], test.TestName) && Equals(dataRow[MethodName], DeserValue));

            Dispatcher.InvokeAsync(() =>
            {
                dataRow[serializer.Name] = RuningValue;
            });

            try
            {
                TObject obj = default;
                TSymbols symbols = default;

                double ns = default;

                var isTimeout = !Task.Run(() =>
                {
                    obj = test.GetObject();
                    symbols = serializer.Serialize(obj);

                    var stopwatch = Stopwatch.StartNew();

                    serializer.Deserialize<TObject>(symbols);

                    ns = stopwatch.ElapsedNanoseconds();

                }).Wait(MillisecondsTimeout);

                if (isTimeout)
                {
                    throw new TimeoutException();
                }

                long count = 0;
                double total = 0;
                double min = double.MaxValue;
                double max = double.MinValue;

                for (int i = 0; i < 10; i++)
                {
                    Run(1);

                    Run((int)(10 / GetAvg()));
                    Run((int)(100 / GetAvg()));
                    Run((int)(1000 / GetAvg()));
                    Run((int)(10000 / GetAvg()));
                    Run((int)(100000 / GetAvg()));
                    Run((int)(1000000 / GetAvg()));
                    Run((int)(10000000 / GetAvg()));
                    Run((int)(100000000 / GetAvg()));
                    Run((int)(500000000 / GetAvg()));
                }

                double GetAvg()
                {
                    return total / count;
                }

                void Run(int times)
                {
                    if (!(times > 0))
                    {
                        return;
                    }

                    TObject tObj = default;

                    var stopwatch = Stopwatch.StartNew();

                    for (int i = 0; i < times; i++)
                    {
                        tObj = serializer.Deserialize<TObject>(symbols);
                    }

                    var ns = stopwatch.ElapsedNanoseconds();

                    total += ns;
                    count += times;

                    min = Math.Min(min, ns / times);
                    max = Math.Max(min, ns / times);

                    if (!test.Equals(tObj, obj))
                    {
                        throw new IncorrectException();
                    }
                }

                result = new Result(GetAvg());
            }
            catch (Exception e)
            {
                result = new ExceptionResult(e);
            }
            finally
            {
                Dispatcher.InvokeAsync(() =>
                {
                    dataRow[serializer.Name] = result;
                });
            }
        }

        public void RunSerialize<TSymbols, TObject>(BaseSerializer<TSymbols> serializer, BaseTest<TObject> test)
        {
            object result = null;

            var dataRow = results.Rows.Cast<DataRow>().First(dataRow => Equals(dataRow[TestName], test.TestName) && Equals(dataRow[MethodName], SerValue));

            Dispatcher.InvokeAsync(() =>
            {
                dataRow[serializer.Name] = RuningValue;
            });

            try
            {
                TObject obj = default;
                TSymbols symbols = default;

                double ns = default;

                var isTimeout = !Task.Run(() =>
                {
                    obj = test.GetObject();
                    symbols = serializer.Serialize(obj);

                    var stopwatch = Stopwatch.StartNew();

                    serializer.Serialize(obj);

                    ns = stopwatch.ElapsedNanoseconds();


                }).Wait(MillisecondsTimeout);

                if (isTimeout)
                {
                    throw new TimeoutException();
                }

                long count = 0;
                double total = 0;
                double min = double.MaxValue;
                double max = double.MinValue;

                for (int i = 0; i < 10; i++)
                {
                    Run(1);

                    Run((int)(10 / GetAvg()));
                    Run((int)(100 / GetAvg()));
                    Run((int)(1000 / GetAvg()));
                    Run((int)(10000 / GetAvg()));
                    Run((int)(100000 / GetAvg()));
                    Run((int)(1000000 / GetAvg()));
                    Run((int)(10000000 / GetAvg()));
                    Run((int)(100000000 / GetAvg()));
                    Run((int)(500000000 / GetAvg()));
                }

                double GetAvg()
                {
                    return total / count;
                }

                void Run(int times)
                {
                    if (!(times > 0))
                    {
                        return;
                    }

                    TSymbols tSym = default;

                    var stopwatch = Stopwatch.StartNew();

                    for (int i = 0; i < times; i++)
                    {
                        tSym = serializer.Serialize(obj);
                    }

                    var ns = stopwatch.ElapsedNanoseconds();

                    total += ns;
                    count += times;

                    min = Math.Min(min, ns / times);
                    max = Math.Max(min, ns / times);

                    var tObj = serializer.Deserialize<TObject>(tSym);

                    if (!test.Equals(tObj, obj))
                    {
                        throw new IncorrectException();
                    }
                }

                result = new Result(GetAvg());
            }
            catch (Exception e)
            {
                result = new ExceptionResult(e);
            }
            finally
            {
                Dispatcher.InvokeAsync(() =>
                {
                    dataRow[serializer.Name] = result;
                });
            }
        }

        private void Dg_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dg.CurrentCell.Column != null && dg.CurrentCell.Item != null)
            {
                dgCurrentCellColumn = dg.CurrentCell.Column;
                dgCurrentCellItem = dg.CurrentCell.Item;
            }

            dg.SelectedItem = null;
        }

        private void Dg_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            dg.SelectedItem = null;
        }

        private void Dg_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            foreach (DataRow item in results.Rows)
            {
                Results_RowChanged(results, new DataRowChangeEventArgs(item, DataRowAction.Nothing));
            }
        }
    }
}