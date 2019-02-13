using Swifter.RW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Swifter.Test
{
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
            testers.Add(new NetJSONTestter());
            testers.Add(new Utf8JsonTester());
            testers.Add(new SwifterTester());

            tests.Add(new CommonDataTest(100000));
            tests.Add(new BooleanTest(100000, 100));
            tests.Add(new Int32Test(100000, 50));
            tests.Add(new Int64Test(10000, 20));
            tests.Add(new FloatTest(10000, 20));
            tests.Add(new DoubleTest(100000, 30));
            tests.Add(new DecimalTest(100000, 20));
            tests.Add(new CommonDataDictionaryTest(80000));
            tests.Add(new CalalogTest(15));
            tests.Add(new CalalogDictionaryTest(10));
            tests.Add(new CSharp7Test());
            tests.Add(new TwoDimensionaArrayTest());
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(959, 386);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.Sorted += DataGridView1_Sorted;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 386);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(959, 100);
            this.panel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(201, 16);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(168, 72);
            this.button2.TabIndex = 1;
            this.button2.Text = "Start(Thread)";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(12, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(168, 72);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start(Task)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dataGridView1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(959, 386);
            this.panel2.TabIndex = 1;
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(390, 16);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(168, 72);
            this.button3.TabIndex = 2;
            this.button3.Text = "GC Collect";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // MyForm
            // 
            this.ClientSize = new System.Drawing.Size(959, 486);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "MyForm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

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

        int task_index;

        private void button1_Click(object sender, System.EventArgs e)
        {
            Task.Run(() => StartTest($"Task {++task_index}", logTypes));
        }

        int thread_index;

        private void button2_Click(object sender, EventArgs e)
        {
            new Thread(() => StartTest($"Thread {++thread_index}", logTypes)).Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }
    }
}