using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Timers;

namespace rob_control
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string[] coms = portSearch();
            foreach(string com in coms )
            {  
                if(portOn(com))
                {
                    break;
                }
            }
            
            textBox2.Text=Properties.Settings.Default.dataFolder;
            //Console.WriteLine(this.Controls.Find("label" + 3, true)[0].Text);
        }
        private string[] portSearch()
        {
            comboPort.Items.Clear();
            comboPort.Enabled = true;
            string[] coms= SerialPort.GetPortNames();
            if (coms.Length != 0)
            {
                comboPort.Items.AddRange(coms);
                comboPort.SelectedItem = coms[0];
            }
            return coms;
        }
        private bool portOn(string portName)
        {
            
            string[] coms = portSearch();
            if (coms.Contains<string>(portName))
            {
                serialPort1.PortName = portName;
                serialPort1.BaudRate = 38400;
                //Console.WriteLine(serialPort1.Encoding);
                try
                {
                    serialPort1.Open();
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();
                }
                catch
                {
                    portOff(portName);
                    return false;
                }
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
                if (serialPort1.IsOpen)
                {
                    comboPort.SelectedItem = portName;
                    COM.Text = "ON";
                    COM.ForeColor = Color.Green;
                    
                    comboPort.Enabled = false;
                    refresh_b.Enabled = false;
                    return true;
                }
            }
            return false;
        }
        private bool portOff(string portName)
        {
            
            serialPort1.Close();
            if (!serialPort1.IsOpen)
            {
                comboPort.SelectedItem = portName;
                COM.Text = "OFF";
                COM.ForeColor = Color.Red;
                comboPort.Enabled = true;
                refresh_b.Enabled = true;
                return true;
            }
            return false;
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            Thread.Sleep(10);  //（毫秒）等待一定時間，確保資料的完整性 int len        
            int len = serialPort1.BytesToRead;
            string receivedata = string.Empty;
            if (len != 0)
            {
                byte[] buff = new byte[len];
                serialPort1.Read(buff, 0, len);
                receivedata = Encoding.UTF8.GetString(buff);

            }
            if (textBox3.InvokeRequired)
            {
                //如果需要invoke
                //step 1. 建立一個delegate方法
                Action updateMethod = new Action(() => textBox3.AppendText(receivedata));

                //step 2. 交給元件Invoke去執行delegate方法
                textBox3.Invoke(updateMethod);
            }
            else
            {
                //如果不需要，那就直接更新元件吧
                textBox3.AppendText(receivedata);
            }
            serialPort1.DiscardInBuffer();
            //Console.WriteLine(receivedata);

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void refresh_b_Click(object sender, EventArgs e)
        {
            
                portSearch();
        }

        private void COM_Click(object sender, EventArgs e)
        {   
            if(comboPort.SelectedItem!=null)
            {
                if(COM.Text=="OFF")
                {
                    portOn(comboPort.SelectedItem.ToString());
                }
                else if(COM.Text == "ON")
                {
                    portOff(comboPort.SelectedItem.ToString());
                }
            }
        }
        
        
        
        private void motionSave_Click(object sender, EventArgs e)
        {
            string[] controData = new string[17];
            controData[0] = (dataGridView1.Rows.Count).ToString();
            for(int i=0;i<16;i++)
            {
                int obj = int.Parse(Properties.Settings.Default["servo" + i.ToString()].ToString());
                if (Controls.Find("maskedTextBox" + obj.ToString(), true)[0].Enabled)
                {
                    
                    controData[i+1] = Controls.Find("maskedTextBox" + obj.ToString(), true)[0].Text;
                }
            }
            dataGridView1.Rows.Add(controData);  
        }

        private void readfile(string folder)
        {
            string[] lines = null;
            lines = System.IO.File.ReadAllLines(folder);
            textBox3.Text = "";

            foreach (string line in lines)
            {
                textBox3.Text+=line+"\n";

            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {  
            if (!serialPort1.IsOpen)
            {
                COM.Text = "OFF";
                COM.ForeColor = Color.Red;
                comboPort.Enabled = true;
                refresh_b.Enabled = true;
                if (dataSend.Enabled)
                    dataSend.Stop();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textBox1.Text + "\n");
                textBox3.Text += "<<" + textBox1.Text + "\r\n";
                textBox1.Text = "";
            }
            
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                serialPort1.Write(textBox1.Text+"\n");
                textBox3.Text += "<<" + textBox1.Text + "\r\n";
                textBox1.Text = "";
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows!=null)
            {
                int selectIndex = dataGridView1.Rows.IndexOf(dataGridView1.CurrentRow);
                if (selectIndex > 0)
                {
                    var temp = dataGridView1.CurrentRow;
                    dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                    dataGridView1.Rows.Insert(selectIndex - 1, temp);
                }
            }
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows != null)
            {
                int selectIndex = dataGridView1.Rows.IndexOf(dataGridView1.CurrentRow);
                if (selectIndex+1 < dataGridView1.Rows.Count)
                {
                    var temp = dataGridView1.CurrentRow;
                    dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                    dataGridView1.Rows.Insert(selectIndex + 1, temp);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
        }

        private void floaderOpen_Click(object sender, EventArgs e)
        {
            string opened_file = null;
            SaveFileDialog opd=new SaveFileDialog();
            opd.Title = "開啟檔案";
            opd.Filter = "CSV|*.csv";
            opd.AddExtension = true;
            opd.OverwritePrompt = false;
            //opd.FileName = "Result_20200325.csv";
            if (opd.ShowDialog() == DialogResult.OK)
            {
                opened_file = opd.FileName;
                textBox2.Text = opened_file;

            }
        }

        private void FolderLoad_Click(object sender, EventArgs e)
        {
            if(textBox2.Text!=null)
            {
                string[] text = File.ReadAllLines(textBox2.Text, Encoding.UTF8);
                for(int i=2; i<text.Length/2; i++)
                {
                    string[] cells = text[i].Split(',');
                    dataGridView1.Rows.Add(cells);
                }     
            }
        }

        private void FolderSave_Click(object sender, EventArgs e)
        {
            //string filename = textBox1.Text;
            if (textBox2.Text != null)
            {
                Task.Run(() =>
                {
                    
                        if (dataGridView1.Rows.Count == 0)
                        {
                            MessageBox.Show("無資料");
                        }
                        string csv = "Degree\r\n";
                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            csv += column.HeaderText + ',';
                        }
                        csv += "\r\n";
                        foreach (DataGridViewRow item in dataGridView1.Rows)
                        {

                            foreach (DataGridViewCell cell in item.Cells)
                            {
                                csv += cell.Value.ToString()+",";

                            }
                            csv += "\r\n";
                        }
                    
                    csv += "In Lib\r\n";
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        csv += column.HeaderText + ',';
                    }
                    csv += "\r\n";
                    foreach (DataGridViewRow item in dataGridView1.Rows)
                    {
                        csv += item.Cells[0].Value.ToString()+",";
                        for (int i=1;i<17;i++ )
                        {
                            string vl = item.Cells[i].Value.ToString();
                            csv += (double.Parse(vl) / 2.5 + 9).ToString()+",";

                        }
                        csv += "\r\n";
                    }
                    File.WriteAllText(textBox2.Text, csv, Encoding.UTF8);
                });
                MessageBox.Show("saved");
            }
        }

        

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            //Graphics img = Graphics.FromImage();
            
        }
        bool latestMotion = true;
        bool ableToSend = true;
        static int[] rob_motion =new int[]{ 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45 };
        //static int[] rob_motion = new int[] { 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60 };
        private void barAction(string index)
        {
            for(int i=0;i<16;i++)
            {
                string ind = Properties.Settings.Default["servo" + i.ToString()].ToString();
                rob_motion[i] = Convert.ToInt32(controller.Controls.Find("trackBar" + ind, false).OfType<TrackBar>().First().Value / 2.5 + 9);//1.875)+12;
            }
            latestMotion = false;
            //int bar = controller.Controls.Find("trackBar" + index, false).OfType<TrackBar>().First().Value;
            //rob_motion[int.Parse(Properties.Settings.Default["servo" + index].ToString())] = bar / 2;
            
            //string ouput = index + "," + (bar/2).ToString()+"\n";
            //string ouput = "";
            
            //if (serialPort1.IsOpen & ableToSend)
            //{
            //for (int i = 0; i < 16; i++)
            //{
            //    ouput += $"{rob_motion[i]}{(i == 15 ? "\n" : " ")}";
            //}
            //    serialPort1.Write(ouput);
            //    //textBox3.Text += ouput + "\r\n";
            //    //serialPort1.Write(ouput);
            //    //serialPort1.DiscardOutBuffer();
            //    //serialPort1.DiscardInBuffer();
            //    ableToSend = false;
                
            //    Task.Run(() =>
            //    {
            //        Thread.Sleep(400);
            //        ableToSend = true;
            //        //for (int j = 0; j < 4; j++)
            //        //{
            //        //    string ouput = "";
            //        //    for (int i = 0; i < 4; i++)
            //        //    {

            //        //        ouput += $"{rob_motion[4 * j + i]}{(i == 3 ? "\n" : ",")}";
            //        //    }
            //        //    if (serialPort1.IsOpen)
            //        //    {
            //        //        serialPort1.DiscardOutBuffer();
            //        //        serialPort1.Write(ouput);
            //        //    }
            //        //    Thread.Sleep(10);
            //        //    //textBox3.Text += ouput + "*\r\n";
            //        //}
            //    });
            //}
            controller.Controls.Find("maskedTextBox" + index, false).OfType<MaskedTextBox>().First().Text = controller.Controls.Find("trackBar" + index, false).OfType<TrackBar>().First().Value.ToString();
        }

        System.Timers.Timer dataSend = new System.Timers.Timer(95);
        private void controller_Enter(object sender, EventArgs e)
        {
            //if ((!dataSend.Enabled) & serialPort1.IsOpen)
            //{
            //    dataSend.Start();
            //    dataSend.Elapsed += DataSend_Elapsed;

                
            //    //MessageBox.Show("start");
            //}
        }

        private void DataSend_Elapsed(object sender, ElapsedEventArgs e)
        {

            string ouput = "";
            //if (serialPort1.IsOpen & (!latestMotion))
            //{
            //    for (int i = 0; i < 16; i++)
            //    {
            //        ouput = i.ToString() + " " + rob_motion[i].ToString();

            //        try
            //        {
            //            serialPort1.DiscardOutBuffer();
            //            //serialPort1.DiscardInBuffer();
            //            serialPort1.Write(ouput);

            //        }
            //        catch (InvalidOperationException)
            //        {
            //            serialPort1.Close();
            //            MessageBox.Show("Fail to connect");
            //        }
            //        catch (IOException)
            //        {
            //            serialPort1.Close();
            //            MessageBox.Show("Fail to connect");
            //        }
            //        Thread.Sleep(50);
            //    }
            //    latestMotion = true;
            //}

            for (int i = 0; i < 16; i++)
            {
                ouput += $"{rob_motion[i]}{(i == 15 ? "\n" : " ")}";
            }

            if (serialPort1.IsOpen)
            {

                //try
                //{
                    //serialPort1.Write("6\n");
                    serialPort1.DiscardOutBuffer();
                    //serialPort1.DiscardInBuffer();
                    serialPort1.Write(ouput);
                    //latestMotion = true;
                //}
                //catch (InvalidOperationException)
                //{
                //    serialPort1.Close();
                //    MessageBox.Show("Fail to connect");
                //}
                //catch (IOException)
                //{
                //    serialPort1.Close();
                //    MessageBox.Show("Fail to connect");
                //}

            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            barAction("0");
        }
        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            barAction("1");
        }

        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            barAction("2");
        }

        private void trackBar4_ValueChanged(object sender, EventArgs e)
        {
            barAction("3");
        }

        private void trackBar5_ValueChanged(object sender, EventArgs e)
        {
            barAction("4");
        }

        private void trackBar6_ValueChanged(object sender, EventArgs e)
        {
            barAction("5");
        }

        private void trackBar7_ValueChanged(object sender, EventArgs e)
        {
            barAction("6");
        }

        private void trackBar8_ValueChanged(object sender, EventArgs e)
        {
            barAction("7");
        }

        private void trackBar9_ValueChanged(object sender, EventArgs e)
        {
            barAction("8");
        }

        private void trackBar10_ValueChanged(object sender, EventArgs e)
        {
            barAction("9");
        }

        private void trackBar11_ValueChanged(object sender, EventArgs e)
        {
            barAction("10");
        }

        private void trackBar12_ValueChanged(object sender, EventArgs e)
        {
            barAction("11");
        }

        private void trackBar13_ValueChanged(object sender, EventArgs e)
        {
            barAction("12");
        }

        private void trackBar14_ValueChanged(object sender, EventArgs e)
        {
            barAction("13");
        }

        private void trackBar15_ValueChanged(object sender, EventArgs e)
        {
            barAction("14");
        }

        private void trackBar16_ValueChanged(object sender, EventArgs e)
        {
            barAction("15");
        }

        private void keyboardAction(string index)
        {
            TrackBar bar = controller.Controls.Find("trackBar" + index, false).OfType<TrackBar>().First();
            MaskedTextBox mtext = controller.Controls.Find("maskedTextBox" + index, false).OfType<MaskedTextBox>().First();
            int textValue = int.Parse(mtext.Text);
            if ((textValue <= 180) && (textValue >= 0))
            {
                for (int i = 0; i < 16; i++)
                {
                    string ind = Properties.Settings.Default["servo" + i.ToString()].ToString();
                    rob_motion[i] = controller.Controls.Find("trackBar" + ind, false).OfType<TrackBar>().First().Value / 2;
                }
                //string ouput = "";
                //for (int i = 0; i < 16; i++)
                //{
                //    ouput += $"{rob_motion[i]}{(i == 15 ? "\n" : " ")}";
                //}
                //if (serialPort1.IsOpen)
                //{
                //    serialPort1.Write(ouput);
                //}
                bar.Value = textValue;              
                //textBox3.Text += ouput+"\r\n";
            }
            else
            {
                mtext.Text = bar.Value.ToString();
            }
        }

        private void maskedTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("0");
            }
        }

        private void maskedTextBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("1");
            }
        }

        private void maskedTextBox3_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("2");
            }
        }

        private void maskedTextBox4_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("3");
            }
        }

        private void maskedTextBox5_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("4");
            }
        }

        private void maskedTextBox6_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("5");
            }
        }

        private void maskedTextBox7_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("6");
            }
        }

        private void maskedTextBox8_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("7");
            }
        }

        private void maskedTextBox9_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("8");
            }
        }

        private void maskedTextBox10_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("9");
            }
        }

        private void maskedTextBox11_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("10");
            }
        }

        private void maskedTextBox12_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("11");
            }
        }

        private void maskedTextBox13_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("12");
            }
        }

        private void maskedTextBox14_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("13");
            }
        }

        private void maskedTextBox15_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("14");
            }
        }

        private void maskedTextBox16_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                keyboardAction("15");
            }
        }


        private void maskedTextBox1_Leave(object sender, EventArgs e)
        {
            keyboardAction("0");
        }

        private void maskedTextBox2_Leave(object sender, EventArgs e)
        {
            keyboardAction("1");
        }

        private void maskedTextBox3_Leave(object sender, EventArgs e)
        {
            keyboardAction("2");
        }

        private void maskedTextBox4_Leave(object sender, EventArgs e)
        {
            keyboardAction("3");
        }

        private void maskedTextBox5_Leave(object sender, EventArgs e)
        {
            keyboardAction("4");
        }

        private void maskedTextBox6_Leave(object sender, EventArgs e)
        {
            keyboardAction("5");
        }

        private void maskedTextBox7_Leave(object sender, EventArgs e)
        {
            keyboardAction("6");
        }

        private void maskedTextBox8_Leave(object sender, EventArgs e)
        {
            keyboardAction("7");
        }

        private void maskedTextBox9_Leave(object sender, EventArgs e)
        {
            keyboardAction("8");
        }

        private void maskedTextBox10_Leave(object sender, EventArgs e)
        {
            keyboardAction("9");
        }
        private void maskedTextBox11_Leave(object sender, EventArgs e)
        {
            keyboardAction("10");
        }

        private void maskedTextBox12_Leave(object sender, EventArgs e)
        {
            keyboardAction("11");
        }

        private void maskedTextBox13_Leave(object sender, EventArgs e)
        {
            keyboardAction("12");
        }

        private void maskedTextBox14_Leave(object sender, EventArgs e)
        {
            keyboardAction("13");
        }

        private void maskedTextBox15_Leave(object sender, EventArgs e)
        {
            keyboardAction("14");
        }

        private void maskedTextBox16_Leave(object sender, EventArgs e)
        {
            keyboardAction("15");
        }


        private void maskedTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox0.SelectAll();
        }

        private void maskedTextBox2_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox1.SelectAll();
        }

        private void maskedTextBox3_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox2.SelectAll();
        }

        private void maskedTextBox4_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox3.SelectAll();
        }

        private void maskedTextBox5_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox4.SelectAll();
        }

        private void maskedTextBox6_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox5.SelectAll();
        }

        private void maskedTextBox7_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox6.SelectAll();
        }

        private void maskedTextBox8_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox7.SelectAll();
        }

        private void maskedTextBox9_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox8.SelectAll();
        }

        private void maskedTextBox10_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox9.SelectAll();
        }

        private void maskedTextBox11_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox10.SelectAll();
        }

        private void maskedTextBox12_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox11.SelectAll();
        }

        private void maskedTextBox13_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox12.SelectAll();
        }

        private void maskedTextBox14_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox13.SelectAll();
        }

        private void maskedTextBox15_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox14.SelectAll();
        }

        private void maskedTextBox16_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox15.SelectAll();
        }

        private void clearButton_Click(object sendr, EventArgs e)
        {
            textBox3.Text = "";
            if (serialPort1.IsOpen)
            {


                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (dataSend.Enabled)
                dataSend.Dispose();
            Properties.Settings.Default["dataFolder"] = textBox2.Text;
            Properties.Settings.Default.Save();
            serialPort1.Dispose();
            
            
        }
        bool settingChangeBack = false;
        private void settings_Leave(object sender, EventArgs e)
        {
            Dictionary<int, int> check = new Dictionary<int, int>(16);
            for(int i=0;i<16;i++)
            {
                check.Add(i, 0);
            }
            var cbos = tabContr.TabPages["settings"].Controls.OfType<GroupBox>().First().Controls.OfType<ComboBox>().ToList();//tabContr.TabPages["settings"].Controls.OfType<ComboBox>().ToList();
            
            cbos = cbos.OrderBy(x => x.TabIndex).ToList();
            foreach(ComboBox cbo in cbos)
            {
                if(cbo.SelectedIndex!=0)
                {
                    check[cbo.SelectedIndex-1]+=1;
                }
                
            }
            if(check.Values.Max()<2)
            {
                int ind = 0;
                
                foreach (ComboBox cbo in cbos)
                {
                    if (cbo.SelectedIndex == 0)
                    {
                        Controls.Find("maskedTextBox" + ind.ToString(),true)[0].Enabled = false;
                        Controls.Find("trackBar" + ind.ToString(),true)[0].Enabled = false;
                    }
                    else
                    {
                        Properties.Settings.Default["servo" + ind.ToString()] = (cbo.SelectedIndex - 1).ToString();
                    }
                    ind++;
                }
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
                
                settingChangeBack = false;
            }
            else
            {
                MessageBox.Show("重複指定部件");
                settingChangeBack = true;
            }
        }
        

        private void settings_Enter(object sender, EventArgs e)
        {
            //this.Controls.Find("sCombo1" + "15", true);
            
            for (int i = 0; i < 16; i++)
            {
                //link.Add(this.Controls.Find("control" + i.ToString(), true), this.Controls.Find("");
                var c = Controls.Find("sCombo" + i.ToString(), true).OfType<ComboBox>().ToArray()[0];
                string obj= Properties.Settings.Default["servo" + i.ToString()].ToString();
                c.SelectedIndex=int.Parse(obj)+1;
                
                //Properties.Settings.Default.new()
            }

        }

        private void configReset_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Reload();
            for (int i = 0; i < 16; i++)
            {
                //link.Add(this.Controls.Find("control" + i.ToString(), true), this.Controls.Find("");
                var c = Controls.Find("sCombo" + i.ToString(), true).OfType<ComboBox>().ToArray()[0];
                c.SelectedIndex = int.Parse(Properties.Settings.Default["servo" + i.ToString()].ToString()) + 1;
                
                //Properties.Settings.Default.new()
            }
        }

        private void tabContr_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (settingChangeBack)
            {
                tabContr.SelectedTab = settings;
            }
        }

       
    

        private void controller_Leave(object sender, EventArgs e)
        {
            //dataSend.Elapsed -= DataSend_Elapsed;
            //dataSend.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dataSend.Interval = double.Parse(numericUpDown1.Value.ToString());
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string[] controData = new string[17];
            int selectIndex = dataGridView1.Rows.IndexOf(dataGridView1.CurrentRow);
            
            for (int i = 0; i < 16; i++)
            {
                int obj = int.Parse(Properties.Settings.Default["servo" + i.ToString()].ToString());
                if (Controls.Find("maskedTextBox" + obj.ToString(), true)[0].Enabled)
                {

                    Controls.Find("maskedTextBox" + obj.ToString(), true)[0].Text =dataGridView1.Rows[selectIndex].Cells[0].Value.ToString();
                    controller.Controls.Find("trackBar" + obj.ToString(), false).OfType<TrackBar>().First().Value = int.Parse(dataGridView1.Rows[selectIndex].Cells[0].Value.ToString());
                }
            }
        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            //if(dataGridView1.Columns.GetColumnCount(dataGridView1.)
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Selected)
            {
                //int selectIndex = dataGridView1.SelectedRows;
                
                for (int i = 0; i < 16; i++)
                {
                    int obj = int.Parse(Properties.Settings.Default["servo" + i.ToString()].ToString());
                    if (Controls.Find("maskedTextBox" + obj.ToString(), true)[0].Enabled)
                    {

                        Controls.Find("maskedTextBox" + obj.ToString(), true)[0].Text = dataGridView1.SelectedRows[0].Cells[i+1].Value.ToString();
                        controller.Controls.Find("trackBar" + obj.ToString(), false).OfType<TrackBar>().First().Value = int.Parse(dataGridView1.SelectedRows[0].Cells[i+1].Value.ToString());
                    }
                }
            }
        }

       
    }
}
