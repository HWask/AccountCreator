using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace AccountCreator
{
    public class Creator
    {
        private struct Account
        {
            public Account(string Vorname, string Nachname, string Tag, string Monat, string Jahr, string PLZ,
                            string Strasse, string Ort, string Telefon, string Email, string Patrick = null,
                            string User = null, string Pass = null)
            {
                vorname = Vorname;
                nachname = Nachname;
                tag = Tag;
                monat = Monat;
                jahr = Jahr;
                plz = PLZ;
                strasse = Strasse;
                ort = Ort;
                telefon = Telefon;
                email = Email;
                patrick = Patrick;
                user = User;
                pass = Pass;
            }

            public string vorname, nachname, tag, monat, jahr, plz, strasse, ort, telefon, email, patrick, user, pass;
        }

        private Proxy CProxy;
        private SemaphoreSlim semaphore;
        private Form1 form;

        public Creator(SemaphoreSlim sem, Form1 form1)
        {
            semaphore = sem;
            form = form1;
            CProxy = new Proxy();
        }

        private void PushToDataBase(string user, string pass)
        {
            string response;
            Dictionary<string, string> d = new Dictionary<string, string>();
            d["user"] = WebUtility.UrlEncode(user);
            d["pass"] = WebUtility.UrlEncode(pass);

            Http.HttpPostRequest("http://paugasolin.funpic.de/ok/acc.php", d, out response);
        }

        private bool GetCaptcha(string html, CookieCollection cookies, out Image captcha, out CookieCollection PostCookies, string szProxy)
        {
            string src;

            try
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode a = doc.GetElementbyId("img_captcha");
                src = "http://www.ok.de" + a.Attributes["src"].Value;
            }
            catch(Exception e)
            {
                captcha = null;
                PostCookies = null;
                return false;
            }

            CookieCollection out_cooks;

            if(Http.HttpGetPictureRequest(src, out captcha, out out_cooks, cookies))
            {
                PostCookies = new CookieCollection();
                PostCookies.Add(cookies["PHPSESSID"]);
                PostCookies.Add(out_cooks["okrr"]);
                return true;
            }

            PostCookies = null;
            return false;
        }

        private bool ParsePatrick(string html, out string patrick)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode Htmlpatrick = doc.DocumentNode.SelectSingleNode(".//input[@name='patrick']");
                string szPatrick = Htmlpatrick.Attributes["value"].Value;
                patrick = szPatrick;

                return true;
            }
            catch(Exception e)
            {
                patrick = null;
                return false;
            }
        }
        
        /*
        private bool CheckCaptcha(CookieCollection PostCookie, string captcha)
        {
            CookieCollection OUT_cookies;
            string resp;
            Dictionary<string, string> d = new Dictionary<string,string>();
            d["reg_captcha"] = captcha;

            if(Http.HttpPostRequest("http://www.ok.de/reg/api/action/checkCaptcha", d, out resp, out OUT_cookies, PostCookie))
            {
                return true;
            }

            return false;
        }
        */

        private bool FinishRegistration(Account acc, string captcha, CookieCollection cookies, string szProxy)
        {
            string resp;
            CookieCollection out_cookie;

            Dictionary<string, string> d = new Dictionary<string, string>();

            acc.strasse = acc.strasse.Replace('\r', ' ');
            acc.strasse = acc.strasse.Replace('�', 's');
            acc.vorname = acc.vorname.Replace('�', 'u');
            acc.nachname = acc.nachname.Replace('�', 'u');
            acc.ort = acc.ort.Replace('�', 'u');

            if (acc.tag[0] == '0')
                acc.tag = acc.tag.Substring(1, acc.tag.Length-1);

            if (acc.monat[0] == '0')
                acc.monat = acc.monat.Substring(1, acc.monat.Length-1);

            d["reg_code"] = "";
            d["u"] = "";
            d["t"] = "";
            d["e"] = "";
            d["patrick"] = WebUtility.UrlEncode(acc.patrick);
            d["reg_mail"] = WebUtility.UrlEncode(acc.user);
            d["reg_title"] = WebUtility.UrlEncode("male");
            d["reg_firstname"] = WebUtility.UrlEncode(acc.vorname);
            d["reg_lastname"] = WebUtility.UrlEncode(acc.nachname);
            d["regBirthdayDay"] = WebUtility.UrlEncode(acc.tag);
            d["regBirthdayMonth"] = WebUtility.UrlEncode(acc.monat);
            d["regBirthdayYear"] = WebUtility.UrlEncode(acc.jahr);
            d["reg_address"] = WebUtility.UrlEncode(acc.strasse);
            d["reg_postalcode"] = WebUtility.UrlEncode(acc.plz);
            d["reg_city"] = WebUtility.UrlEncode(acc.ort);
            d["reg_country"] = "DEU";
            d["reg_altmail"] = WebUtility.UrlEncode(acc.email);
            d["reg_phone"] = WebUtility.UrlEncode(acc.telefon);
            d["reg_mobile"] = "";
            d["reg_password"] = WebUtility.UrlEncode(acc.pass);
            d["reg_captcha"] = WebUtility.UrlEncode(captcha);

            if (Http.HttpPostRequest("http://www.ok.de/welcome/", d, out resp, out out_cookie, cookies))
            {
                if (!resp.Contains("Fehler"))
                    return true;
            }

            return false;
        }

        private bool GetIdentity(out Account? acc, string szProxy)
        {
            string response;
            CookieCollection out_cookies;
            bool ret;

            if (form.checkBox1.Checked)
                ret = Http.HttpProxyGetRequest("http://identity.lima-city.de/", szProxy, out response, out out_cookies);
            else
                ret = Http.HttpGetRequest("http://identity.lima-city.de/", out response, out out_cookies);

            if(ret)
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(response);

                HtmlNode tbody = doc.DocumentNode.SelectSingleNode(".//table");
                IEnumerable<HtmlNode> trs = tbody.Descendants("tr");

                string Vorname = trs.ElementAt(1).SelectSingleNode("td[@class='td_val']").InnerHtml;
                string Nachname = trs.ElementAt(3).SelectSingleNode("td[@class='td_val']").InnerHtml;
                string[] Geboren = trs.ElementAt(4).SelectSingleNode("td[@class='td_val']").InnerHtml.Split('.');
                string[] Anschrift = Regex.Split(trs.ElementAt(5).SelectSingleNode("td[@class='td_val']").InnerHtml, "<br>");
                string[] plz_ort = Anschrift[1].Split(' ');
                string telefon = trs.ElementAt(6).SelectSingleNode("td[@class='td_val']").InnerHtml;
                string email = trs.ElementAt(7).SelectSingleNode("td[@class='td_val']/a").InnerHtml;

                acc = new Account(Vorname, Nachname, Geboren[0], Geboren[1], Geboren[2], plz_ort[0], Anschrift[0], plz_ort[1], telefon, email);

                return true;
            }

            acc = null;
            return false;
        }

        internal void RegistrationThread(int emailCount)
        {
            if (CProxy.GetProxyList())
            {
                SetRichTextBox(form.richTextBox1, "Proxy list obtained");
                int j = 0;

                for (int i = 0; i < emailCount; i++)
                {
                    string response;
                    CookieCollection cookies;

                    string proxy = CProxy.Proxies[j % CProxy.Proxies.Count].m_IP + ":" + CProxy.Proxies[j % CProxy.Proxies.Count].PORT;

                    if (Http.HttpGetRequest("http://www.ok.de/reg/", out response, out cookies))
                    {
                        SetRichTextBox(form.richTextBox1, "reg => Get-Request: success");

                        Image captcha;
                        CookieCollection PostCookies;
                        if (GetCaptcha(response, cookies, out captcha, out PostCookies, proxy))
                        {
                            SetRichTextBox(form.richTextBox1, "captcha => Get-Request: success");
                            //generate random data
                            Account? acc;
                            if (GetIdentity(out acc, proxy))
                            {
                                SetRichTextBox(form.richTextBox1, "identity obtained");

                                string user = GenerateRandom.RandomString(10);
                                string pass = GenerateRandom.RandomString(10);

                                string patrick;
                                if (ParsePatrick(response, out patrick))
                                {

                                    Account register = new Account(acc.Value.vorname, acc.Value.nachname, acc.Value.tag, acc.Value.monat, acc.Value.jahr,
                                                                    acc.Value.plz, acc.Value.strasse, acc.Value.ort, acc.Value.telefon, acc.Value.email,
                                                                    patrick, user, pass);
                                    SetText(form.textBox1, user);
                                    SetText(form.textBox2, pass);

                                    SetRichTextBox(form.richTextBox1, "random data generated");
                                    SetRichTextBox(form.richTextBox1, "waiting for user input");

                                    SetPictureBox(form.pictureBox1, captcha);
                                    SetTextboxEnabled(form.textBox3, true);

                                    semaphore.Wait(); //wait for user input
                                    SetRichTextBox(form.richTextBox1, "user input obtained");

                                    SetTextboxEnabled(form.textBox3, false);

                                    string captcha_challenge = form.textBox3.Text;

                                    //post request, finish registration
                                    if (FinishRegistration(register, captcha_challenge, PostCookies, proxy))
                                    {
                                        PushToDataBase(user, pass);
                                        SetRichTextBox(form.richTextBox1, "registration finished!");
                                        SetProgressBar(form.progressBar1, i + 1);
                                        SetLabel(form.label4, (i + 1).ToString() + "/" + emailCount);
                                    }
                                    else
                                    {
                                        SetRichTextBox(form.richTextBox1, "registration failed");
                                        i--;
                                        j++;
                                        continue;
                                    }

                                    SetText(form.textBox3, "");
                                    SetTextboxEnabled(form.textBox3, true);
                                }
                                else
                                {
                                    SetRichTextBox(form.richTextBox1, "could not parse patrick");
                                    i--;
                                    j++;
                                    continue;
                                }
                            }
                            else
                            {
                                SetRichTextBox(form.richTextBox1, "Could not obtain identity");
                                i--;
                                j++;
                                continue;
                            }

                        }
                        else
                        {
                            SetRichTextBox(form.richTextBox1, "Could not obtain captcha");
                            i--;
                            j++;
                            continue;
                        }
                    }
                    else
                    {
                        SetRichTextBox(form.richTextBox1, "ok.de/reg get failed");
                        i--;
                        j++;
                        continue;
                    }
                }
            }
            else
            {
                SetRichTextBox(form.richTextBox1, "Could not obtain proxy list");
            }

            //fertig
            SetNumericEnabled(form.numericUpDown1, false);
            SetButtonText(form.button1, "Start");
            SetButtonEnabled(form.button1, true);
        }

        delegate void SetTextDelegate(TextBox control, string sztext);

        void SetText(TextBox control, string sztext)
        {
            if(control.InvokeRequired)
            {
                control.Invoke(new SetTextDelegate(SetText), new object[] { control, sztext });
            }
            else
            {
                control.Text = sztext;
            }
        }

        delegate void SetButtonTextDelegate(Button control, string sztext);

        void SetButtonText(Button control, string sztext)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetButtonTextDelegate(SetButtonText), new object[] { control, sztext });
            }
            else
            {
                control.Text = sztext;
            }
        }

        delegate void SetNumericEnabledDelegate(NumericUpDown control, bool sztext);

        void SetNumericEnabled(NumericUpDown control, bool sztext)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetNumericEnabledDelegate(SetNumericEnabled), new object[] { control, sztext });
            }
            else
            {
                control.ReadOnly = false;
            }
        }

        delegate void SetButtonEnabledDelegate(Button control, bool sztext);

        void SetButtonEnabled(Button control, bool sztext)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetButtonEnabledDelegate(SetButtonEnabled), new object[] { control, sztext });
            }
            else
            {
                control.Enabled = sztext;
            }
        }

        delegate void SetTextboxEnabledDelegate(TextBox control, bool sztext);

        void SetTextboxEnabled(TextBox control, bool sztext)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetTextboxEnabledDelegate(SetTextboxEnabled), new object[] { control, sztext });
            }
            else
            {
                control.Enabled = sztext;
            }
        }

        delegate void SetProgressBarDelegate(ProgressBar bar, int value);

        void SetProgressBar(ProgressBar bar, int value)
        {
            if (bar.InvokeRequired)
            {
                bar.Invoke(new SetProgressBarDelegate(SetProgressBar), new object[] { bar, value });
            }
            else
            {
                bar.Value = value;
            }
        }

        delegate void SetLabelDelegate(Label bar, string value);

        void SetLabel(Label bar, string value)
        {
            if (bar.InvokeRequired)
            {
                bar.Invoke(new SetLabelDelegate(SetLabel), new object[] { bar, value });
            }
            else
            {
                bar.Text = value;
            }
        }

        delegate void SetRichTextBoxDelegate(RichTextBox bar, string value);

        void SetRichTextBox(RichTextBox bar, string value)
        {
            if (bar.InvokeRequired)
            {
                bar.Invoke(new SetRichTextBoxDelegate(SetRichTextBox), new object[] { bar, value });
            }
            else
            {
                bar.Text += value + "\n";
                bar.SelectionStart = bar.Text.Length;
                bar.ScrollToCaret();
            }
        }

        delegate void SetPictureBoxDelegate(PictureBox bar, Image value);

        void SetPictureBox(PictureBox bar, Image value)
        {
            if (bar.InvokeRequired)
            {
                bar.Invoke(new SetPictureBoxDelegate(SetPictureBox), new object[] { bar, value });
            }
            else
            {
                bar.Image = value;
            }
        }
    }
}
