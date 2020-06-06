using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Swifter.Test
{
    [Serializable]
    public partial class MyForm : Form
    {
        private const string TimeName = "Time";
        private const string CaseName = "Case";
        private const string ModeName = "Mode";
        private const string ThreadName = "Thread";

        private const string Error = "Error";
        private const string Timeout = "Timeout";
        private const string Exception = "Exception";

        private const LogTypes logTypes = LogTypes.OnlyTheNext;

        private Panel panel1;
        private DataGridView dataGridView1;
        private DataTable results;
        private Button button1;
        private List<ITester> testers;
        private List<ITestInfo> tests;
        private Button button2;
        private Button button3;
        private Button button4;
        private Panel panel2;

        public MyForm()
        {
            InitializeComponent();

            results = new DataTable();
            testers = new List<ITester>();
            tests = new List<ITestInfo>();

            testers.Add(new NewtonsoftTester());
            testers.Add(new LitJsonTester());
            testers.Add(new ServiceStackTester());
            testers.Add(new FastJsonTester());
            testers.Add(new JilTester());
            // testers.Add(new NetJSONTestter());
            testers.Add(new Utf8JsonTester());
            testers.Add(new KoobooTester());
            //testers.Add(new AutoCSerTester());
#if NETCOREAPP
            testers.Add(new System_Text_JsonTester());
            testers.Add(new SpanJsonJsonTester());
#endif

            testers.Add(new SwifterTester());

            tests.Add(new CommonDataTest(100000));
            tests.Add(new BooleanTest(100000, 100));
            tests.Add(new Int32Test(100000, 50));
            tests.Add(new Int64Test(10000, 20));
            tests.Add(new FloatTest(10000, 20));
            tests.Add(new DoubleTest(100000, 30));
            tests.Add(new DecimalTest(100000, 20));
            tests.Add(new CommonDataDictionaryTest(80000));
            tests.Add(new DataTableTest());
            tests.Add(new CalalogTest(15));
            tests.Add(new CalalogDictionaryTest(10));
            tests.Add(new CSharp7Test());
            tests.Add(new TwoDimensionaArrayTest());
            tests.Add(new ThreeDimensionalArray());
            tests.Add(new LongStringTest());
            tests.Add(new ShortStringTest());
            tests.Add(new Utf32StringTest());
            tests.Add(new DateTimeTest());
            tests.Add(new BasicTypesTest(100000));
            tests.Add(new PolymorphismTest());
            tests.Add(new StructTest());
            tests.Add(new UnsafeTest());


            results.Columns.Add(TimeName);
            results.Columns.Add(CaseName);
            results.Columns.Add(ModeName);
            results.Columns.Add(ThreadName);

            foreach (var item in testers)
            {
                results.Columns.Add(item.Name, typeof(object));
            }

            dataGridView1.DataSource = results;

            results.RowChanged += Results_RowChanged;
        }

        private void Results_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            var row = dataGridView1.Rows.Cast<DataGridViewRow>().First(item =>
                Equals(item.Cells[CaseName].Value, e.Row[CaseName]) &&
                Equals(item.Cells[ModeName].Value, e.Row[ModeName]) &&
                Equals(item.Cells[ThreadName].Value, e.Row[ThreadName])
                );

            double min = int.MaxValue;
            double max = int.MinValue;


            var startColor = Color.FromArgb(0x30, 0xFF, 0x30);
            var endColor = Color.FromArgb(0xFF, 0xFF, 0x00);

            foreach (DataGridViewCell item in row.Cells)
            {
                if (item.Value is long value)
                {
                    min = Math.Min(min, value);
                    max = Math.Max(max, value);
                }
                else if (item.Value is string stValue)
                {
                    switch (stValue)
                    {
                        case "Error":
                        case "Exception":
                        case "Timeout":
                            item.Style.BackColor = Color.FromArgb(0xff, 0x50, 0x50);
                            break;
                    }
                }
            }

            max = Math.Max(min * 3, 5);

            foreach (DataGridViewCell item in row.Cells)
            {
                if (item.Value is long value)
                {
                    var muly = (Math.Min(max, value) - min) / (max - min);

                    item.Style.BackColor = Color.FromArgb(
                        (int)((endColor.R - startColor.R) * muly) + startColor.R,
                        (int)((endColor.G - startColor.G) * muly) + startColor.G,
                        (int)((endColor.B - startColor.B) * muly) + startColor.B
                        );
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            dataGridView1.Columns[0].Width = 80;
            dataGridView1.Columns[1].Width = 120;
            dataGridView1.Columns[2].Width = 180;
            dataGridView1.Columns[3].Width = 80;

            base.OnShown(e);
        }

        public void Test<T>(string threadName, ITestInfo testInfo, LogTypes logType)
        {
            var name = testInfo.Name;
            var count = testInfo.Count;
            var text = testInfo.Text;

            var obj = default(T);

            var first = this.DataTableNewRow(results);
            var times = this.DataTableNewRow(results);

            this.SetCellValue(first, TimeName, DateTime.Now.ToString("HH:mm:ss"));
            this.SetCellValue(times, TimeName, DateTime.Now.ToString("HH:mm:ss"));
            this.SetCellValue(first, CaseName, name);
            this.SetCellValue(times, CaseName, name);
            this.SetCellValue(first, ModeName, $"deser (first)");
            this.SetCellValue(times, ModeName, $"deser (the next {count} times)");
            this.SetCellValue(first, ThreadName, threadName);
            this.SetCellValue(times, ThreadName, threadName);

            if ((logType & LogTypes.OnlyFirst) != 0)
            {
                this.DataTableAddRow(results, first);
            }

            if ((logType & LogTypes.OnlyTheNext) != 0)
            {
                this.DataTableAddRow(results, times);
            }

            foreach (var item in testers)
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();

                    obj = item.Deserialize<T>(text);

                    var time = stopwatch.ElapsedMilliseconds;

                    if (testInfo.VerDeser(obj))
                    {
                        this.SetCellValue(first, item.Name, time);
                    }
                    else
                    {
                        this.SetCellValue(first, item.Name, Error);
                    }
                }
                catch (TimeoutException)
                {
                    this.SetCellValue(first, item.Name, Timeout);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{item.Name} -- deser -- {name} : \n{e}");

                    this.SetCellValue(first, item.Name, Exception);
                }


                if ((logType & LogTypes.OnlyTheNext) != 0)
                {
                    try
                    {
                        obj = item.Deserialize<T>(text);

                        if (testInfo.VerDeser(obj))
                        {
                            var stopwatch = Stopwatch.StartNew();

                            for (int i = 0; i < count; i++)
                            {
                                item.Deserialize<T>(text);
                            }

                            var time = stopwatch.ElapsedMilliseconds;

                            this.SetCellValue(times, item.Name, time);
                        }
                        else
                        {
                            this.SetCellValue(times, item.Name, Error);
                        }
                    }
                    catch (TimeoutException)
                    {
                        this.SetCellValue(times, item.Name, Timeout);
                    }
                    catch (Exception)
                    {
                        this.SetCellValue(times, item.Name, Exception);
                    }
                }
            }

            foreach (var item in testers)
            {
                try
                {
                    obj = item.Deserialize<T>(text);

                    if (testInfo.VerDeser(obj))
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                }
            }

            first = this.DataTableNewRow(results);
            times = this.DataTableNewRow(results);

            this.SetCellValue(first, TimeName, DateTime.Now.ToString("HH:mm:ss"));
            this.SetCellValue(times, TimeName, DateTime.Now.ToString("HH:mm:ss"));
            this.SetCellValue(first, CaseName, name);
            this.SetCellValue(times, CaseName, name);
            this.SetCellValue(first, ModeName, $"ser (first)");
            this.SetCellValue(times, ModeName, $"ser (the next {count} times)");
            this.SetCellValue(first, ThreadName, threadName);
            this.SetCellValue(times, ThreadName, threadName);

            if ((logType & LogTypes.OnlyFirst) != 0)
            {
                this.DataTableAddRow(results, first);
            }

            if ((logType & LogTypes.OnlyTheNext) != 0)
            {
                this.DataTableAddRow(results, times);
            }

            foreach (var item in testers)
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();

                    var json = item.Serialize(obj);

                    var time = stopwatch.ElapsedMilliseconds;


                    if (testInfo.VerSer(json))
                    {
                        this.SetCellValue(first, item.Name, time);
                    }
                    else
                    {
                        this.SetCellValue(first, item.Name, Error);
                    }
                }
                catch (TimeoutException)
                {
                    this.SetCellValue(first, item.Name, Timeout);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{item.Name} -- ser -- {name} : \n{e}");

                    this.SetCellValue(first, item.Name, Exception);
                }


                if ((logType & LogTypes.OnlyTheNext) != 0)
                {
                    try
                    {
                        var json = item.Serialize(obj);

                        if (testInfo.VerSer(json))
                        {
                            var stopwatch = Stopwatch.StartNew();

                            for (int i = 0; i < count; i++)
                            {
                                item.Serialize(obj);
                            }

                            var time = stopwatch.ElapsedMilliseconds;

                            this.SetCellValue(times, item.Name, time);
                        }
                        else
                        {
                            this.SetCellValue(times, item.Name, Error);
                        }
                    }
                    catch (TimeoutException)
                    {
                        this.SetCellValue(times, item.Name, Timeout);
                    }
                    catch (Exception)
                    {
                        this.SetCellValue(times, item.Name, Exception);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            panel1 = new Panel();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            panel2 = new Panel();
            button4 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowTemplate.Height = 23;
            dataGridView1.Size = new Size(959, 386);
            dataGridView1.TabIndex = 0;
            dataGridView1.Sorted += DataGridView1_Sorted;
            // 
            // panel1
            // 
            panel1.Controls.Add(button4);
            panel1.Controls.Add(button3);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 386);
            panel1.Name = "panel1";
            panel1.Size = new Size(959, 100);
            panel1.TabIndex = 1;
            // 
            // button3
            // 
            button3.Font = new Font("Courier New", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button3.Location = new Point(390, 16);
            button3.Name = "button3";
            button3.Size = new Size(168, 72);
            button3.TabIndex = 2;
            button3.Text = "GC Collect";
            button3.UseVisualStyleBackColor = true;
            button3.Click += new EventHandler(button3_Click);
            // 
            // button2
            // 
            button2.Font = new Font("Courier New", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button2.Location = new Point(201, 16);
            button2.Name = "button2";
            button2.Size = new Size(168, 72);
            button2.TabIndex = 1;
            button2.Text = "Start(Thread)";
            button2.UseVisualStyleBackColor = true;
            button2.Click += new EventHandler(button2_Click);
            // 
            // button1
            // 
            button1.Font = new Font("Courier New", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button1.Location = new Point(12, 16);
            button1.Name = "button1";
            button1.Size = new Size(168, 72);
            button1.TabIndex = 0;
            button1.Text = "Start(Task)";
            button1.UseVisualStyleBackColor = true;
            button1.Click += new EventHandler(button1_Click);
            // 
            // panel2
            // 
            panel2.Controls.Add(dataGridView1);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(959, 386);
            panel2.TabIndex = 1;
            // 
            // button4
            // 
            button4.Font = new Font("Courier New", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button4.Location = new Point(581, 16);
            button4.Name = "button4";
            button4.Size = new Size(168, 72);
            button4.TabIndex = 3;
            button4.Text = "New Window";
            button4.UseVisualStyleBackColor = true;
            button4.Click += new EventHandler(button4_Click);
            // 
            // MyForm
            // 
            ClientSize = new Size(959, 486);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "MyForm";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);

        }

        private void DataGridView1_Sorted(object sender, EventArgs e)
        {
            foreach (DataRow item in results.Rows)
            {
                item[CaseName] = item[CaseName];
            }
        }

        private void StartTest(string threadName, LogTypes logType)
        {
            var testMethod = GetType().GetMethod(nameof(Test));

            foreach (var item in tests)
            {
                var method = testMethod.MakeGenericMethod(item.Type);

                method.Invoke(this, new object[] { threadName, item, logType });
            }
        }

        static int task_index;

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() => StartTest($"Task {++task_index}", logTypes));
        }

        static int thread_index;

        private void button2_Click(object sender, EventArgs e)
        {
            new Thread(() => StartTest($"Thread {++thread_index}", logTypes)).Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Application.Run(new MyForm());

            }).Start();
        }
    }
}