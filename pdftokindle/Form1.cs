using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;

namespace pdftokindle
{
    public partial class Form1 : Form
    {
        List<string> url = new List<string>();
        List<string> urls = new List<string>();
        List<string> paths = new List<string>();
        List<string> files = new List<string>();
        string next = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (string i in paths)
            {
                try
                {
                    SmtpClient mailServer = new SmtpClient("smtp.gmail.com", 587);
                    mailServer.EnableSsl = true;
                    mailServer.Credentials = new System.Net.NetworkCredential("someone@gmail.com", "something");

                    string from = "timjonathanalber@gmail.com";
                    //string to = "gaby.alber_18@kindle.com";
                    string to = "t.planet@web.de";
                    MailMessage msg = new MailMessage(from, to);
                    msg.Subject = "";
                    msg.Body = "";
                    msg.Attachments.Add(new Attachment(i));
                    mailServer.Send(msg);
                    //textBox1.Text = textBox1.Text + " Done";
                }
                catch (Exception ex)
                {
                    //textBox1.Text = ex.ToString();
                    Console.WriteLine("Unable to send email. Error : " + ex);
                }
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            WebClient wc = new WebClient();
            string curent = wc.DownloadString("http://www.gutenberg.org/robot/harvest?filetypes[]=html&langs[]=de");
            Console.WriteLine("dwnled");
            next = Regex.Match(curent, @"(<a.*?>Next Page</a>)").Groups[1].Value;
            Regex ofset = new Regex(@"[^\d]");
            next =  ofset.Replace(next, "");
            Console.WriteLine("next: "+next);
            foreach (LinkItem i in LinkFinder.Find(curent))
            {
               string name = loadbookstolist(i);
               listBox1.Items.Add(name);
            }
            listBox1.SelectionMode = SelectionMode.One;
            listBox2.SelectionMode = SelectionMode.One;
            
        }
        public string loadbookstolist(LinkItem i)
        {
            WebClient wc = new WebClient();
            string num = Regex.Split(Regex.Split(i.ToString(), " ")[0], " /n ")[0];
            Console.WriteLine(num);
            //string[] all = Regex.Split(num, "/");
            Console.WriteLine("all: " + Regex.Split(Regex.Split(Regex.Split(i.ToString(), " ")[0], " /n ")[0], "/")[7]);
            string all = Regex.Split(Regex.Split(Regex.Split(i.ToString(), " ")[0], " /n ")[0], "/")[7];
            url.Add(num);
            Console.WriteLine("http://www.gutenberg.org/ebooks/search/?query=%" + all);
            string name = wc.DownloadString("http://www.gutenberg.org/ebooks/search/?query=%" + all);
            name = Regex.Match(name, @"<span class=""title"">.*?</span>").Groups[1].Value;
            Console.WriteLine("name : "+ name);
            return name;
            //listBox1.Items.Add(name);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (string i in urls)
            {
                WebClient wc = new WebClient();
                string p = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), ".zip");
                Console.WriteLine(p);
                wc.DownloadFile(i,p);
                string p2 = Path.Combine(Path.GetTempPath(), "/extracted/", Path.GetRandomFileName(), ".html");
                System.IO.Compression.ZipFile.ExtractToDirectory(p,p2);
                paths.Add(p2);
                Console.WriteLine(p2);
            }
        }

        private void listbox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Regex.Split(i.ToString(), "] [")[0]
            urls.Add(url[listBox1.SelectedIndex]);
            listBox2.Items.Add(url[listBox1.SelectedIndex] + Environment.NewLine);
        }

        private void listbox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newurl = urls[listBox2.SelectedIndex].ToString() + ".html.images";
            newurl = Regex.Replace(newurl, "//", "http://");
            Console.WriteLine("list click: " + newurl);
            webBrowser1.Navigate(new System.Uri(newurl));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WebClient wc = new WebClient();
            string curent = wc.DownloadString("http://www.gutenberg.org/robot/harvest?offset="+next+"&filetypes[]=html&langs[]=de");
            Console.WriteLine("dwnled");
            next = Regex.Match(curent, @"(<a.*?>Next Page</a>)").Groups[1].Value;
            Regex ofset = new Regex(@"[^\d]");
            next = ofset.Replace(next, "");
            Console.WriteLine("next: " + next);
            foreach (LinkItem i in LinkFinder.Find(curent))
            {
                loadbookstolist(i);
            }
            listBox1.SelectionMode = SelectionMode.One;
            listBox2.SelectionMode = SelectionMode.One;
        }
    }
    public struct LinkItem
    {
        public string Href;
        public string Text;

        public override string ToString()
        {
            return Href + " \n " + Text;
        }
    }

    static class LinkFinder
    {
        public static List<LinkItem> Find(string file)
        {
            List<LinkItem> list = new List<LinkItem>();

            // 1.
            // Find all matches in file.
            MatchCollection m1 = Regex.Matches(file, @"(<a.*?>http://www.gutenberg.lib.md.us/.*?</a>)",
                RegexOptions.Singleline);

            // 2.
            // Loop over each match.
            foreach (Match m in m1)
            {
                string value = m.Groups[1].Value;
                LinkItem i = new LinkItem();

                // 3.
                // Get href attribute.
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                RegexOptions.Singleline);
                if (m2.Success)
                {
                    i.Href = m2.Groups[1].Value;
                }

                // 4.
                // Remove inner tags from text.
                string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                RegexOptions.Singleline);
                i.Text = t;

                list.Add(i);
            }
            return list;
        }
    }
}
