﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Diagnostics;

namespace BeanfunLogin
{
    public partial class BeanfunClient : WebClient
    {

        private string RegularLogin(string id, string pass, string skey)
        {
            try
            {
                string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + skey);
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                    {this.errmsg = "LoginNoViewstate"; return null;}
                string viewstate = regex.Match(response).Groups[1].Value;
                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                    { this.errmsg = "LoginNoEventvalidation"; return null; }
                string eventvalidation = regex.Match(response).Groups[1].Value;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("t_AccountID", id);
                payload.Add("t_Password", pass);
                payload.Add("CodeTextBox", "");
                payload.Add("btn_login.x", "46");
                payload.Add("btn_login.y", "31");
                payload.Add("LBD_VCID_c_login_idpass_form_samplecaptcha", "");
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + skey, payload));
                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                { this.errmsg = "LoginNoAkey"; return null; }
                string akey = regex.Match(this.ResponseUri.ToString()).Groups[1].Value;

                return akey;
            }
            catch
            {
                this.errmsg = "LoginUnknown";
                return null;
            }
        }

        private bool vaktenAuthenticate(string lblSID)
        {
            try
            {
                string[] ports = { "14057", "16057", "17057" };
                foreach (string port in ports)
                {
                    string response = this.DownloadString("https://localhost:" + port + "/api/1/status.jsonp?api=YXBpLmtleXBhc2NvaWQuY29tOjQ0My9SZXN0L0FwaVNlcnZpY2Uv&callback=_jqjsp&alt=json-in-script");
                    if (response == "_jqjsp( {\"statusCode\":200} );")
                    {
                        response = this.DownloadString("https://localhost:" + port + "/api/1/aut.jsonp?sid=GAMANIA" + lblSID + "&api=YXBpLmtleXBhc2NvaWQuY29tOjQ0My9SZXN0L0FwaVNlcnZpY2Uv&callback=_jqjsp&alt=json-in-script");
                        if (response == "_jqjsp( {\"statusCode\":200} );") return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string KeypascoLogin(string id, string pass, string skey)
        {
            try
            {
                string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/keypasco_form.aspx?skey=" + skey);
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoViewstate"; return null; }
                string viewstate = regex.Match(response).Groups[1].Value;
                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoEventvalidation"; return null; }
                string eventvalidation = regex.Match(response).Groups[1].Value;
                regex = new Regex("lblSID\"><font color=\"White\">(\\w+)</font></span>");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoLblSID"; return null; }
                string lblSID = regex.Match(response).Groups[1].Value;
                if (!vaktenAuthenticate(lblSID))
                { this.errmsg = "LoginNoResponseVakten"; return null; }

                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("t_AccountID", id);
                payload.Add("t_Password", pass);
                payload.Add("CodeTextBox", "");
                payload.Add("btn_login.x", "46");
                payload.Add("btn_login.y", "31");
                payload.Add("LBD_VCID_c_login_keypasco_form_samplecaptcha", "");
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/keypasco_form.aspx?skey=" + skey, payload));
                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                { this.errmsg = "LoginNoAkey"; return null; }
                return regex.Match(this.ResponseUri.ToString()).Groups[1].Value;
            }
            catch
            {
                this.errmsg = "LoginUnknown";
                return null;
            }
        }

        private string GamaotpLogin(string id, string pass, string skey)
        {
            try
            {
                string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/gamaotp_form.aspx?skey=" + skey);
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoViewstate"; return null; }
                string viewstate = regex.Match(response).Groups[1].Value;
                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoEventvalidation"; return null; }
                string eventvalidation = regex.Match(response).Groups[1].Value;
                regex = new Regex("motp_challenge_code\" value=\"(\\d+)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoMotp"; return null; }
                string motp = regex.Match(response).Groups[1].Value;
                response = this.DownloadString("https://tw.newlogin.beanfun.com/generic_handlers/get_security_otp.ashx?d=" + GetCurrentTime(1));
                regex = new Regex("<playsafe_otp>(\\w+)</playsafe_otp>");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoSotp"; return null; }
                string sotp = regex.Match(response).Groups[1].Value;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("original", "M~" + sotp + "~" + id + "~" + pass + "|" + motp);
                payload.Add("signature", "");
                payload.Add("serverotp", sotp);
                payload.Add("motp_challenge_code", motp);
                payload.Add("t_AccountID", id);
                payload.Add("t_Password", pass);
                payload.Add("btn_login", "Login");
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/gamaotp_form.aspx?skey=" + skey, payload));
                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                { this.errmsg = "LoginNoAkey"; return null; }
                return regex.Match(this.ResponseUri.ToString()).Groups[1].Value;
            }
            catch
            {
                this.errmsg = "LoginUnknown"; 
                return null;
            }
        }

        private string OtpLogin(string userID, string pass, string skey)
        {
            try
            {
                string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/otp_form.aspx?skey=" + skey);
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoViewstate"; return null; }
                string viewstate = regex.Match(response).Groups[1].Value;
                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoEventvalidation"; return null; }
                string eventvalidation = regex.Match(response).Groups[1].Value;
                response = this.DownloadString("https://tw.newlogin.beanfun.com/generic_handlers/get_security_otp.ashx?d=" + GetCurrentTime(1));
                regex = new Regex("<playsafe_otp>(\\w+)</playsafe_otp>");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoSotp"; return null; }
                string sotp = regex.Match(response).Groups[1].Value;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("original", "O~" + sotp + "~" + userID + "~" + pass);
                payload.Add("signature", "");
                payload.Add("serverotp", sotp);
                payload.Add("t_AccountID", userID);
                payload.Add("t_Password", pass);
                payload.Add("btn_login", "Login");
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/otp_form.aspx?skey=" + skey, payload));
                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                { this.errmsg = "LoginNoAkey"; return null; }
                return regex.Match(this.ResponseUri.ToString()).Groups[1].Value;
            }
            catch
            {
                this.errmsg = "LoginUnknown";
                return null;
            }
        }

        private string OtpELogin(string id, string pass, string securePass, string skey)
        {
            try
            {
                string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/otp_form.aspx?type=E&skey=" + skey);
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoViewstate"; return null; }
                string viewstate = regex.Match(response).Groups[1].Value;
                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoEventvalidation"; return null; }
                string eventvalidation = regex.Match(response).Groups[1].Value;
                response = this.DownloadString("https://tw.newlogin.beanfun.com/generic_handlers/get_security_otp.ashx?d=" + GetCurrentTime(1));
                regex = new Regex("<playsafe_otp>(\\w+)</playsafe_otp>");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoSotp"; return null; }
                string sotp = regex.Match(response).Groups[1].Value;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("original", "E~" + sotp + "~" + id + "~" + pass);
                payload.Add("signature", "");
                payload.Add("serverotp", sotp);
                payload.Add("t_AccountID", id);
                payload.Add("t_MainAccountPassword", pass);
                payload.Add("t_Password", securePass);
                payload.Add("btn_login", "Login");
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/otp_form.aspx?type=E&skey=" + skey, payload));
                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                { this.errmsg = "LoginNoAkey"; return null; }
                return regex.Match(this.ResponseUri.ToString()).Groups[1].Value;
            }
            catch
            {
                this.errmsg = "LoginUnknown"; 
                return null;
            }
        }

        private string playsafeLogin(string id, string pass, string skey)
        {
            try
            {
                string response = this.DownloadString("https://tw.newlogin.beanfun.com/login/playsafe_form.aspx?skey=" + skey);
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoViewstate"; return null; }
                string viewstate = regex.Match(response).Groups[1].Value;
                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoEventvalidation"; return null; }
                string eventvalidation = regex.Match(response).Groups[1].Value;
                response = this.DownloadString("https://tw.newlogin.beanfun.com/generic_handlers/get_security_otp.ashx?d=" + GetCurrentTime(1));
                regex = new Regex("<playsafe_otp>(\\w+)</playsafe_otp>");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoSotp"; return null; }
                string sotp = regex.Match(response).Groups[1].Value;

                PlaySafe ps = new PlaySafe();
                var readername = ps.GetReader();
                if (readername == null)
                { this.errmsg = "LoginNoReaderName"; return null; }
                if (ps.cardType == null)
                { this.errmsg = "LoginNoCardType"; return null; }
                ps.cardid = ps.GetPublicCN(readername);
                if (ps.cardid == null)
                { this.errmsg = "LoginNoCardId"; return null; }
                var opinfo = ps.GetOPInfo(readername, pass);
                if (opinfo == null)
                { this.errmsg = "LoginNoOpInfo"; return null; }
                var original = ps.cardType + "~" + sotp + "~" + id + "~" + opinfo;
                var encryptedData = ps.EncryptData(readername, pass, original);
                if (encryptedData == null)
                { this.errmsg = "LoginNoEncryptedData"; return null; }

                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("card_check_id", ps.cardid);
                payload.Add("original", original);
                payload.Add("signature", encryptedData);
                payload.Add("serverotp", sotp);
                payload.Add("t_AccountID", id);
                payload.Add("t_Password", pass);
                payload.Add("btn_login", "Login");
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/playsafe_form.aspx?skey=" + skey, payload));
                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                { this.errmsg = "LoginNoAkey"; return null; }
                return ps.cardid + " " + regex.Match(this.ResponseUri.ToString()).Groups[1].Value;
            }
            catch
            {
                this.errmsg = "LoginUnknown";
                return null;
            }
        }

        public void Login(string id, string pass, int loginMethod, string securePass = null)
        {
            try
            {
                string response = this.DownloadString("https://tw.beanfun.com/beanfun_block/bflogin/default.aspx?service=999999_T0");
                if (response == "")
                { this.errmsg = "LoginNoResponse"; return; }
                response = this.ResponseUri.ToString();
                Debug.WriteLine(response);
                Regex regex = new Regex("skey=(.*)&display");
                if (!regex.IsMatch(response))
                { this.errmsg = "LoginNoSkey"; return; }
                string skey = regex.Match(response).Groups[1].Value;
                string akey = null;
                string cardid = null;

                switch (loginMethod)
                {
                    case 0:
                        akey = RegularLogin(id, pass, skey);
                        break;
                    case 1:
                        akey = KeypascoLogin(id, pass, skey);
                        break;
                    case 2:
                        akey = GamaotpLogin(id, pass, skey);
                        break;
                    case 3:
                        akey = OtpLogin(id, pass, skey);
                        break;
                    case 4:
                        akey = OtpELogin(id, pass, securePass, skey);
                        break;
                    case 5:
                        string[] temp = playsafeLogin(id, pass, skey).Split(' ');
                        if (temp.Count() != 2)
                        { this.errmsg = "LoginPlaySafeResultError"; return; }
                        cardid = temp[0];
                        akey = temp[1];
                        break;
                    default:
                        this.errmsg = "LoginNoMethod";
                        return;
                }
                if (akey == null)
                    return;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("SessionKey", skey);
                payload.Add("AuthKey", akey);
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.beanfun.com/beanfun_block/bflogin/return.aspx", payload));
                this.webtoken = this.GetCookie("bfWebToken");
                if (this.webtoken == "")
                { this.errmsg = "LoginNoWebtoken"; return; }
                if (loginMethod == 5)
                    response = this.DownloadString("https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D610074_T9&web_token=" + webtoken + "&cardid=" + cardid, Encoding.UTF8);
                else
                    response = this.DownloadString("https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D610074_T9&web_token=" + webtoken, Encoding.UTF8);

                if (loginMethod == 5)
                {
                    regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                    if (!regex.IsMatch(response))
                    { this.errmsg = "LoginNoViewstate"; return; }
                    string viewstate = regex.Match(response).Groups[1].Value;
                    regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                    if (!regex.IsMatch(response))
                    { this.errmsg = "LoginNoEventvalidation"; return; }
                    string eventvalidation = regex.Match(response).Groups[1].Value;
                    payload = new NameValueCollection();
                    payload.Add("__VIEWSTATE", viewstate);
                    payload.Add("__EVENTVALIDATION", eventvalidation);
                    payload.Add("btnCheckPLASYSAFE", "Hidden+Button");
                    response = Encoding.UTF8.GetString(this.UploadValues("https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D610074_T9&web_token=" + webtoken + "&cardid=" + cardid, payload));
                }

                // Add account list to ListView.
                regex = new Regex("<div id=\"(\\w+)\" sn=\"(\\d+)\" name=\"([^\"]+)\"");
                this.accountList.Clear();
                foreach (Match match in regex.Matches(response))
                {
                    if (match.Groups[1].Value == "" || match.Groups[2].Value == "" || match.Groups[3].Value == "")
                    { this.errmsg = "LoginNoAccountMatch"; return; }
                    accountList.Add(new AccountList(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
                }
                if (accountList.Count == 0)
                { this.errmsg = "LoginNoAccount"; return; }

                this.errmsg = null;
            }
            catch
            {
                this.errmsg = "LoginUnknown"; 
                return;
            }
        }

    }
}
