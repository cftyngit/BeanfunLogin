﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Net;
using System.Diagnostics;

namespace BeanfunLogin
{
    public partial class main : Form
    {
        private string otp;

        // Login do work.
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = "";
            try
            {
                this.bfClient.Login(this.textBox1.Text, this.textBox2.Text, Properties.Settings.Default.loginMethod, this.textBox4.Text);
                if (this.bfClient.errmsg != null)
                    e.Result = this.bfClient.errmsg;
                else
                    e.Result = null;
            }
            catch
            {
                e.Result = "登入失敗，未知的錯誤。"; 
            }
        }

        // Login completed.
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.panel2.Enabled = true;
            this.UseWaitCursor = false;
            this.button1.Text = "登入";
            if (e.Error != null)
            {
                errexit(e.Error.Message, 1);
                return;
            }
            if ((string)e.Result != null)
            {
                errexit((string)e.Result, 1);
                return;
            }

            try
            {
                listView1.Items.Clear();
                foreach (var account in this.bfClient.accountList)
                {
                    string[] row = { WebUtility.HtmlDecode(account.sname), account.sacc };
                    var listViewItem = new ListViewItem(row);
                    this.listView1.Items.Add(listViewItem);
                }

                // Handle panel switching.
                this.ActiveControl = null;
                this.panel2.SendToBack();
                this.panel1.BringToFront();
                this.AcceptButton = this.button3;
                if (Properties.Settings.Default.autoSelect == true)
                {
                    this.textBox3.Text = "獲取密碼中...";
                    this.listView1.Enabled = false;
                    this.backgroundWorker1.RunWorkerAsync(Properties.Settings.Default.autoSelectIndex);
                }
                if (Properties.Settings.Default.keepLogged && !this.ping.IsBusy)
                    this.ping.RunWorkerAsync();
                ShowToolTip(listView1, "步驟1", "選擇欲開啟的遊戲帳號，雙擊以複製帳號。");
                ShowToolTip(button3, "步驟2", "按下以在右側產生並自動複製密碼，至遊戲中貼上帳密登入。");
                Tip.SetToolTip(button3, "點擊獲取密碼");
                Tip.SetToolTip(listView1, "雙擊即自動複製");
                Tip.SetToolTip(textBox3, "點擊一次即自動複製");
                Properties.Settings.Default.showTip = false;
                Properties.Settings.Default.Save();
            }
            catch
            {
                errexit("登入失敗，無法取得帳號列表。", 1);
            }
            
        }

        // getOTP do work.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int index = (int)e.Argument;
            this.otp = this.bfClient.GetOTP(Properties.Settings.Default.loginMethod, this.bfClient.accountList[index]);
            if (this.otp == null)
                e.Result = -1;
            else
            {
                e.Result = index;
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "MapleStory")
                        return;
                }
                ProcessStartInfo gamestart = new ProcessStartInfo();
                gamestart.FileName = Properties.Settings.Default.gamePath;
                gamestart.Verb = "runas";
                try
                {
                    Process.Start(gamestart);
                }
                catch
                {
                    errexit("啟動失敗，請嘗試手動以系統管理員身分啟動遊戲。", 2);
                }
            }
            
        }

        // getOTP completed.
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.button3.Text = "獲取密碼";
            this.listView1.Enabled = true;
            if (e.Error != null)
            {
                this.textBox3.Text = "獲取失敗";
                errexit(e.Error.Message, 2);
                return;
            }
            int index = (int)e.Result;

            if (index == -1)
            {
                this.textBox3.Text = "獲取失敗";
                errexit(this.bfClient.errmsg, 2);
            }
            else
            {
                this.textBox3.Text = this.otp;
                Clipboard.SetText(textBox3.Text);
                this.Text = "進行遊戲 - " + WebUtility.HtmlDecode(this.bfClient.accountList[index].sname);
            }
        }

    }
}
