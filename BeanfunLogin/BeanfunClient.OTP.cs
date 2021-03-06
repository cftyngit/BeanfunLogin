﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Security.Cryptography;

namespace BeanfunLogin
{
    public partial class BeanfunClient : WebClient
    {

        // Decrypt OTP.
        private string DecryptDES(string hexString, string key)
        {
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;
                des.Key = Encoding.ASCII.GetBytes(key);
                byte[] s = new byte[hexString.Length / 2];
                int j = 0;
                for (int i = 0; i < hexString.Length / 2; i++)
                {
                    s[i] = Byte.Parse(hexString[j].ToString() + hexString[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                    j += 2;
                }
                ICryptoTransform desencrypt = des.CreateDecryptor();
                return Encoding.ASCII.GetString(desencrypt.TransformFinalBlock(s, 0, s.Length));
            }
            catch
            {
                this.errmsg = "DecryptDESError";
                return null;
            }
        }

        public string GetOTP(int loginMethod, AccountList acc)
        {
            try
            {
                string response;
                if (loginMethod == 5)
                    response = this.DownloadString("https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=610074&service_region=T9&sotp=" + acc.sotp + "&dt=" + GetCurrentTime(2));
                else
                    response = this.DownloadString("https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start_step2.aspx%3Fservice_code%3D610074%26service_region%3DT9%26sotp%3D" + acc.sotp + "&web_token=" + this.webtoken);
                if (response == "")
                { this.errmsg = "OTPNoResponse"; return null; }
                Regex regex = new Regex("GetResultByLongPolling&key=(.*)\"");
                if (!regex.IsMatch(response))
                { this.errmsg = "OTPNoLongPollingKey"; return null; }
                string longPollingKey = regex.Match(response).Groups[1].Value;
                if (acc.screatetime == null)
                {
                    regex = new Regex("ServiceAccountCreateTime: \"([^\"]+)\"");
                    if (!regex.IsMatch(response))
                    { this.errmsg = "OTPNoCreateTime"; return null; }
                    acc.screatetime = regex.Match(response).Groups[1].Value;
                }
                response = this.DownloadString("https://tw.newlogin.beanfun.com/generic_handlers/get_cookies.ashx");
                regex = new Regex("var m_strSecretCode = '(.*)';");
                if (!regex.IsMatch(response))
                { this.errmsg = "OTPNoSecretCode"; return null; }
                string secretCode = regex.Match(response).Groups[1].Value;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("service_code", "610074");
                payload.Add("service_region", "T9");
                payload.Add("service_account_id", acc.sacc);
                payload.Add("service_sotp", acc.sotp);
                payload.Add("service_display_name", acc.sname);
                payload.Add("service_create_time", acc.screatetime);
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.new.beanfun.com/beanfun_block/generic_handlers/record_service_start.ashx", payload));
                response = this.DownloadString("https://tw.new.beanfun.com/generic_handlers/get_result.ashx?meth=GetResultByLongPolling&key=" + longPollingKey + "&_=" + GetCurrentTime());
                response = this.DownloadString("https://tw.new.beanfun.com/beanfun_block/generic_handlers/get_webstart_otp.ashx?SN=" + longPollingKey + "&WebToken=" + this.webtoken + "&SecretCode=" + secretCode + "&ppppp=FE40250C435D81475BF8F8009348B2D7F56A5FFB163A12170AD615BBA534B932&ServiceCode=610074&ServiceRegion=T9&ServiceAccount=" + acc.sacc + "&CreateTime=" + acc.screatetime.Replace(" ", "%20"));
                response = response.Substring(2);
                string key = response.Substring(0, 8);
                string plain = response.Substring(8);
                string otp = DecryptDES(plain, key);
                if (otp != null)
                    this.errmsg = null;       
           
                return otp;
            }
            catch
            {
                this.errmsg = "OTPUnknown"; 
                return null;
            }
        }

    }
}
